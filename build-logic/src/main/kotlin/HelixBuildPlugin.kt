import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.api.tasks.Copy
import java.io.File
import java.nio.charset.StandardCharsets
import java.util.UUID

@DslMarker
annotation class HelixBuildDsl

@HelixBuildDsl
open class MonorepoSolutionSpec(
    val path: String,
    internal val folderName: String? = null,
    internal val includeUnselectedProjects: Boolean = true,
    internal val emitSelectedProjects: Boolean = true,
) {
    internal var includedProjects: MutableSet<String>? = null

    fun project(vararg names: String) {
        includedProjects = (includedProjects ?: mutableSetOf()).apply { addAll(names) }
    }

    fun project(name: String) {
        includedProjects = (includedProjects ?: mutableSetOf()).apply { add(name) }
    }
}

@HelixBuildDsl
open class SolutionsExtension {
    internal val solutions = mutableListOf<MonorepoSolutionSpec>()

    fun solution(path: String) {
        solutions += MonorepoSolutionSpec(path)
    }

    fun solution(path: String, configure: MonorepoSolutionSpec.() -> Unit) {
        solutions += MonorepoSolutionSpec(path).apply(configure)
    }
}

private data class SolutionProject(
    val typeId: String,
    val name: String,
    val path: String,
    val id: String,
    val lines: List<String>,
)

private data class ParsedSolution(
    val file: File,
    val projects: List<SolutionProject>,
    val solutionConfigurations: List<String>,
    val projectConfigurations: List<String>,
    val nestedProjects: List<String>,
)

private data class SolutionFolder(val name: String, val id: String)

class HelixBuildPlugin : Plugin<Project> {
    override fun apply(project: Project) {
        val solutions = project.extensions.create("solutions", SolutionsExtension::class.java)
        val unityPackages = project.extensions.create("unity", UnityExtension::class.java)
        unityPackages.projectRoot = project.providers.gradleProperty("UnityProjectRoot").orNull.orEmpty()
        project.afterEvaluate {
            unityPackages.validate()
        }

        project.tasks.register("generateMonorepoSolution") {
            group = "helix"
            description = "Generates HELIX.sln from the registered component solutions."
            inputs.files(project.provider {
                (solutions.solutions + unityPackages.asSolutionSpecs(project))
                    .map { project.layout.projectDirectory.file(it.path) } +
                    unityPackages.packages.flatMap { packageSpec ->
                        unityPackages.packageDirectory(project, packageSpec).walkTopDown()
                            .filter { it.isFile && it.extension == "asmdef" }
                            .toList()
                    }
            })
            inputs.property("solutionConfiguration", project.provider {
                (solutions.solutions + unityPackages.asSolutionSpecs(project)).associate { solution ->
                    "${solution.path}:${solution.folderName.orEmpty()}" to solution.includedProjects?.sorted().orEmpty()
                }
            })
            outputs.file(project.layout.projectDirectory.file("HELIX.sln"))
            doLast {
                generateMonorepoSolution(project, solutions.solutions + unityPackages.asSolutionSpecs(project))
            }
        }

        project.tasks.register("syncGrammars", Copy::class.java) {
            group = "helix"
            description = "Copies TextMate grammars into the documentation project."
            from(project.layout.projectDirectory.dir("src/Grammars/mixin_expressions/syntaxes")) {
                include("*.tmLanguage.json")
            }
            into(project.layout.projectDirectory.dir("docs/grammars"))
        }

        registerUnityPackageTasks(project, unityPackages)
    }
}

private const val solutionFolderType = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}"
private val projectHeader = Regex("""^Project\("([^"]+)"\) = "([^"]+)", "([^"]+)", "([^"]+)"$""")
private val nestedEntry = Regex("""^\s*(\{[^}]+})\s*=\s*(\{[^}]+})\s*$""")
private val configurationEntry = Regex("""^\s*(\{[^}]+})\.""")
private val solutionItem = Regex("""^(\s*[^=]+?\s*=\s*)(.+?)\s*$""")

private fun solutionSection(lines: List<String>, name: String): List<String> {
    val start = lines.indexOfFirst { it.trimStart().startsWith("GlobalSection($name)") }
    if (start < 0) return emptyList()
    val end = (start + 1 until lines.size).firstOrNull { lines[it].trim() == "EndGlobalSection" }
        ?: error("Unterminated $name section")
    return lines.subList(start + 1, end)
}

private fun parseSolution(file: File): ParsedSolution {
    require(file.isFile) { "Registered solution does not exist: $file" }
    val lines = file.readText(StandardCharsets.UTF_8).removePrefix("\uFEFF").lines()
    val projects = mutableListOf<SolutionProject>()
    var index = 0
    while (index < lines.size) {
        val match = projectHeader.matchEntire(lines[index])
        if (match == null) {
            index++
            continue
        }
        val end = (index + 1 until lines.size).firstOrNull { lines[it] == "EndProject" }
            ?: error("Unterminated project in $file at line ${index + 1}")
        val (typeId, name, path, id) = match.destructured
        projects += SolutionProject(typeId, name, path, id, lines.subList(index, end + 1))
        index = end + 1
    }
    return ParsedSolution(
        file,
        projects,
        solutionSection(lines, "SolutionConfigurationPlatforms"),
        solutionSection(lines, "ProjectConfigurationPlatforms"),
        solutionSection(lines, "NestedProjects"),
    )
}

private fun rebasedPath(root: File, sourceDirectory: File, path: String): String {
    val resolved = sourceDirectory.toPath().resolve(path.replace('\\', '/')).normalize()
    return root.toPath().relativize(resolved).toString().replace('/', '\\')
}

private fun rebaseProject(root: File, solution: ParsedSolution, project: SolutionProject): List<String> {
    val sourceDirectory = solution.file.parentFile
    return project.lines.mapIndexed { index, line ->
        when {
            index == 0 && project.typeId.equals(solutionFolderType, ignoreCase = true) -> line
            index == 0 -> "Project(\"${project.typeId}\") = \"${project.name}\", \"${
                rebasedPath(root, sourceDirectory, project.path)
            }\", \"${project.id}\""
            line.firstOrNull()?.isWhitespace() == true &&
                line.contains(" = ") &&
                project.typeId.equals(solutionFolderType, ignoreCase = true) -> {
                val match = solutionItem.matchEntire(line)
                if (match == null) line
                else match.groupValues[1] + rebasedPath(root, sourceDirectory, match.groupValues[2])
            }
            else -> line
        }
    }
}

private fun createSolutionFolder(name: String, key: String) = SolutionFolder(
    name,
    "{${UUID.nameUUIDFromBytes("generate-monorepo-sln:$key".toByteArray(StandardCharsets.UTF_8))}}".uppercase(),
)

private fun solutionFolderBlock(folder: SolutionFolder) = listOf(
    "Project(\"$solutionFolderType\") = \"${folder.name}\", \"${folder.name}\", \"${folder.id}\"",
    "EndProject",
)

private fun generateMonorepoSolution(project: Project, registeredSolutions: List<MonorepoSolutionSpec>) {
    val root = project.rootProject.projectDir.canonicalFile
    val output = File(root, "HELIX.sln")
    val parsed = registeredSolutions.map { entry -> entry to parseSolution(File(root, entry.path)) }
    val emittedIds = linkedSetOf<String>()
    val emittedProjects = mutableListOf<Pair<ParsedSolution, SolutionProject>>()
    val solutionConfigurations = linkedSetOf<String>()
    val projectConfigurations = mutableListOf<String>()
    val nestedProjects = mutableListOf<String>()
    val syntheticFolders = linkedMapOf<String, SolutionFolder>()
    var excludedProjectCount = 0

    for ((entry, solution) in parsed) {
        val byId = solution.projects.associateBy { it.id.uppercase() }
        val visible = solution.projects
            .filter { entry.includedProjects == null || it.name in entry.includedProjects!! }
            .mapTo(linkedSetOf()) { it.id.uppercase() }

        do {
            var changed = false
            for (line in solution.nestedProjects) {
                val match = nestedEntry.matchEntire(line) ?: continue
                val child = match.groupValues[1].uppercase()
                val parent = match.groupValues[2].uppercase()
                if (child in visible && parent !in visible && byId[parent]?.typeId.equals(solutionFolderType, true)) {
                    visible += parent
                    changed = true
                }
            }
        } while (changed)

        val excluded = if (entry.includeUnselectedProjects) {
            solution.projects
                .filterNot { it.typeId.equals(solutionFolderType, ignoreCase = true) }
                .mapTo(linkedSetOf()) { it.id.uppercase() }
                .minus(visible)
        } else {
            emptySet()
        }
        val emitted = (if (entry.emitSelectedProjects) visible else emptySet()) + excluded

        for (solutionProject in solution.projects.filter { it.id.uppercase() in emitted }) {
            check(emittedIds.add(solutionProject.id.uppercase())) {
                "Duplicate project GUID ${solutionProject.id} (${solutionProject.name}) in ${solution.file}"
            }
            emittedProjects += solution to solutionProject
        }
        solutionConfigurations += solution.solutionConfigurations
        projectConfigurations += solution.projectConfigurations.filter { line ->
            configurationEntry.find(line)?.groupValues?.get(1)?.uppercase() in emitted
        }
        val visibleNesting = solution.nestedProjects.filter { line ->
            val match = nestedEntry.matchEntire(line)
            match != null && match.groupValues[1].uppercase() in visible && match.groupValues[2].uppercase() in visible
        }
        nestedProjects += visibleNesting

        if (entry.emitSelectedProjects && visible.isNotEmpty()) {
            val folderKey = "${entry.path}:${entry.folderName.orEmpty()}"
            val sourceFolder = createSolutionFolder(
                entry.folderName ?: solution.file.nameWithoutExtension,
                "folder:projects:$folderKey",
            )
            syntheticFolders["projects:$folderKey"] = sourceFolder
            val nestedVisibleProjects = visibleNesting.mapNotNullTo(linkedSetOf()) { line ->
                nestedEntry.matchEntire(line)?.groupValues?.get(1)?.uppercase()
            }
            for (projectId in visible - nestedVisibleProjects) {
                nestedProjects += "\t\t${byId.getValue(projectId).id} = ${sourceFolder.id}"
            }
        }

        if (excluded.isNotEmpty()) {
            val excludedRoot = syntheticFolders.getOrPut("excluded") {
                createSolutionFolder("Excluded Projects", "folder:excluded")
            }
            val sourceFolder = createSolutionFolder(solution.file.nameWithoutExtension, "folder:excluded:${entry.path}")
            check(sourceFolder.id.uppercase() !in emittedIds) {
                "Synthetic solution-folder GUID ${sourceFolder.id} conflicts with a project in ${solution.file}"
            }
            syntheticFolders["excluded:${entry.path}"] = sourceFolder
            nestedProjects += "\t\t${sourceFolder.id} = ${excludedRoot.id}"
            for (projectId in excluded) {
                nestedProjects += "\t\t${byId.getValue(projectId).id} = ${sourceFolder.id}"
            }
            excludedProjectCount += excluded.size
        }
    }

    val orderedSyntheticFolders = syntheticFolders.entries
        .sortedBy { (key, _) -> if (key == "excluded") 1 else 0 }
        .map { it.value }
    for (folder in orderedSyntheticFolders) {
        check(emittedIds.add(folder.id.uppercase())) {
            "Duplicate synthetic solution-folder GUID ${folder.id} (${folder.name})"
        }
    }

    val result = buildList {
        add("Microsoft Visual Studio Solution File, Format Version 12.00")
        for ((solution, solutionProject) in emittedProjects) addAll(rebaseProject(root, solution, solutionProject))
        for (folder in orderedSyntheticFolders) addAll(solutionFolderBlock(folder))
        add("Global")
        if (solutionConfigurations.isNotEmpty()) {
            add("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution")
            addAll(solutionConfigurations)
            add("\tEndGlobalSection")
        }
        if (projectConfigurations.isNotEmpty()) {
            add("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution")
            addAll(projectConfigurations)
            add("\tEndGlobalSection")
        }
        if (nestedProjects.isNotEmpty()) {
            add("\tGlobalSection(NestedProjects) = preSolution")
            addAll(nestedProjects)
            add("\tEndGlobalSection")
        }
        add("EndGlobal")
    }

    output.writeText("\uFEFF" + result.joinToString("\r\n", postfix = "\r\n"), StandardCharsets.UTF_8)
    project.logger.lifecycle(
        "Generated ${output.relativeTo(root)} from ${registeredSolutions.size} registered solutions " +
            "(${emittedProjects.size} projects, $excludedProjectCount remapped under Excluded Projects)."
    )
}

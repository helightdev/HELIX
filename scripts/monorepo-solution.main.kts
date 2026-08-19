import java.io.File
import java.nio.charset.StandardCharsets
import java.util.UUID

data class RegisteredSolution(
    val path: String,
    // null keeps every project at its original location; otherwise these are the
    // exact project names kept visible at their original location. Other real
    // projects remain in the generated solution under Excluded Projects.
    val projects: Set<String>? = null,
)

// This is the registry. Add a solution here, or adjust its project allow-list.
val registry = listOf(
    RegisteredSolution("src/RiderPlugin/HelixRider.sln"),
    RegisteredSolution("src/HelixSourceGenerator/HelixSourceGenerator.sln"),
    RegisteredSolution(
        "src/HELIX/HELIX.sln",
        setOf(
            "HELIX",
            "HELIX.Compose",
            "HELIX.Compose.Editor",
            "HELIX.Context",
            "HELIX.Context.Tests",
            "HELIX.UI",
            "HELIX.Support.Odin",
            "HELIX.Tests",
            "HELIX.Devtools.Editor.Odin",
            "HELIX.Devtools.Editor",
            "HELIX.Devtools",
            "HELIX.Devtools.Odin",
        ),
    ),
)

val solutionFolderType = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}"
val projectHeader = Regex("""^Project\("([^"]+)"\) = "([^"]+)", "([^"]+)", "([^"]+)"$""")
val nestedEntry = Regex("""^\s*(\{[^}]+})\s*=\s*(\{[^}]+})\s*$""")
val configurationEntry = Regex("""^\s*(\{[^}]+})\.""")
val solutionItem = Regex("""^(\s*[^=]+?\s*=\s*)(.+?)\s*$""")

data class ProjectBlock(
    val typeId: String,
    val name: String,
    val path: String,
    val id: String,
    val lines: List<String>,
)

data class ParsedSolution(
    val file: File,
    val projects: List<ProjectBlock>,
    val solutionConfigurations: List<String>,
    val projectConfigurations: List<String>,
    val nestedProjects: List<String>,
)

data class SolutionFolder(
    val name: String,
    val id: String,
)

fun section(lines: List<String>, name: String): List<String> {
    val start = lines.indexOfFirst { it.trimStart().startsWith("GlobalSection($name)") }
    if (start < 0) return emptyList()
    val end = (start + 1 until lines.size).firstOrNull { lines[it].trim() == "EndGlobalSection" }
        ?: error("Unterminated $name section")
    return lines.subList(start + 1, end)
}

fun parse(file: File): ParsedSolution {
    require(file.isFile) { "Registered solution does not exist: $file" }
    val lines = file.readText(StandardCharsets.UTF_8).removePrefix("\uFEFF").lines()
    val projects = mutableListOf<ProjectBlock>()
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
        projects += ProjectBlock(typeId, name, path, id, lines.subList(index, end + 1))
        index = end + 1
    }
    return ParsedSolution(
        file,
        projects,
        section(lines, "SolutionConfigurationPlatforms"),
        section(lines, "ProjectConfigurationPlatforms"),
        section(lines, "NestedProjects"),
    )
}

fun rebasedPath(root: File, sourceDirectory: File, path: String): String {
    val resolved = sourceDirectory.toPath().resolve(path.replace('\\', '/')).normalize()
    return root.toPath().relativize(resolved).toString().replace('/', '\\')
}

fun rebaseBlock(root: File, solution: ParsedSolution, project: ProjectBlock): List<String> {
    val sourceDirectory = solution.file.parentFile
    return project.lines.mapIndexed { index, line ->
        when {
            index == 0 && project.typeId.equals(solutionFolderType, ignoreCase = true) -> line
            index == 0 -> "Project(\"${project.typeId}\") = \"${project.name}\", \"${
                rebasedPath(
                    root,
                    sourceDirectory,
                    project.path
                )
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

fun solutionFolder(name: String, key: String) = SolutionFolder(
    name,
    "{${UUID.nameUUIDFromBytes("generate-monorepo-sln:$key".toByteArray(StandardCharsets.UTF_8))}}".uppercase(),
)

fun solutionFolderBlock(folder: SolutionFolder) = listOf(
    "Project(\"$solutionFolderType\") = \"${folder.name}\", \"${folder.name}\", \"${folder.id}\"",
    "EndProject",
)

val root = File(args.firstOrNull() ?: ".").canonicalFile
val output = File(root, "HELIX.sln")
val parsed = registry.map { entry -> entry to parse(File(root, entry.path)) }

val emittedIds = linkedSetOf<String>()
val emittedProjects = mutableListOf<Pair<ParsedSolution, ProjectBlock>>()
val solutionConfigurations = linkedSetOf<String>()
val projectConfigurations = mutableListOf<String>()
val nestedProjects = mutableListOf<String>()
val syntheticFolders = linkedMapOf<String, SolutionFolder>()
var excludedProjectCount = 0

for ((entry, solution) in parsed) {
    val byId = solution.projects.associateBy { it.id.uppercase() }
    val visible = solution.projects
        .filter { entry.projects == null || it.name in entry.projects }
        .mapTo(linkedSetOf()) { it.id.uppercase() }

    // Keep any solution-folder ancestors needed by visible projects.
    var changed: Boolean
    do {
        changed = false
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

    val excluded = if (entry.projects == null) emptySet() else solution.projects
        .filterNot { it.typeId.equals(solutionFolderType, ignoreCase = true) }
        .mapTo(linkedSetOf()) { it.id.uppercase() }
        .minus(visible)
    val emitted = visible + excluded

    for (project in solution.projects.filter { it.id.uppercase() in emitted }) {
        check(emittedIds.add(project.id.uppercase())) {
            "Duplicate project GUID ${project.id} (${project.name}) in ${solution.file}"
        }
        emittedProjects += solution to project
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

    if (visible.isNotEmpty()) {
        val sourceFolder = solutionFolder(solution.file.nameWithoutExtension, "folder:projects:${entry.path}")
        syntheticFolders["projects:${entry.path}"] = sourceFolder

        val nestedVisibleProjects = visibleNesting.mapNotNullTo(linkedSetOf()) { line ->
            nestedEntry.matchEntire(line)?.groupValues?.get(1)?.uppercase()
        }
        for (projectId in visible - nestedVisibleProjects) {
            nestedProjects += "\t\t${byId.getValue(projectId).id} = ${sourceFolder.id}"
        }
    }

    if (excluded.isNotEmpty()) {
        val excludedRoot = syntheticFolders.getOrPut("excluded") {
            solutionFolder("Excluded Projects", "folder:excluded")
        }
        val sourceFolder = solutionFolder(solution.file.nameWithoutExtension, "folder:excluded:${entry.path}")
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
    for ((solution, project) in emittedProjects) addAll(rebaseBlock(root, solution, project))
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
println(
    "Generated ${output.relativeTo(root)} from ${registry.size} registered solutions " +
        "(${emittedProjects.size} projects, $excludedProjectCount remapped under Excluded Projects)."
)

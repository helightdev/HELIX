import org.gradle.api.DefaultTask
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.tasks.InputFile
import org.gradle.api.tasks.OutputFile
import org.gradle.api.tasks.TaskAction
import org.gradle.api.Plugin
import org.gradle.api.Project
import org.gradle.api.tasks.Exec
import org.gradle.api.tasks.Copy
import org.gradle.api.tasks.JavaExec
import java.io.File
import java.nio.charset.StandardCharsets
import java.util.Base64
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
            inputs.property("registeredSolutions", project.provider {
                (solutions.solutions + unityPackages.asSolutionSpecs(project)).map(::encodeSolutionSpec)
            })
            outputs.file(project.layout.projectDirectory.file("HELIX.sln"))
            doLast {
                val output = outputs.files.singleFile
                val registeredSolutions = (inputs.properties.getValue("registeredSolutions") as List<*>)
                    .map { decodeSolutionSpec(it as String) }
                val summary = generateMonorepoSolution(output.parentFile.canonicalFile, output, registeredSolutions)
                logger.lifecycle(summary)
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

        registerHixBuildTasks(project)
        registerHixGrammarTasks(project)

        registerUnityPackageTasks(project, unityPackages)
    }
}

private fun registerHixGrammarTasks(project: Project) {
    project.repositories.mavenCentral()
    val antlrTool = project.configurations.create("antlrTool")
    project.dependencies.add(antlrTool.name, "org.antlr:antlr4:4.13.2")

    val grammarDirectory = project.layout.projectDirectory.dir("src/Grammars/hix")
    val csharpOutput = project.layout.projectDirectory.dir("src/Hix/Language/Compiler/Generated")
    val ideOutput = project.layout.projectDirectory.dir(
        "src/RiderPlugin/src/rider/main/java/dev/helight/helix/hix/generated",
    )
    val grammarFiles = listOf(grammarDirectory.file("HixLexer.g4"), grammarDirectory.file("HixParser.g4"))

    val csharp = project.tasks.register("generateHixCSharpGrammar", JavaExec::class.java) {
        group = "code generation"
        description = "Generates the C# lexer and parser for the Hix language."
        classpath = antlrTool
        mainClass.set("org.antlr.v4.Tool")
        workingDir = grammarDirectory.asFile
        inputs.files(grammarFiles)
        outputs.files(
            csharpOutput.file("HixLexer.cs"),
            csharpOutput.file("HixParser.cs"),
            csharpOutput.file("HixParserBaseVisitor.cs"),
            csharpOutput.file("HixParserVisitor.cs"),
        )
        args(
            "-Dlanguage=CSharp", "-visitor", "-no-listener",
            "-package", "Hix.Compiler.Generated",
            "-o", csharpOutput.asFile.absolutePath,
            "-lib", csharpOutput.asFile.absolutePath,
            "HixLexer.g4", "HixParser.g4",
        )
        doLast { normalizeGeneratedSources(csharpOutput.asFile, "cs") }
    }
    val ide = project.tasks.register("generateHixIdeGrammar", JavaExec::class.java) {
        group = "code generation"
        description = "Generates the Rider lexer and parser for the Hix language."
        classpath = antlrTool
        mainClass.set("org.antlr.v4.Tool")
        workingDir = grammarDirectory.asFile
        inputs.files(grammarFiles)
        outputs.files(
            ideOutput.file("HixLexer.java"),
            ideOutput.file("HixParser.java"),
            ideOutput.file("HixParserBaseVisitor.java"),
            ideOutput.file("HixParserVisitor.java"),
        )
        args(
            "-Dlanguage=Java", "-visitor", "-no-listener",
            "-package", "dev.helight.helix.hix.generated",
            "-o", ideOutput.asFile.absolutePath,
            "-lib", ideOutput.asFile.absolutePath,
            "HixLexer.g4", "HixParser.g4",
        )
        doLast { normalizeGeneratedSources(ideOutput.asFile, "java") }
    }
    project.tasks.register("generateHixGrammar") {
        group = "code generation"
        description = "Generates all ANTLR outputs for the Hix language."
        dependsOn(csharp, ide)
    }
    project.gradle.projectsEvaluated {
        project.findProject(":riderPlugin")?.tasks?.matching {
            it.name == "compileKotlin" || it.name == "compileJava"
        }?.configureEach { dependsOn(ide) }
        project.findProject(":riderPlugin")?.tasks?.matching {
            it.name == "compileDotNet"
        }?.configureEach { dependsOn(csharp) }
    }
}

private fun normalizeGeneratedSources(directory: File, extension: String) {
    directory.listFiles { file -> file.isFile && file.extension == extension }?.forEach { file ->
        val source = file.readText(StandardCharsets.UTF_8)
        val normalized = source.replace(Regex("[ \\t]+(?=\\r?$)", RegexOption.MULTILINE), "")
        if (normalized != source) file.writeText(normalized, StandardCharsets.UTF_8)
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

private fun encodeSolutionSpec(spec: MonorepoSolutionSpec): String {
    val values = listOf(
        spec.path,
        spec.folderName.orEmpty(),
        spec.includeUnselectedProjects.toString(),
        spec.emitSelectedProjects.toString(),
        spec.includedProjects?.sorted()?.joinToString("\u0000").orEmpty(),
        (spec.includedProjects != null).toString(),
    )
    return values.joinToString(".") { value ->
        Base64.getUrlEncoder().withoutPadding().encodeToString(value.toByteArray(StandardCharsets.UTF_8))
    }
}

private fun decodeSolutionSpec(encoded: String): MonorepoSolutionSpec {
    val values = encoded.split('.').map { value ->
        String(Base64.getUrlDecoder().decode(value), StandardCharsets.UTF_8)
    }
    require(values.size == 6) { "Invalid registered solution input" }
    return MonorepoSolutionSpec(
        path = values[0],
        folderName = values[1].ifEmpty { null },
        includeUnselectedProjects = values[2].toBooleanStrict(),
        emitSelectedProjects = values[3].toBooleanStrict(),
    ).apply {
        if (values[5].toBooleanStrict()) {
            includedProjects = values[4].split('\u0000').filterTo(mutableSetOf()) { it.isNotEmpty() }
        }
    }
}

private fun generateMonorepoSolution(
    root: File,
    output: File,
    registeredSolutions: List<MonorepoSolutionSpec>,
): String {
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
    return "Generated ${output.relativeTo(root)} from ${registeredSolutions.size} registered solutions " +
        "(${emittedProjects.size} projects, $excludedProjectCount remapped under Excluded Projects)."
}

private fun registerHixBuildTasks(project: Project) {
    val configuration = project.providers.gradleProperty("BuildConfiguration").orElse("Debug")
    val generatorConfiguration = project.providers.gradleProperty("HixGeneratorConfiguration").orElse(configuration)
    val build = project.tasks.register("buildHix", Exec::class.java) {
        group = "hix"
        description = "Builds the Hix libraries, standalone runtime, generator, and test projects."
        workingDir(project.layout.projectDirectory)
        commandLine("dotnet", "build", "src/Hix/Hix.sln", "--configuration", configuration.get())
    }
    project.tasks.register("testHix", Exec::class.java) {
        group = "hix"
        description = "Runs the core, Roslyn, standalone, and mixin generator test suites."
        dependsOn(build)
        workingDir(project.layout.projectDirectory)
        commandLine("dotnet", "test", "src/Hix/Hix.sln", "--configuration", configuration.get(), "--no-build")
    }
    val generator = project.tasks.register("buildHixMixinGenerator", Exec::class.java) {
        group = "hix"
        description = "Builds the self-contained Unity source generator."
        workingDir(project.layout.projectDirectory)
        commandLine("dotnet", "build", "src/Hix.MixinGenerator/Hix.MixinGenerator.csproj", "--configuration", generatorConfiguration.get())
    }
    project.tasks.register("copyHixMixinGenerator", HixGeneratorCopyTask::class.java) {
        group = "hix"
        description = "Builds and copies the bundled source generator DLL into the Unity project."
        dependsOn(generator)
        source.set(project.layout.projectDirectory.file(generatorConfiguration.map {
            "src/Hix.MixinGenerator/bin/$it/netstandard2.0/HelixSourceGenerator.dll"
        }))
        destination.set(project.layout.projectDirectory.file(project.providers.gradleProperty("UnityProjectRoot").orElse("src/HELIX").map {
            "$it/Assets/Plugins/HELIX/Runtime/Scripts/HelixSourceGenerator.dll"
        }))
    }
}

abstract class HixGeneratorCopyTask : DefaultTask() {
    @get:InputFile abstract val source: RegularFileProperty
    @get:OutputFile abstract val destination: RegularFileProperty
    @TaskAction fun copyGenerator() {
        val sourceFile = source.get().asFile
        val target = destination.get().asFile
        target.parentFile.mkdirs()
        sourceFile.copyTo(target, overwrite = true)
        check(sourceFile.readBytes().contentEquals(target.readBytes())) {
            "Copied Hix generator does not match ${sourceFile.absolutePath}"
        }
        logger.lifecycle("Deployed ${sourceFile.absolutePath} to ${target.absolutePath}")
    }
}

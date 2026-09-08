import com.jetbrains.plugin.structure.base.utils.isFile
import groovy.ant.FileNameFinder
import org.apache.tools.ant.taskdefs.condition.Os
import org.jetbrains.intellij.platform.gradle.Constants
import org.gradle.process.ExecOperations
import org.gradle.api.DefaultTask
import org.gradle.api.file.RegularFileProperty
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.provider.Property
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.InputFile
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.TaskAction
import java.io.ByteArrayOutputStream
import javax.inject.Inject

abstract class DotNetBuildTask @Inject constructor(
    private val execOperations: ExecOperations,
) : DefaultTask() {
    @get:InputFile
    abstract val solution: RegularFileProperty

    @get:Input
    abstract val buildConfiguration: Property<String>

    @get:Internal
    abstract val projectDirectory: DirectoryProperty

    @TaskAction
    fun build() {
        var buildExecutable = "dotnet"
        val buildArguments = mutableListOf("msbuild")

        if (Os.isFamily(Os.FAMILY_WINDOWS)) {
            val stdout = ByteArrayOutputStream()
            execOperations.exec {
                executable(projectDirectory.file("tools/vswhere.exe").get().asFile.absolutePath)
                args("-latest", "-property", "installationPath", "-products", "*")
                standardOutput = stdout
                workingDir(projectDirectory)
            }

            val directory = stdout.toString().trim()
            if (directory.isNotEmpty()) {
                val files = FileNameFinder().getFileNames("$directory\\MSBuild", "**/MSBuild.exe")
                buildExecutable = files.first()
                buildArguments.clear()
                buildArguments.add("/v:minimal")
            }
        }

        buildArguments.add(solution.get().asFile.absolutePath)
        buildArguments.add("/p:Configuration=${buildConfiguration.get()}")
        buildArguments.add("/p:HostFullIdentifier=")
        // Gradle invokes this task whenever the sandbox is prepared. Let MSBuild perform its
        // normal incremental checks instead of forcing every backend project and shared
        // language dependency to rebuild before each Rider launch.
        buildArguments.add("/t:Restore;Build")
        execOperations.exec {
            executable(buildExecutable)
            args(buildArguments)
            workingDir(projectDirectory)
        }
    }
}

abstract class ExecOperationsHolder @Inject constructor() {
    @get:Inject
    abstract val execOperations: ExecOperations
}

plugins {
    id("java")
    alias(libs.plugins.kotlinJvm)
    alias(libs.plugins.kotlinSerialization)
    id("org.jetbrains.intellij.platform") version "2.18.0"     // See https://github.com/JetBrains/intellij-platform-gradle-plugin/releases
}

val isWindows = Os.isFamily(Os.FAMILY_WINDOWS)
extra["isWindows"] = isWindows

val execOperations = objects.newInstance<ExecOperationsHolder>().execOperations

val DotnetSolution = providers.gradleProperty("DotnetSolution")
val BuildConfiguration = providers.gradleProperty("BuildConfiguration")
val ProductVersion = providers.gradleProperty("ProductVersion")
val DotnetPluginId = providers.gradleProperty("DotnetPluginId")
val RiderPluginId = providers.gradleProperty("RiderPluginId")
val PublishToken = providers.gradleProperty("PublishToken")

allprojects {
    repositories {
        maven { setUrl("https://cache-redirector.jetbrains.com/maven-central") }
    }
}

repositories {
    intellijPlatform {
        defaultRepositories()
        jetbrainsRuntime()
    }
}

version = extra["PluginVersion"] as String

tasks.processResources {
    from("dependencies.json") { into("META-INF") }
    // Rider loads ReSharper settings from this exact path inside the IntelliJ plugin JAR.
    // Embedding a DotSettings file in the backend assembly alone does not register it as a layer.
    from("src/dotnet/${DotnetPluginId.get()}/HelixLiveTemplates.DotSettings") {
        into("dotnet/Extensions/${RiderPluginId.get()}/settings")
    }
}

sourceSets {
    main {
        java.srcDir("src/rider/main/java")
        kotlin.srcDir("src/rider/main/kotlin")
        resources.srcDir("src/rider/main/resources")
    }
    test {
        kotlin.srcDir("src/rider/test/kotlin")
    }
}

val compileDotNet = tasks.register<DotNetBuildTask>("compileDotNet") {
    solution.set(layout.projectDirectory.file(DotnetSolution.get()))
    buildConfiguration.set(BuildConfiguration)
    projectDirectory.set(layout.projectDirectory)
}

val testDotNet = tasks.register("testDotNet") {
    doLast {
        execOperations.exec {
            executable("dotnet")
            args("test", file(DotnetSolution).absolutePath, "--logger", "GitHubActions")
            workingDir(projectDir)
        }
    }
}

tasks.buildPlugin {
    doLast {
        copy {
            from(layout.buildDirectory.file("distributions/${project.name}-${version}.zip"))
            into(layout.projectDirectory.dir("output"))
        }

        // TODO: See also org.jetbrains.changelog: https://github.com/JetBrains/gradle-changelog-plugin
        val changelogText = file("CHANGELOG.md").readText()
        val changelogMatches = Regex("(?s)(-.+?)(?=##|$)").findAll(changelogText)
        val changeNotes = changelogMatches.map {
            it.groups[1]!!.value.replace("(?s)- ".toRegex(), "\u2022 ").replace("`", "").replace(",", "%2C").replace(";", "%3B")
        }.take(1).joinToString()

        val executable = "dotnet"
        val arguments = mutableListOf("msbuild", file(DotnetSolution).absolutePath, "/p:Configuration=${BuildConfiguration.get()}", "/p:HostFullIdentifier=", "/t:Pack")
        arguments.add("/p:PackageOutputPath=${layout.projectDirectory.dir("output").asFile.absolutePath}")
        arguments.add("/p:PackageReleaseNotes=${changeNotes}")
        arguments.add("/p:PackageVersion=${version}")
        execOperations.exec {
            executable(executable)
            args(arguments)
            workingDir(projectDir)
        }
    }
}

dependencies {
    implementation(libs.kotlinxSerializationJson)
    implementation("org.antlr:antlr4-runtime:4.13.2")

    intellijPlatform {
        rider(ProductVersion) {
            useInstaller.set(false)
        }
        jetbrainsRuntime()
        bundledPlugin("com.intellij.resharper.unity")
        bundledPlugin("org.jetbrains.plugins.yaml")
        bundledModule("intellij.rd.client")
        bundledModule("intellij.rider.languages")
        bundledModule("intellij.rider.rdclient.dotnet")
        testFramework(org.jetbrains.intellij.platform.gradle.TestFrameworkType.Platform)
    }

    testImplementation(kotlin("test"))
}

tasks.test {
    systemProperty("unityExtensions.projectRoot", projectDir.absolutePath)
}

tasks.runIde {
    // Rider 2026.2 with the bundled Unity plugin exceeds the old 1.5 GB sandbox default while indexing.
    // Keep this scoped to the launched IDE; callers can lower or raise it with -PRiderMaxHeapSize=<size>.
    maxHeapSize = providers.gradleProperty("RiderMaxHeapSize").getOrElse("4g")
}

tasks.patchPluginXml {
    // TODO: See also org.jetbrains.changelog: https://github.com/JetBrains/gradle-changelog-plugin
    val changelogText = file("CHANGELOG.md").readText()
    val changelogMatches = Regex("(?s)(-.+?)(?=##|\$)").findAll(changelogText)

    changeNotes.set(changelogMatches.map {
        it.groups[1]!!.value.replace("(?s)\r?\n".toRegex(), "<br />\n")
    }.take(1).joinToString())
}

tasks.prepareSandbox {
    dependsOn(compileDotNet)

    // Keep the settings available beside the backend DLL in the development
    // sandbox too. Rider resolves bundled settings from the plugin JAR in a
    // packaged install, but the sandbox backend scans this physical location.
    from("src/dotnet/${DotnetPluginId.get()}/HelixLiveTemplates.DotSettings") {
        into("${project.name}/dotnet/Extensions/${RiderPluginId.get()}/settings")
    }

    val outputFolder = layout.projectDirectory.dir("src/dotnet/${DotnetPluginId.get()}/bin/${DotnetPluginId.get()}.Rider/${BuildConfiguration.get()}")
    val dllFiles = listOf(
            outputFolder.file("${DotnetPluginId.get()}.dll"),
            outputFolder.file("${DotnetPluginId.get()}.pdb"),
            outputFolder.file("Helix.MixinLanguage.dll"),
            outputFolder.file("Helix.MixinLanguage.pdb"),
    )

    dllFiles.forEach { pluginFile ->
        // prepareSandbox names this subproject's plugin directory after project.name.
        // Using rootProject.name here creates a sibling directory that Rider does not
        // recognise as the IntelliJ plugin, so its ReSharper backend never loads the DLL.
        from(pluginFile) { into("${project.name}/dotnet") }
    }
}

tasks.publishPlugin {
    dependsOn(testDotNet)
    dependsOn(tasks.buildPlugin)
    token.set("$PublishToken")

    doLast {
        execOperations.exec {
            executable("dotnet")
            args("nuget", "push", "output/${DotnetPluginId}.${version}.nupkg", "--api-key", PublishToken, "--source", "https://plugins.jetbrains.com")
            workingDir(projectDir)
        }
    }
}

val riderModel: Configuration = configurations.create("riderModel") {
    isCanBeConsumed = true
    isCanBeResolved = false
}

artifacts {
    add(riderModel.name, provider {
        intellijPlatform.platformPath.resolve("lib/rd/rider-model.jar").also {
            check(it.isFile) {
                "rider-model.jar is not found at $riderModel"
            }
        }
    }) {
        builtBy(Constants.Tasks.INITIALIZE_INTELLIJ_PLATFORM_PLUGIN)
    }
}

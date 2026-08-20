import com.jetbrains.rd.generator.gradle.RdGenTask
import org.gradle.jvm.toolchain.JavaLanguageVersion

plugins {
    id("org.jetbrains.kotlin.jvm")
    id("com.jetbrains.rdgen") version libs.versions.rdGen
}

dependencies {
    implementation(libs.kotlinStdLib)
    implementation(libs.rdGen)
    implementation(project(mapOf("path" to ":riderPlugin", "configuration" to "riderModel")))
}

rdgen {
    val pluginRoot = projectDir.parentFile

    verbose = true
    packages = "model.rider"

    generator {
        language = "kotlin"
        transform = "asis"
        root = "com.jetbrains.rider.model.nova.ide.IdeRoot"
        namespace = "com.jetbrains.rider.model"
        directory = pluginRoot.resolve("src/rider/main/kotlin/dev/helight/helix/protocol").path
    }

    generator {
        language = "csharp"
        transform = "reversed"
        root = "com.jetbrains.rider.model.nova.ide.IdeRoot"
        namespace = "JetBrains.Rider.Model"
        directory = pluginRoot.resolve("src/dotnet/HelixRider/Protocol").path
    }
}

tasks.withType<RdGenTask> {
    val classPath = sourceSets["main"].runtimeClasspath
    dependsOn(classPath)
    classpath(classPath)
    javaLauncher.set(javaToolchains.launcherFor {
        languageVersion.set(JavaLanguageVersion.of(25))
    })
}

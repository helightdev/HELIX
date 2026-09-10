plugins {
    id("org.jetbrains.kotlin.jvm")
}

dependencies {
    implementation(libs.kotlinStdLib)
    implementation(libs.rdGen)
    implementation(project(mapOf("path" to ":riderPlugin", "configuration" to "riderModel")))
}

// Invoke the generator directly: the upstream RdGenTask mutates its classpath/arguments
// at execution time and captures Project, preventing configuration-cache reuse.
tasks.register<JavaExec>("rdgen") {
    group = "code generation"
    description = "Generates the Kotlin and C# Rider protocol when its model or generator changes."
    classpath = sourceSets["main"].runtimeClasspath
    mainClass.set("com.jetbrains.rd.generator.nova.MainKt")
    workingDir(layout.projectDirectory)
    inputs.file(layout.projectDirectory.file("protocol.generators"))
    // The generator specs target Rider's external IdeRoot. Include its package in discovery
    // explicitly; relying on HelixExpressionModel's superclass initialization makes root
    // discovery depend on classloader iteration order and breaks in IntelliJ's cached runs.
    args("-p", "model.rider,com.jetbrains.rider.model.nova.ide", "-g", "protocol.generators")
    val generated = listOf(
        layout.projectDirectory.file("../src/rider/main/kotlin/dev/helight/helix/protocol/HelixExpressionModel.Generated.kt").asFile,
        layout.projectDirectory.file("../src/dotnet/HelixRider/Protocol/HelixExpressionModel.Generated.cs").asFile,
    )
    // The destinations also contain handwritten hosts; never claim or clear their directories.
    outputs.files(generated)
    doLast {
        generated.forEach { file ->
            val source = file.readText()
            val normalized = source.replace(Regex("[ \\t]+(?=\\r?$)", RegexOption.MULTILINE), "")
            if (normalized != source) file.writeText(normalized)
        }
    }
}

pluginManagement {
    includeBuild("build-logic")

    repositories {
        maven { setUrl("https://cache-redirector.jetbrains.com/plugins.gradle.org") }
        maven { setUrl("https://cache-redirector.jetbrains.com/maven-central") }
        maven { setUrl("https://cache-redirector.jetbrains.com/dl.bintray.com/kotlin/kotlin-eap") }
    }

}

// Load platform transforms above the project/convention-plugin classloader. Otherwise
// edits to helix.build change their implementation fingerprint and extract Rider again.
plugins {
    id("org.jetbrains.intellij.platform.settings") version "2.18.0"
}

rootProject.name = "HELIX"

include(":riderPlugin")
project(":riderPlugin").projectDir = file("src/RiderPlugin")
include(":riderPlugin:protocol")

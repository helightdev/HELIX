plugins {
    // Keep convention-plugin compilation isolated from the main build's artifact transforms.
    `kotlin-dsl`
    `java-gradle-plugin`
}

repositories {
    mavenCentral()
}

gradlePlugin {
    plugins {
        create("helixBuild") {
            id = "helix.build"
            implementationClass = "HelixBuildPlugin"
        }
    }
}

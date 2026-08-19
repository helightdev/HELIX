plugins {
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

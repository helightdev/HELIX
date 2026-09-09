plugins {
    base
    id("helix.build")
}

solutions {
    solution("src/RiderPlugin/HelixRider.sln")
    solution("src/Hix/Hix.sln")
}

unity {
    metadata {
        author {
            name = "Christoph 'HelightDev' Feuerer"
            url = "https://helight.dev"
        }
        keywords("HELIX")
        documentationUrl = "https://helix.helight.dev/docs/"
        licensesUrl = "https://github.com/helightdev/HELIX/blob/main/LICENSE"
        license = "MIT"
    }

    val core = unityPackage("Assets/Plugins/HELIX") {
        name = "dev.helight.helix.core"
        displayName = "HELIX Core"
        description = "Core library for other HELIX packages including the generator and mixins."
        category = "Scripting"

        dependency("com.unity.mathematics", "1.4.0")
        dependency("com.unity.profiling.core", "1.0.3")
    }
    val compose = unityPackage("Assets/Plugins/HELIX.Compose") {
        name = "dev.helight.helix.compose"
        displayName = "HELIX Compose"
        description = "Intermediate-enough ui composition and styling layer on-top of unity ui toolkit."
        category = "UI"
        keywords("UI", "UI Toolkit", "Compose")

        dependency("com.unity.inputsystem", "1.19.0")
        projectDependency(core)
    }
    val context = unityPackage("Assets/Plugins/HELIX.Context") {
        name = "dev.helight.helix.context"
        displayName = "HELIX Context"
        description = "Service and dependency management system with lifecycle management and productivity helpers."
        category = "Scripting"
        keywords("DI", "Dependency Injection", "Service Locator", "Context")

        dependency("com.unity.addressables", "2.11.1")
        dependency("com.cysharp.unitask", "2.5.11")
        projectDependency(core)
    }
    val support = unityPackage("Assets/Plugins/HELIX.Support") {
        name = "dev.helight.helix.support"
        displayName = "HELIX Support"
        description = "Development and debugging tools for HELIX-based Unity projects."
        category = "Integration"
        keywords("Debugging", "Development", "Support")

        projectDependency(core)
        projectDependency(compose)
        projectDependency(context)
    }
    val ui = unityPackage("Assets/Plugins/HELIX.UI") {
        name = "dev.helight.helix.ui"
        displayName = "HELIX UI"
        description = "A collection of customizable and reusable UI components, styles, and utilities for UI Toolkit."
        category = "UI"
        keywords("UI", "UI Toolkit", "Widgets")

        projectDependency(core)
        projectDependency(compose)
        projectDependency(context)
    }
    val devtools = unityPackage("Assets/Plugins/HELIX.Devtools") {
        name = "dev.helight.helix.devtools"
        displayName = "HELIX Devtools"
        description = "Development and debugging tools for HELIX-based Unity projects."
        category = "Integration"
        keywords("Debugging", "Development", "Support", "IDE", "Pipeline")

        dependency("com.unity.pipeline", "0.5.0-exp.1 ")
        projectDependency(core)
        projectDependency(compose)
        projectDependency(context)
        projectDependency(support)
    }

    val boot = unityPackage("Assets/Plugins/HELIX.Boot") {
        name = "dev.helight.helix.boot"
        displayName = "HELIX Boot"
        description = "Boostrap and meta package for fully HELIX based packages."
        category = "HELIX"
        keywords("Boot", "Meta", "Tools", "Framework", "UI", "DI", "Service")

        projectDependency(core)
        projectDependency(compose)
        projectDependency(context)
        projectDependency(support)
        projectDependency(ui)
        projectDependency(devtools)
    }
}

tasks.wrapper {
    gradleVersion = "9.7.0"
    distributionType = Wrapper.DistributionType.ALL
    distributionUrl =
        "https://cache-redirector.jetbrains.com/services.gradle.org/distributions/gradle-${gradleVersion}-all.zip"
}

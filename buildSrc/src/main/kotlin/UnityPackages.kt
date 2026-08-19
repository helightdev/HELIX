import groovy.json.JsonOutput
import groovy.json.JsonSlurper
import org.gradle.api.DefaultTask
import org.gradle.api.Project
import org.gradle.api.file.ConfigurableFileCollection
import org.gradle.api.file.DirectoryProperty
import org.gradle.api.provider.MapProperty
import org.gradle.api.provider.Property
import org.gradle.api.tasks.Input
import org.gradle.api.tasks.InputFiles
import org.gradle.api.tasks.Internal
import org.gradle.api.tasks.OutputDirectory
import org.gradle.api.tasks.OutputFiles
import org.gradle.api.tasks.PathSensitive
import org.gradle.api.tasks.PathSensitivity
import org.gradle.api.tasks.SkipWhenEmpty
import org.gradle.api.tasks.TaskAction
import org.gradle.process.ExecOperations
import java.io.File
import java.net.URI
import java.nio.charset.StandardCharsets
import java.nio.file.Files
import java.util.Base64
import javax.inject.Inject

@HelixBuildDsl
open class UnityPackageAuthorSpec {
    var name: String? = null
    var email: String? = null
    var url: String? = null
}

@HelixBuildDsl
open class UnityPackageMetadata {
    var displayName: String? = null
    var description: String? = null
    var license: String? = null
    var category: String? = null
    var type: String? = null
    var documentationUrl: String? = null
    var changelogUrl: String? = null
    var licensesUrl: String? = null
    var hideInEditor: Boolean? = null
    internal val keywordValues = mutableListOf<String>()
    internal var authorValue: UnityPackageAuthorSpec? = null
    internal val dependencyValues = linkedMapOf<String, String>()
    internal val extraProperties = linkedMapOf<String, Any>()

    fun keywords(vararg values: String) {
        keywordValues += values
    }

    fun author(configure: UnityPackageAuthorSpec.() -> Unit) {
        authorValue = UnityPackageAuthorSpec().apply(configure)
    }

    fun dependency(name: String, version: String) {
        addDependency(name, version)
    }

    fun dependency(notation: String) {
        val separator = notation.indexOf(':')
        if (separator < 0) {
            projectDependency(notation)
            return
        }

        addDependency(notation.substring(0, separator), notation.substring(separator + 1))
    }

    fun projectDependency(name: String) {
        addDependency(name, USE_GLOBAL_PACKAGE_VERSION)
    }

    fun projectDependency(packageSpec: UnityPackageSpec) {
        projectDependency(requireNotNull(packageSpec.name) {
            "Unity project package '${packageSpec.path}' must define its package.json name before it is referenced."
        })
    }

    private fun addDependency(name: String, version: String) {
        require(name.isNotBlank()) { "Unity package dependency name must not be blank." }
        require(version.isNotBlank()) { "Unity package dependency version must not be blank." }
        dependencyValues[name] = version
    }

    fun property(name: String, value: Any) {
        extraProperties[name] = value
    }

    fun properties(values: Map<String, Any>) {
        extraProperties += values
    }
}

@HelixBuildDsl
open class UnityPackageSpec(val path: String) : UnityPackageMetadata() {
    var name: String? = null
    var publishable: Boolean = true
}

@HelixBuildDsl
open class UnityExtension {
    internal var projectRoot: String = ""
    internal val metadata = UnityPackageMetadata()
    internal val packages = mutableListOf<UnityPackageSpec>()

    fun metadata(configure: UnityPackageMetadata.() -> Unit) {
        metadata.configure()
    }

    fun unityPackage(path: String, configure: UnityPackageSpec.() -> Unit): UnityPackageSpec {
        return UnityPackageSpec(path).apply(configure).also(packages::add)
    }

    internal fun asSolutionSpecs(project: Project): List<MonorepoSolutionSpec> {
        validate()
        val solutionPath = project.relativePath(solutionFile(project))
        val registeredAssemblyNames = linkedSetOf<String>()
        val packageSolutions = packages.map { packageSpec ->
            val packageDirectory = packageDirectory(project, packageSpec)
            require(packageDirectory.isDirectory) { "Unity package directory does not exist: $packageDirectory" }
            val assemblyNames = packageDirectory.walkTopDown()
                .filter { it.isFile && it.extension == "asmdef" }
                .mapNotNull { (JsonSlurper().parse(it) as? Map<*, *>)?.get("name") as? String }
                .toSortedSet()
            require(assemblyNames.isNotEmpty()) {
                "Unity package does not contain any .asmdef files: $packageDirectory"
            }
            registeredAssemblyNames += assemblyNames
            MonorepoSolutionSpec(
                path = solutionPath,
                folderName = packageSpec.displayName ?: packageSpec.name ?: packageDirectory.name,
                includeUnselectedProjects = false,
            ).apply {
                project(*assemblyNames.toTypedArray())
            }
        }
        val excludedProjects = MonorepoSolutionSpec(
            path = solutionPath,
            includeUnselectedProjects = true,
            emitSelectedProjects = false,
        ).apply {
            project(*registeredAssemblyNames.toTypedArray())
        }
        return packageSolutions + excludedProjects
    }

    internal fun validate() {
        require(projectRoot.isNotBlank()) {
            "Set UnityProjectRoot in gradle.properties or pass -PUnityProjectRoot=<path>."
        }
        packages.forEach { packageSpec ->
            require(!packageSpec.name.isNullOrBlank()) {
                "Unity package '${packageSpec.path}' must define its package.json name."
            }
        }
    }

    internal fun packageDirectory(project: Project, packageSpec: UnityPackageSpec): File =
        project.file(projectRoot).resolve(packageSpec.path)

    internal fun manifestFile(project: Project, packageSpec: UnityPackageSpec): File =
        packageDirectory(project, packageSpec).resolve("package.json")

    private fun solutionFile(project: Project): File {
        val root = project.file(projectRoot)
        return root.resolve("${root.name}.sln")
    }
}

abstract class GenerateUnityPackageManifestsTask : DefaultTask() {
    @get:Input
    abstract val manifests: MapProperty<String, String>

    @get:OutputFiles
    abstract val manifestFiles: ConfigurableFileCollection

    @TaskAction
    fun generate() {
        manifests.get().toSortedMap().forEach { (path, json) ->
            val manifest = project.file(path)
            manifest.parentFile.mkdirs()
            manifest.writeText(json)
            logger.lifecycle("Generated ${manifest.relativeTo(project.rootDir)}")
        }
    }
}

abstract class PackUnityPackagesTask @Inject constructor(
    private val execOperations: ExecOperations,
) : DefaultTask() {
    @get:InputFiles
    @get:PathSensitive(PathSensitivity.RELATIVE)
    abstract val packageDirectories: ConfigurableFileCollection

    @get:Input
    abstract val organizationId: Property<String>

    @get:Input
    abstract val upmExecutable: Property<String>

    @get:Internal
    abstract val serviceAccountKeyId: Property<String>

    @get:Internal
    abstract val serviceAccountKeySecret: Property<String>

    @get:OutputDirectory
    abstract val destinationDirectory: DirectoryProperty

    @TaskAction
    fun pack() {
        val organization = requiredProperty("UpmOrganizationId", organizationId.get())
        val keyId = requiredProperty("UpmServiceAccountKeyId", serviceAccountKeyId.get())
        val keySecret = requiredProperty("UpmServiceAccountKeySecret", serviceAccountKeySecret.get())
        val destination = destinationDirectory.get().asFile.apply { mkdirs() }

        packageDirectories.files.sortedBy(File::getPath).forEach { packageDirectory ->
            require(File(packageDirectory, "package.json").isFile) {
                "Unity package directory does not contain package.json: $packageDirectory"
            }
            logger.lifecycle("Packing Unity package ${packageDirectory.relativeTo(project.rootDir)}")
            execOperations.exec {
                executable(upmExecutable.get())
                args(
                    "pack",
                    packageDirectory.absolutePath,
                    "--organization-id",
                    organization,
                    "--destination",
                    destination.absolutePath,
                )
                environment("UPM_SERVICE_ACCOUNT_KEY_ID", keyId)
                environment("UPM_SERVICE_ACCOUNT_KEY_SECRET", keySecret)
                workingDir(project.rootDir)
            }
        }
    }
}

abstract class PublishUnityPackagesTask @Inject constructor(
    private val execOperations: ExecOperations,
) : DefaultTask() {
    @get:InputFiles
    @get:SkipWhenEmpty
    @get:PathSensitive(PathSensitivity.RELATIVE)
    abstract val packageArchives: ConfigurableFileCollection

    @get:Input
    abstract val registry: Property<String>

    @get:Input
    abstract val npmExecutable: Property<String>

    @get:Internal
    abstract val username: Property<String>

    @get:Internal
    abstract val password: Property<String>

    @TaskAction
    fun publish() {
        val registryUrl = normalizedRegistry(requiredProperty("UpmRegistry", registry.get()))
        val loginName = requiredProperty("UpmRegistryUsername", username.get())
        val loginPassword = requiredProperty("UpmRegistryPassword", password.get())
        require(loginName.none { it == '\n' || it == '\r' }) {
            "UpmRegistryUsername must not contain line breaks."
        }
        require(loginPassword.none { it == '\n' || it == '\r' }) {
            "UpmRegistryPassword must not contain line breaks."
        }

        val npmConfig = Files.createTempFile("helix-upm-publish-", ".npmrc")
        try {
            val registryUri = URI(registryUrl)
            val authPath = registryUri.rawPath.let { if (it.endsWith('/')) it else "$it/" }
            val authScope = "//${registryUri.rawAuthority}$authPath"
            val encodedPassword = Base64.getEncoder()
                .encodeToString(loginPassword.toByteArray(StandardCharsets.UTF_8))
            Files.writeString(
                npmConfig,
                buildString {
                    appendLine("registry=$registryUrl")
                    appendLine("${authScope}:username=$loginName")
                    appendLine("${authScope}:_password=$encodedPassword")
                },
                StandardCharsets.UTF_8,
            )

            packageArchives.files.sortedBy(File::getName).forEach { archive ->
                require(archive.isFile && archive.extension == "tgz") {
                    "Unity package archive must be a .tgz file: $archive"
                }
                logger.lifecycle("Publishing Unity package ${archive.relativeTo(project.rootDir)}")
                execOperations.exec {
                    executable(npmExecutable.get())
                    args(
                        "publish",
                        archive.absolutePath,
                        "--registry",
                        registryUrl,
                        "--userconfig",
                        npmConfig.toAbsolutePath().toString(),
                    )
                    workingDir(project.rootDir)
                }
            }
        } finally {
            Files.deleteIfExists(npmConfig)
        }
    }
}

internal fun registerUnityPackageTasks(project: Project, extension: UnityExtension) {
    val packageVersion = project.providers.gradleProperty("PackageVersion")
    val unityVersion = project.providers.gradleProperty("UnityVersion")

    val generateManifests = project.tasks.register(
        "generateUnityPackageManifests",
        GenerateUnityPackageManifestsTask::class.java,
    ) {
        group = "build"
        description = "Generates package.json files for all configured Unity packages."
        manifests.set(project.provider {
            extension.manifests(
                project,
                requiredProperty("PackageVersion", packageVersion.get()),
                requiredProperty("UnityVersion", unityVersion.get()),
            )
        })
        manifestFiles.from(project.provider {
            extension.packages.map { extension.manifestFile(project, it) }
        })
    }

    val packPackages = project.tasks.register("packUnityPackages", PackUnityPackagesTask::class.java) {
        group = "distribution"
        description = "Creates signed .tgz archives for all publishable Unity packages."
        dependsOn(generateManifests)
        packageDirectories.from(project.provider {
            extension.validate()
            extension.packages.filter { it.publishable == true }.map { extension.packageDirectory(project, it) }
        })
        organizationId.set(project.providers.gradleProperty("UpmOrganizationId"))
        serviceAccountKeyId.set(project.providers.gradleProperty("UpmServiceAccountKeyId"))
        serviceAccountKeySecret.set(project.providers.gradleProperty("UpmServiceAccountKeySecret"))
        upmExecutable.convention(project.providers.gradleProperty("UpmExecutable").orElse("upm"))
        destinationDirectory.set(project.layout.dir(
            project.providers.gradleProperty("UpmPackageDestination")
                .orElse("build/upm-packages")
                .map(project::file)
        ))
    }

    project.tasks.register("publishUnityPackages", PublishUnityPackagesTask::class.java) {
        group = "publishing"
        description = "Publishes the signed Unity package archives to the configured npm registry."
        dependsOn(packPackages)
        packageArchives.from(project.provider {
            project.fileTree(packPackages.get().destinationDirectory) {
                include("*.tgz")
            }
        })
        registry.set(project.providers.gradleProperty("UpmRegistry"))
        username.set(project.providers.gradleProperty("UpmRegistryUsername"))
        password.set(project.providers.gradleProperty("UpmRegistryPassword"))
        npmExecutable.convention(project.providers.gradleProperty("NpmExecutable").orElse("npm"))
    }

    project.tasks.named("assemble") {
        dependsOn(packPackages)
    }
}

private fun UnityExtension.manifests(project: Project, packageVersion: String, unityVersion: String): Map<String, String> {
    validate()
    require(Regex("""\d+\.\d+""").matches(unityVersion)) {
        "UnityVersion must use Unity's package.json major.minor format, for example 6000.5."
    }
    return packages.associate { packageSpec ->
        val manifest = linkedMapOf<String, Any>().apply {
            putAll(metadata.extraProperties)
            putAll(packageSpec.extraProperties)
            put("name", packageSpec.name!!)
            put("displayName", effective(packageSpec.displayName, metadata.displayName, packageSpec.name!!))
            put("version", packageVersion)
            put("unity", unityVersion)
        }
        putOptional(manifest, "description", packageSpec.description ?: metadata.description)
        putOptional(manifest, "license", packageSpec.license ?: metadata.license)
        putOptional(manifest, "category", packageSpec.category ?: metadata.category)
        putOptional(manifest, "type", packageSpec.type ?: metadata.type)
        putOptional(manifest, "documentationUrl", packageSpec.documentationUrl ?: metadata.documentationUrl)
        putOptional(manifest, "changelogUrl", packageSpec.changelogUrl ?: metadata.changelogUrl)
        putOptional(manifest, "licensesUrl", packageSpec.licensesUrl ?: metadata.licensesUrl)
        (packageSpec.hideInEditor ?: metadata.hideInEditor)?.let { manifest["hideInEditor"] = it }

        val author = packageSpec.authorValue ?: metadata.authorValue
        author?.let {
            manifest["author"] = linkedMapOf<String, Any>().apply {
                putOptional(this, "name", it.name)
                putOptional(this, "email", it.email)
                putOptional(this, "url", it.url)
            }
        }
        val keywords = (metadata.keywordValues + packageSpec.keywordValues).distinct()
        if (keywords.isNotEmpty()) manifest["keywords"] = keywords

        val dependencies = (metadata.dependencyValues + packageSpec.dependencyValues).mapValues { (_, version) ->
            if (version == USE_GLOBAL_PACKAGE_VERSION) packageVersion else version
        }
        manifest["dependencies"] = dependencies

        val json = JsonOutput.prettyPrint(JsonOutput.toJson(manifest))
            .replace(Regex("""\{\s+}"""), "{}")
        manifestFile(project, packageSpec).path to "$json\n"
    }
}

private fun <T> effective(value: T?, default: T?, fallback: T): T = value ?: default ?: fallback

private fun putOptional(target: MutableMap<String, Any>, key: String, value: String?) {
    if (!value.isNullOrBlank()) target[key] = value
}

private fun requiredProperty(name: String, rawValue: String): String {
    val value = rawValue.trim().removeSurrounding("\"")
    require(value.isNotBlank() && value != "_PLACEHOLDER_") {
        "Set $name in gradle.properties or pass -P$name=<value>."
    }
    return value
}

private fun normalizedRegistry(rawValue: String): String {
    val registry = rawValue.trim().removeSuffix("/") + "/"
    val uri = URI(registry)
    require(uri.scheme == "https" || uri.scheme == "http") {
        "UpmRegistry must be an HTTP(S) URL."
    }
    require(!uri.host.isNullOrBlank() && uri.userInfo == null && uri.query == null && uri.fragment == null) {
        "UpmRegistry must be a registry URL without credentials, a query, or a fragment."
    }
    return registry
}

private const val USE_GLOBAL_PACKAGE_VERSION = "<global-package-version>"

package dev.helight.helix.cli

import dev.helight.helix.HelixMessagesBundle.message
import com.intellij.execution.configurations.PathEnvironmentVariableUtil
import com.intellij.openapi.Disposable
import com.intellij.openapi.components.Service
import com.intellij.openapi.components.service
import com.intellij.openapi.diagnostic.Logger
import com.intellij.openapi.project.Project
import com.intellij.util.io.awaitExit
import kotlinx.coroutines.*
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import kotlinx.serialization.json.JsonElement
import kotlinx.serialization.json.decodeFromJsonElement
import java.io.BufferedWriter
import java.io.File
import java.io.IOException
import java.nio.charset.StandardCharsets
import java.nio.file.Path
import java.nio.file.Paths
import java.util.*
import java.util.concurrent.ConcurrentHashMap
import java.util.concurrent.atomic.AtomicReference
import kotlin.time.Duration.Companion.milliseconds

/** Project-scoped, lazily started connection to the Unity CLI NDJSON shell. */
@Service(Service.Level.PROJECT)
internal class UnityCliProjectService(
    private val project: Project,
    internal val coroutineScope: CoroutineScope,
) : Disposable {
    private val projectPath: Path = findUnityProjectRoot(project.basePath)
    private val session = AtomicReference<ShellSession?>()
    private val sessionMutex = Mutex()
    private val _status = MutableStateFlow<UnityCliStatusState>(UnityCliStatusState.NotLoaded)
    private var statusRefresh: Job? = null

    val status: StateFlow<UnityCliStatusState> = _status.asStateFlow()
    val unityProjectPath: String get() = projectPath.toString()

    suspend fun execute(command: String, arguments: List<String> = emptyList(), targetProject: Boolean = true): UnityCliCommandResponse {
        require(command.isNotBlank()) { message("unity.cli.error.command.blank") }
        val argv = buildList {
            add(command)
            addAll(arguments)
            if (targetProject && arguments.none { it == "--project-path" || it.startsWith("--project-path=") }) {
                add("--project-path")
                add(unityProjectPath)
            }
        }
        var lastFailure: Throwable? = null
        repeat(2) {
            val current = getOrStartSession()
            try {
                return current.request(argv)
            } catch (failure: Throwable) {
                if (failure is CancellationException) throw failure
                lastFailure = failure
                discardSession(current)
            }
        }
        throw IOException(message("unity.cli.error.shell.unavailable"), lastFailure)
    }

    suspend fun loadStatus(): UnityCliStatusSnapshot {
        val response = execute("status")
        val data = response.envelope.data?.let { UnityCliJson.decodeFromJsonElement<UnityCliStatusData>(it) }
            ?: UnityCliStatusData()
        val exactProject = normalizePath(unityProjectPath)
        return UnityCliStatusSnapshot(
            projectPath = unityProjectPath,
            exitCode = response.exitCode,
            success = response.envelope.success,
            instances = data.instances.filter { normalizePath(it.project) == exactProject },
            errors = response.envelope.errors,
            warnings = response.envelope.warnings,
        )
    }

    suspend fun executeTool(name: String, arguments: List<String> = emptyList()): JsonElement {
        require(name.isNotBlank()) { message("unity.cli.error.pipeline.command.blank") }
        val response = execute("command", listOf(name) + arguments)
        if (!response.envelope.success) throw commandFailure(name, response)
        val commandData = response.envelope.data?.let { UnityCliJson.decodeFromJsonElement<UnityCliToolCommandData>(it) }
            ?: throw UnityCliCommandException(message("unity.cli.error.no.command.data", name))
        if (!commandData.success || commandData.result == null) throw commandFailure(name, response)
        return commandData.result
    }

    fun refreshStatus() {
        if (project.isDisposed || statusRefresh?.isActive == true) return
        val previous = (_status.value as? UnityCliStatusState.Loaded)?.snapshot
        _status.value = UnityCliStatusState.Loading(previous)
        statusRefresh = coroutineScope.launch(Dispatchers.IO) {
            _status.value = try {
                UnityCliStatusState.Loaded(loadStatus())
            } catch (failure: Throwable) {
                if (failure is CancellationException) throw failure
                LOG.warn("Unable to read Unity CLI status for $unityProjectPath", failure)
                UnityCliStatusState.Failed(unityProjectPath, failure.message ?: failure.javaClass.simpleName)
            }
        }
    }

    private fun commandFailure(name: String, response: UnityCliCommandResponse): UnityCliCommandException {
        val message = response.envelope.errors.joinToString("; ") { it.message }
            .ifBlank { message("unity.cli.error.pipeline.command.failed", name, response.exitCode) }
        return UnityCliCommandException(message)
    }

    private suspend fun getOrStartSession(): ShellSession = sessionMutex.withLock {
        session.get()?.takeIf(ShellSession::isAlive)?.let { return@withLock it }
        startSession().also(session::set)
    }

    private suspend fun startSession(): ShellSession = withContext(Dispatchers.IO) {
        val executable = System.getenv("UNITY_CLI_PATH")?.takeIf(String::isNotBlank)
            ?: PathEnvironmentVariableUtil.findInPath("unity")?.absolutePath ?: "unity"
        val process = ProcessBuilder(
            executable, "--format", "json", "--no-banner", "--non-interactive", "shell", "--protocol", "ndjson",
        ).directory(projectPath.toFile()).start()
        ShellSession(process, coroutineScope, ::onSessionExited)
    }

    private suspend fun discardSession(candidate: ShellSession) {
        sessionMutex.withLock { if (session.compareAndSet(candidate, null)) candidate.close() }
    }

    private fun onSessionExited(exited: ShellSession) {
        session.compareAndSet(exited, null)
    }

    override fun dispose() {
        statusRefresh?.cancel()
        session.getAndSet(null)?.close()
    }

    private class ShellSession(
        private val process: Process,
        scope: CoroutineScope,
        private val onExit: (ShellSession) -> Unit,
    ) {
        private val writer: BufferedWriter = process.outputWriter(StandardCharsets.UTF_8)
        private val writeMutex = Mutex()
        private val pending = ConcurrentHashMap<String, CompletableDeferred<UnityCliCommandResponse>>()
        private val lastError = AtomicReference<String?>(null)

        init {
            scope.launch(Dispatchers.IO) { readResponses() }
            scope.launch(Dispatchers.IO) { readErrors() }
            scope.launch(Dispatchers.IO) {
                val exitCode = process.awaitExit()
                failPending(IOException(buildExitMessage(exitCode)))
                onExit(this@ShellSession)
            }
        }

        fun isAlive(): Boolean = process.isAlive

        suspend fun request(argv: List<String>): UnityCliCommandResponse {
            check(process.isAlive) { message("unity.cli.error.shell.exited") }
            val id = UUID.randomUUID().toString()
            val result = CompletableDeferred<UnityCliCommandResponse>()
            pending[id] = result
            try {
                val request = UnityCliJson.encodeToString(UnityCliShellRequest(id, argv))
                writeMutex.withLock {
                    writer.write(request)
                    writer.newLine()
                    writer.flush()
                }
                return withTimeout(COMMAND_TIMEOUT_MILLIS.milliseconds) { result.await() }
            } finally {
                pending.remove(id, result)
            }
        }

        private fun readResponses() {
            try {
                process.inputReader(StandardCharsets.UTF_8).useLines { lines ->
                    lines.filter(String::isNotBlank).forEach { line ->
                        try {
                            val response = UnityCliJson.decodeFromString<UnityCliCommandResponse>(line)
                            pending.remove(response.id)?.complete(response)
                        } catch (failure: Throwable) {
                            LOG.warn("Ignoring malformed Unity CLI shell response: $line", failure)
                        }
                    }
                }
            } catch (failure: Throwable) {
                if (process.isAlive) failPending(IOException(message("unity.cli.error.read.output"), failure))
            }
        }

        private fun readErrors() {
            try {
                process.errorReader(StandardCharsets.UTF_8).useLines { lines ->
                    lines.filter(String::isNotBlank).forEach { line ->
                        lastError.set(line)
                        LOG.warn("Unity CLI: $line")
                    }
                }
            } catch (_: IOException) {
                // Expected when the project or process is disposed.
            }
        }

        private fun buildExitMessage(exitCode: Int): String = buildString {
            append(message("unity.cli.error.shell.exit.code", exitCode))
            lastError.get()?.let { append(": ").append(it) }
        }

        private fun failPending(failure: Throwable) {
            pending.values.forEach { it.completeExceptionally(failure) }
            pending.clear()
        }

        fun close() {
            runCatching { writer.close() }
            process.destroy()
            if (process.isAlive) process.destroyForcibly()
            failPending(IOException(message("unity.cli.error.shell.closed")))
        }
    }

    companion object {
        private val LOG = Logger.getInstance(UnityCliProjectService::class.java)
        private const val COMMAND_TIMEOUT_MILLIS = 30_000L

        fun getInstance(project: Project): UnityCliProjectService = project.service()

        fun isUnityProject(project: Project): Boolean {
            val basePath = project.basePath ?: return false
            return findUnityProjectRoot(Paths.get(basePath)) != null
        }

        private fun findUnityProjectRoot(basePath: String?): Path {
            val fallback = Paths.get(basePath ?: System.getProperty("user.dir")).toAbsolutePath().normalize()
            return findUnityProjectRoot(fallback) ?: fallback
        }

        private fun findUnityProjectRoot(path: Path): Path? =
            generateSequence(path.toAbsolutePath().normalize()) { it.parent }
                .firstOrNull { it.resolve("Assets").toFile().isDirectory && it.resolve("ProjectSettings").toFile().isDirectory }

        private fun normalizePath(value: String): String =
            runCatching { Paths.get(value).toAbsolutePath().normalize().toString() }
                .getOrDefault(value.trimEnd(File.separatorChar, '/', '\\'))
    }
}

class UnityCliCommandException(message: String) : IOException(message)

package dev.helight.helix.hix

import com.intellij.openapi.application.ApplicationManager
import com.intellij.codeInsight.daemon.DaemonCodeAnalyzer
import com.intellij.openapi.components.Service
import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.openapi.vfs.VirtualFileManager
import com.intellij.openapi.vfs.newvfs.BulkFileListener
import com.intellij.openapi.vfs.newvfs.events.VFileEvent
import com.intellij.openapi.vfs.newvfs.events.VFileMoveEvent
import com.intellij.openapi.vfs.newvfs.events.VFilePropertyChangeEvent
import com.intellij.openapi.util.Key
import com.intellij.psi.PsiManager
import com.intellij.util.concurrency.AppExecutorUtil
import com.jetbrains.rd.framework.RdTaskResult
import com.jetbrains.rd.framework.impl.RpcTimeouts
import com.jetbrains.rd.util.reactive.adviseOnce
import com.jetbrains.rider.plugins.unity.UnityProjectLifetimeService
import com.jetbrains.rider.projectView.solution
import dev.helight.helix.protocol.MixinFileInput
import dev.helight.helix.protocol.MixinFileSnapshot
import dev.helight.helix.protocol.MixinLanguageDefinition
import dev.helight.helix.protocol.MixinParseRequest
import dev.helight.helix.protocol.MixinCompletionItem
import dev.helight.helix.protocol.MixinCompletionRequest
import dev.helight.helix.protocol.helixExpressionModel
import java.util.concurrent.ConcurrentHashMap
import java.util.concurrent.CopyOnWriteArrayList
import java.util.concurrent.TimeUnit
import java.util.concurrent.atomic.AtomicBoolean
import java.util.concurrent.atomic.AtomicLong

/**
 * Frontend cache for immutable backend language-service snapshots. This is intentionally shaped
 * like an LSP document store: calls are asynchronous, responses are revision checked, and PSI
 * only consumes already materialised snapshots synchronously.
 */
@Service(Service.Level.PROJECT)
class HixSnapshotService(private val project: Project) {
    private val revisions = ConcurrentHashMap<String, AtomicLong>()
    private val byPath = ConcurrentHashMap<String, MixinFileSnapshot>()
    private val byHash = ConcurrentHashMap<Long, MutableList<MixinFileSnapshot>>()
    private val openBuffers = ConcurrentHashMap<String, String>()
    private val openFiles = ConcurrentHashMap<String, OpenFile>()
    private val knownFiles = ConcurrentHashMap<String, VirtualFile>()
    private val requestedDirectoryHashes = ConcurrentHashMap<String, Long>()
    private val semanticRetries = ConcurrentHashMap<String, SemanticRetry>()
    private val listeners = CopyOnWriteArrayList<(String, MixinFileSnapshot?) -> Unit>()
    private val catalogRequestInFlight = AtomicBoolean(false)

    @Volatile
    var definitions: Array<MixinLanguageDefinition> = emptyArray()
        private set

    init {
        project.messageBus.connect(project).subscribe(VirtualFileManager.VFS_CHANGES,
            object : BulkFileListener {
                override fun after(events: List<VFileEvent>) {
                    refreshAffectedDirectories(events)
                }
            })
        requestLanguageCatalog()
    }

    fun ensureLanguageCatalog() {
        if (definitions.isNotEmpty() || project.isDisposed || !catalogRequestInFlight.compareAndSet(false, true))
            return
        val lifetime = UnityProjectLifetimeService.getLifetime(project)
        project.solution.helixExpressionModel.getMixinLanguageCatalog
            .start(lifetime, true).result.adviseOnce(lifetime) { result ->
                catalogRequestInFlight.set(false)
                if (result is RdTaskResult.Success && result.value.definitions.isNotEmpty()) {
                    definitions = result.value.definitions
                    val files = knownFiles.values.toList()
                    ApplicationManager.getApplication().invokeLater {
                        if (project.isDisposed) return@invokeLater
                        val psiManager = PsiManager.getInstance(project)
                        val daemon = DaemonCodeAnalyzer.getInstance(project)
                        files.mapNotNull(psiManager::findFile).forEach(daemon::restart)
                    }
                } else scheduleCatalogRetry()
            }
    }

    fun refreshLanguageCatalog() {
        definitions = emptyArray()
        ensureLanguageCatalog()
    }

    private fun requestLanguageCatalog() = ensureLanguageCatalog()

    private fun scheduleCatalogRetry() {
        if (project.isDisposed || definitions.isNotEmpty()) return
        AppExecutorUtil.getAppScheduledExecutorService().schedule(
            { ensureLanguageCatalog() }, CATALOG_RETRY_DELAY_MS, TimeUnit.MILLISECONDS)
    }

    fun lazyCompletions(kind: String, prefix: String): Array<MixinCompletionItem> = try {
        project.solution.helixExpressionModel.completeMixin
            .sync(MixinCompletionRequest(kind, prefix), RpcTimeouts(500L, 2_000L)).items
    } catch (_: Throwable) {
        emptyArray()
    }

    fun snapshot(path: String): MixinFileSnapshot? = byPath[normalise(path)]

    fun virtualFile(path: String): VirtualFile? = knownFiles[normalise(path)]

    fun snapshotForText(text: CharSequence, preferredPath: String? = null): MixinFileSnapshot? {
        val hash = sourceHash(text)
        val candidates = byHash[hash] ?: return null
        val preferred = preferredPath?.let(::normalise)
        return candidates.firstOrNull { snapshot ->
            // Hash collisions are extremely unlikely. The lexer additionally validates all
            // ranges against the current buffer before consuming a snapshot.
            snapshot.sourceHash == hash && (preferred == null || normalise(snapshot.filePath) == preferred)
        } ?: candidates.firstOrNull { it.sourceHash == hash }
    }

    fun snapshotsInDirectory(filePath: String): List<MixinFileSnapshot> {
        val directory = normalise(filePath).substringBeforeLast('/', "")
        return byPath.values.filter {
            normalise(it.filePath).substringBeforeLast('/', "") == directory
        }.sortedBy { it.filePath.lowercase() }
    }

    fun updateOpenBuffer(filePath: String, source: String) {
        openBuffers[normalise(filePath)] = source
    }

    fun openBuffer(file: VirtualFile, source: String) {
        val path = normalise(file.path)
        openBuffers[path] = source
        openFiles.compute(path) { _, existing ->
            if (existing == null) OpenFile(file, AtomicLong(1))
            else existing.apply { users.incrementAndGet() }
        }
    }

    fun closeOpenBuffer(filePath: String) {
        val path = normalise(filePath)
        val remaining = openFiles.computeIfPresent(path) { _, existing ->
            if (existing.users.decrementAndGet() <= 0) null else existing
        }
        if (remaining == null) openBuffers.remove(path)
    }

    fun requestDirectory(origin: VirtualFile, sourceOverride: String? = null) {
        val files = origin.parent?.children
            ?.filter { !it.isDirectory && HixFileType.isCanonical(it.name) }
            ?.sortedBy { it.path.lowercase() }
            ?: listOf(origin)
        val sources = files.map { file ->
            val path = normalise(file.path)
            knownFiles[path] = file
            val source = when {
                file == origin && sourceOverride != null -> sourceOverride
                else -> openBuffers[path] ?: read(file)
            }
            path to source
        }
        val directory = normalise(origin.parent?.path ?: origin.path)
        val requestHash = batchHash(sources)
        if (requestedDirectoryHashes.put(directory, requestHash) == requestHash) {
            // A newly opened editor can request the same already-analysed batch after the
            // original listener has gone away. Re-deliver matching immutable snapshots instead
            // of silently returning and leaving the new editor in its loading state.
            val cached = sources.mapNotNull { (path, source) ->
                byPath[path]?.takeIf { it.sourceHash == sourceHash(source) }?.let { path to it }
            }
            if (cached.isNotEmpty()) ApplicationManager.getApplication().invokeLater {
                cached.forEach { (path, snapshot) ->
                    listeners.forEach { listener -> listener(path, snapshot) }
                }
            }
            return
        }

        val inputs = sources.map { (path, source) ->
            val revision = revisions.computeIfAbsent(path) { AtomicLong() }.incrementAndGet()
            MixinFileInput(path, source, revision)
        }.toTypedArray()

        val lifetime = UnityProjectLifetimeService.getLifetime(project)
        val responded = AtomicBoolean(false)
        AppExecutorUtil.getAppScheduledExecutorService().schedule({
            if (responded.get() || project.isDisposed) return@schedule
            requestedDirectoryHashes.remove(directory, requestHash)
            ApplicationManager.getApplication().invokeLater {
                if (!responded.get() && !project.isDisposed)
                    inputs.forEach { listeners.forEach { listener -> listener(it.filePath, null) } }
            }
        }, REQUEST_TIMEOUT_SECONDS, TimeUnit.SECONDS)
        project.solution.helixExpressionModel.parseMixinFiles
            .start(lifetime, MixinParseRequest(inputs)).result.adviseOnce(lifetime) { result ->
                responded.set(true)
                if (result !is RdTaskResult.Success) {
                    requestedDirectoryHashes.remove(directory, requestHash)
                    ApplicationManager.getApplication().invokeLater {
                        inputs.forEach { listeners.forEach { listener -> listener(it.filePath, null) } }
                    }
                    return@adviseOnce
                }
                accept(directory, requestHash, result.value.files)
            }
    }

    fun revalidateDirectory(origin: VirtualFile, sourceOverride: String? = null) {
        val directory = normalise(origin.parent?.path ?: origin.path)
        requestedDirectoryHashes.remove(directory)
        semanticRetries.remove(directory)
        requestDirectory(origin, sourceOverride)
    }

    fun addListener(listener: (String, MixinFileSnapshot?) -> Unit): AutoCloseable {
        listeners += listener
        return AutoCloseable { listeners -= listener }
    }

    private fun accept(directory: String, requestHash: Long, files: Array<MixinFileSnapshot>) {
        val changed = ArrayList<String>()
        val accepted = ArrayList<String>()
        for (snapshot in files) {
            val path = normalise(snapshot.filePath)
            val expected = revisions[path]?.get() ?: continue
            if (snapshot.revision != expected) continue
            val openSource = openBuffers[path]
            if (openSource != null && snapshot.sourceHash != sourceHash(openSource)) continue
            val previous = byPath[path]
            replaceSnapshot(path, snapshot)
            accepted += path
            if (previous == null || !samePsiSemantics(previous, snapshot)) changed += path
        }
        // Persistent VFS lookup may touch disk and is prohibited on the EDT. Resolve paths on
        // the current RD/background callback, then perform only PSI reparse/listener delivery UI-side.
        val detached = HixDetachedWorkspaceService.getInstance(project)
        val semanticFiles = accepted.mapNotNull { path ->
            (detached.detachedFile(path)
                ?: knownFiles[path]
                ?: com.intellij.openapi.vfs.LocalFileSystem.getInstance().findFileByPath(path))?.let { path to it }
        }
        ApplicationManager.getApplication().invokeLater {
            if (project.isDisposed) return@invokeLater
            val psiManager = PsiManager.getInstance(project)
            val psiByPath = semanticFiles.mapNotNull { (path, file) ->
                psiManager.findFile(file)?.also { psi ->
                    // Semantic state belongs to the stable local PSI file, but does not
                    // participate in parsing or alter its nodes.
                    psi.putUserData(SEMANTIC_SNAPSHOT, byPath[path])
                }?.let { path to it }
            }.toMap()
            if (changed.isNotEmpty()) {
                val daemon = DaemonCodeAnalyzer.getInstance(project)
                changed.mapNotNull(psiByPath::get).forEach(daemon::restart)
            }
            changed.forEach { path -> listeners.forEach { listener -> listener(path, byPath[path]) } }
        }
        scheduleSemanticRetryIfNeeded(directory, requestHash, files)
    }

    private fun scheduleSemanticRetryIfNeeded(directory: String, requestHash: Long,
        files: Array<MixinFileSnapshot>) {
        val hasUnresolvedTypes = files.any { snapshot -> snapshot.diagnostics.any { diagnostic ->
            diagnostic.message.startsWith("unresolved C# type '")
        } }
        if (!hasUnresolvedTypes) {
            semanticRetries.remove(directory)
            return
        }

        // ReSharper can answer the protocol while its Roslyn symbol cache is still warming up.
        // Such a response is syntactically complete but not semantically final, and there is no
        // document change that would naturally issue another request. Retry the unchanged batch
        // for a bounded period; genuine unresolved names remain diagnosed after the retries end.
        val retry = semanticRetries.compute(directory) { _, previous ->
            if (previous == null || previous.requestHash != requestHash) SemanticRetry(requestHash, 1)
            else previous.copy(attempt = previous.attempt + 1)
        } ?: return
        if (retry.attempt > MAX_SEMANTIC_RETRIES) return
        requestedDirectoryHashes.remove(directory, requestHash)
        AppExecutorUtil.getAppScheduledExecutorService().schedule({
            if (project.isDisposed || semanticRetries[directory] != retry) return@schedule
            val origin = openFiles.entries.firstOrNull { (path, _) ->
                path.substringBeforeLast('/', "") == directory
            } ?: return@schedule
            requestDirectory(origin.value.file, openBuffers[origin.key])
        }, SEMANTIC_RETRY_DELAY_MS, TimeUnit.MILLISECONDS)
    }

    private fun read(file: VirtualFile): String = try {
        String(file.contentsToByteArray(), file.charset)
    } catch (_: Exception) {
        ""
    }

    private fun refreshAffectedDirectories(events: List<VFileEvent>) {
        if (openFiles.isEmpty()) return
        val directories = events.asSequence().flatMap(::eventPaths)
            .filter { HixFileType.isCanonical(it.substringAfterLast('/')) }
            .map { it.substringBeforeLast('/', "") }
            .toSet()
        if (directories.isEmpty()) return
        ApplicationManager.getApplication().invokeLater {
            if (project.isDisposed) return@invokeLater
            openFiles.forEach { (path, state) ->
                if (path.substringBeforeLast('/', "") !in directories || !state.file.isValid)
                    return@forEach
                requestDirectory(state.file, openBuffers[path])
            }
        }
    }

    private fun eventPaths(event: VFileEvent): Sequence<String> = sequence {
        yield(normalise(event.path))
        when (event) {
            is VFileMoveEvent -> yield(normalise(event.oldParent.path + "/" + event.file.name))
            is VFilePropertyChangeEvent -> if (event.propertyName == VirtualFile.PROP_NAME) {
                val parent = event.file.parent?.path ?: return@sequence
                yield(normalise(parent + "/" + event.oldValue))
            }
        }
    }

    private fun replaceSnapshot(path: String, snapshot: MixinFileSnapshot) {
        val previous = byPath.put(path, snapshot)
        if (previous != null) {
            byHash[previous.sourceHash]?.let { candidates ->
                candidates.removeIf { normalise(it.filePath) == path }
                if (candidates.isEmpty()) byHash.remove(previous.sourceHash, candidates)
            }
        }
        byHash.computeIfAbsent(snapshot.sourceHash) { CopyOnWriteArrayList() }.add(snapshot)
    }

    private fun samePsiSemantics(left: MixinFileSnapshot, right: MixinFileSnapshot): Boolean =
        left.sourceHash == right.sourceHash &&
            left.declarations.contentDeepEquals(right.declarations) &&
            left.references.contentDeepEquals(right.references) &&
            left.diagnostics.contentDeepEquals(right.diagnostics) &&
            left.typeFacts.contentDeepEquals(right.typeFacts)

    private data class OpenFile(val file: VirtualFile, val users: AtomicLong)
    private data class SemanticRetry(val requestHash: Long, val attempt: Int)

    companion object {
        private const val REQUEST_TIMEOUT_SECONDS = 10L
        private const val SEMANTIC_RETRY_DELAY_MS = 750L
        private const val MAX_SEMANTIC_RETRIES = 80
        private const val CATALOG_RETRY_DELAY_MS = 500L
        val ORIGINAL_PATH: Key<String> = Key.create("helix.mixin.original.path")
        val SEMANTIC_SNAPSHOT: Key<MixinFileSnapshot> = Key.create("helix.mixin.semantic.snapshot")
        fun getInstance(project: Project): HixSnapshotService = project.service()

        fun sourceHash(source: CharSequence): Long {
            var hash = 1469598103934665603L
            for (character in source) {
                hash = hash xor character.code.toLong()
                hash *= 1099511628211L
            }
            return hash
        }

        private fun normalise(path: String): String = path.replace('\\', '/')

        private fun batchHash(files: List<Pair<String, String>>): Long {
            var hash = 1469598103934665603L
            for ((path, source) in files) {
                for (character in path) {
                    hash = hash xor character.code.toLong()
                    hash *= 1099511628211L
                }
                hash = hash xor 0xffL
                hash *= 1099511628211L
                for (character in source) {
                    hash = hash xor character.code.toLong()
                    hash *= 1099511628211L
                }
            }
            return hash
        }
    }
}

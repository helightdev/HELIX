package dev.helight.helix.hix

import com.google.gson.*
import com.intellij.openapi.Disposable
import com.intellij.openapi.components.Service
import com.intellij.openapi.project.Project
import com.intellij.util.concurrency.AppExecutorUtil
import dev.helight.helix.protocol.*
import dev.helight.helix.workspace.WorkspaceSettings
import com.jetbrains.rider.projectView.solution
import com.jetbrains.rd.framework.impl.RpcTimeouts
import java.io.File
import java.util.concurrent.TimeUnit

/** One serialized, stateful stdio analysis session per project. Never falls back to bundled analysis. */
@Service(Service.Level.PROJECT)
class HixToolchainService(private val project: Project) : Disposable {
    @Volatile private var process: Process? = null
    private var executable = ""
    private var reader: java.io.BufferedReader? = null
    private var writer: java.io.BufferedWriter? = null
    private var sequence = 0L
    private var interopConfigured = false
    private val gson = Gson()
    private var reportedFailure: String? = null
    private fun reportFailure(message: String) {
        if (reportedFailure == message || project.isDisposed) return
        reportedFailure = message
        com.intellij.notification.NotificationGroupManager.getInstance().getNotificationGroup("Hix Toolchain")
            .createNotification(message, com.intellij.notification.NotificationType.ERROR).notify(project)
    }

    private val requestLock = java.util.concurrent.locks.ReentrantLock()
    fun request(method: String, parameters: Any = emptyMap<String, String>(), timeoutMillis: Long = 30_000): JsonElement {
        if (!requestLock.tryLock(if (method == "hix/synchronize") timeoutMillis else 500, TimeUnit.MILLISECONDS)) throw java.util.concurrent.TimeoutException("Hix analyzer is busy")
        try { return requestLocked(method, parameters, timeoutMillis) }
        finally { requestLock.unlock() }
    }
    private fun requestLocked(method: String, parameters: Any, timeoutMillis: Long): JsonElement {
        val configured = WorkspaceSettings.getInstance(project).toolchainPath.trim()
        if (configured.isEmpty()) {
            val message = "Configure the Hix toolchain executable in HELIX project settings."
            reportFailure(message); error(message)
        }
        if (process?.isAlive != true || executable != configured) {
            dispose()
            val started = try { ProcessBuilder(configured, "analysis-server").directory(project.basePath?.let(::File)).start() }
            catch (failure: Exception) { reportFailure("Cannot start Hix toolchain: ${failure.message}"); throw failure }
            process = started; executable = configured
            reader = started.inputStream.bufferedReader(Charsets.UTF_8)
            writer = started.outputStream.bufferedWriter(Charsets.UTF_8)
            AppExecutorUtil.getAppExecutorService().execute {
                started.errorStream.bufferedReader().useLines { lines -> lines.forEach { LOG.warn(it) } }
            }
        }
        val params = gson.toJsonTree(parameters)
        if (params.isJsonObject && params.asJsonObject["backend"]?.asString?.equals("Unity", true) == true && !interopConfigured) {
            val schema = project.solution.helixExpressionModel.getCSharpHostSchema.sync("", RpcTimeouts(500L, 2_000L))
            request("hix/configureInterop", mapOf("schema" to JsonParser.parseString(schema)))
            interopConfigured = true
        }
        val id = ++sequence
        try {
            writer!!.apply { write(gson.toJson(mapOf("id" to id, "method" to method, "params" to parameters))); newLine(); flush() }
            val responseReader = reader!!
            val read = AppExecutorUtil.getAppExecutorService().submit<String?> { responseReader.readLine() }
            val line = read.get(timeoutMillis, TimeUnit.MILLISECONDS) ?: error("Hix analysis server exited")
            val response = JsonParser.parseString(line).asJsonObject
            check(response["id"]?.asLong == id) { "Unexpected Hix response identifier" }
            response["error"]?.let { error(it.asJsonObject["message"].asString) }
            reportedFailure = null
            return response["result"]
        } catch (failure: Exception) {
            dispose()
            reportFailure("Hix toolchain request failed: ${failure.message}")
            LOG.warn("Hix toolchain request failed: $method", failure)
            throw failure
        }
    }

    fun parse(inputs: Array<MixinFileInput>, backend: String): Array<MixinFileSnapshot> =
        request("hix/synchronize", mapOf("backend" to backend, "documents" to inputs.map {
            mapOf("path" to it.filePath, "text" to it.sourceText, "revision" to it.revision)
        })).asJsonArray.map { element ->
            val item = element.asJsonObject
            item.add("filePath", item.remove("path"))
            val references = item["references"].asJsonArray
            val names = references.filter { it.asJsonObject["kind"].asString == "CSharpType" }
                .map { it.asJsonObject["name"].asString }.distinct()
            val targets = if (names.isEmpty()) emptyMap() else project.solution.helixExpressionModel.resolveCSharpTypes
                .sync(names.toTypedArray(), RpcTimeouts(500L, 2_000L)).items.associateBy { it.name }
            references.forEach { reference ->
                val ref = reference.asJsonObject
                flattenTarget(ref)
                if (ref["kind"].asString == "CSharpType") targets[ref["name"].asString]?.let { target ->
                    ref.addProperty("targetFilePath", target.targetFilePath)
                    ref.add("targetRange", gson.toJsonTree(target.targetRange))
                }
            }
            gson.fromJson(wireRanges(item), MixinFileSnapshot::class.java)
        }.toTypedArray()

    fun definitions(backend: String, kind: String = "", receiverType: String = "", operandType: String = "", prefix: String = "") =
        gson.fromJson(request("hix/definitions", mapOf("backend" to backend, "kind" to kind,
            "receiverType" to receiverType, "operandType" to operandType, "prefix" to prefix), 2_000), Array<MixinLanguageDefinition>::class.java)

    fun complete(kind: String, prefix: String, backend: String): Array<MixinCompletionItem> =
        request("hix/complete", mapOf("kind" to kind, "prefix" to prefix, "backend" to backend), 2_000).asJsonArray.map {
            flattenTarget(it.asJsonObject); gson.fromJson(wireRanges(it), MixinCompletionItem::class.java)
        }.toTypedArray()

    private fun flattenTarget(value: JsonObject) {
        val target = value.remove("target")?.takeUnless { it.isJsonNull }?.asJsonObject
        value.addProperty("targetFilePath", target?.get("path")?.asString ?: "")
        value.add("targetRange", target?.get("range") ?: JsonObject().apply { addProperty("start", 0); addProperty("end", 0) })
    }
    private fun wireRanges(value: JsonElement): JsonElement {
        if (value.isJsonArray) value.asJsonArray.forEach(::wireRanges)
        if (value.isJsonObject) {
            val obj = value.asJsonObject
            if (obj.has("start") && obj.has("end")) {
                obj.add("startOffset", obj.remove("start")); obj.add("endOffset", obj.remove("end"))
            }
            obj.entrySet().toList().forEach { wireRanges(it.value) }
        }
        return value
    }
    override fun dispose() { process?.destroyForcibly(); process = null; interopConfigured = false }
    companion object {
        private val LOG = com.intellij.openapi.diagnostic.Logger.getInstance(HixToolchainService::class.java)
        fun detectExecutable(path: String = System.getenv("PATH") ?: "", windows: Boolean = com.intellij.openapi.util.SystemInfo.isWindows): String? {
            val names = if (windows) listOf("hix.exe", "Hix.Toolchain.exe") else listOf("hix", "Hix.Toolchain")
            return path.split(if (windows) ";" else ":").filter { it.isNotBlank() }.asSequence()
                .flatMap { directory -> names.asSequence().map { File(directory.trim('"'), it) } }
                .firstOrNull { it.isFile && (windows || it.canExecute()) }?.absolutePath
        }
    }
}

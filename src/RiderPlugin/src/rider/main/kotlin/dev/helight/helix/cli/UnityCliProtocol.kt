package dev.helight.helix.cli

import kotlinx.serialization.SerialName
import kotlinx.serialization.Serializable
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonElement

internal val UnityCliJson = Json {
    ignoreUnknownKeys = true
    explicitNulls = false
}

@Serializable
internal data class UnityCliShellRequest(
    val id: String,
    val argv: List<String>,
)

@Serializable
data class UnityCliCommandResponse(
    val id: String,
    val exitCode: Int,
    val envelope: UnityCliResponseEnvelope,
)

@Serializable
data class UnityCliResponseEnvelope(
    val success: Boolean,
    val command: String,
    val data: JsonElement? = null,
    val errors: List<UnityCliMessage> = emptyList(),
    val warnings: List<UnityCliMessage> = emptyList(),
)

@Serializable
data class UnityCliMessage(
    val code: String? = null,
    val message: String,
)

@Serializable
data class UnityCliStatusData(
    val count: Int = 0,
    val instances: List<UnityCliEditorInstance> = emptyList(),
)

@Serializable
data class UnityCliEditorInstance(
    val port: Int,
    val project: String,
    val version: String,
    val pid: Long,
    val state: String,
)

@Serializable
internal data class UnityCliToolCommandData(
    val command: String,
    val result: JsonElement? = null,
    val success: Boolean,
)

data class UnityCliStatusSnapshot(
    val projectPath: String,
    val exitCode: Int,
    val success: Boolean,
    val instances: List<UnityCliEditorInstance>,
    val errors: List<UnityCliMessage>,
    val warnings: List<UnityCliMessage>,
)

sealed interface UnityCliStatusState {
    data object NotLoaded : UnityCliStatusState
    data class Loading(val previous: UnityCliStatusSnapshot? = null) : UnityCliStatusState
    data class Loaded(val snapshot: UnityCliStatusSnapshot) : UnityCliStatusState
    data class Failed(val projectPath: String, val message: String) : UnityCliStatusState
}

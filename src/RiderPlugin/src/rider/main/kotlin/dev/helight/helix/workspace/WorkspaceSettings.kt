package dev.helight.helix.workspace

import com.intellij.openapi.components.SerializablePersistentStateComponent
import com.intellij.openapi.components.Service
import com.intellij.openapi.components.State
import com.intellij.openapi.components.Storage
import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project

@Service(Service.Level.PROJECT)
@State(name = "WorkspaceSettings", storages = [Storage("helixWorkspace.xml")])
internal class WorkspaceSettings(private val project: Project) :
    SerializablePersistentStateComponent<WorkspaceSettingsState>(WorkspaceSettingsState()) {
    companion object {
        fun getInstance(project: Project): WorkspaceSettings = project.service()
    }

    var entries: List<WorkspaceEntry>
        get() = state.entries.map { entry ->
            val recovered = if (entry.typeId.isNotBlank()) entry
            else if (entry.options["path"] != null) entry.copy(typeId = DirectoryWorkspaceEntryProvider.TYPE_ID)
            else entry.copy(typeId = UnityPackagesWorkspaceEntryProvider.TYPE_ID, showPath = false)
            recovered.copy(options = recovered.options.toMap())
        }
        set(value) {
            val snapshot = value.map { it.copy(options = it.options.toMap()) }
            updateState { it.copy(entries = snapshot) }
        }
}

internal data class WorkspaceSettingsState(
    @JvmField val entries: List<WorkspaceEntry> = emptyList()
)

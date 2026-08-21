package dev.helight.helix.workspace

import com.intellij.openapi.components.SerializablePersistentStateComponent
import com.intellij.openapi.components.Service
import com.intellij.openapi.components.State
import com.intellij.openapi.components.Storage
import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import com.jetbrains.rider.projectView.solution
import dev.helight.helix.protocol.helixExpressionModel

@Service(Service.Level.PROJECT)
@State(name = "WorkspaceSettings", storages = [Storage("helixWorkspace.xml")])
internal class WorkspaceSettings(private val project: Project) :
    SerializablePersistentStateComponent<WorkspaceSettingsState>(WorkspaceSettingsState()) {
    companion object {
        fun getInstance(project: Project): WorkspaceSettings = project.service()
    }

    init {
        syncHelixEnabled(state.helixEnabled)
    }

    override fun loadState(state: WorkspaceSettingsState) {
        super.loadState(state)
        syncHelixEnabled(state.helixEnabled)
    }

    var helixEnabled: Boolean
        get() = state.helixEnabled
        set(value) {
            updateState { it.copy(helixEnabled = value) }
            syncHelixEnabled(value)
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

    private fun syncHelixEnabled(value: Boolean) {
        project.solution.helixExpressionModel.isHelixEnabled.set(value)
    }
}

internal data class WorkspaceSettingsState(
    @JvmField var helixEnabled: Boolean = false,
    @JvmField var entries: List<WorkspaceEntry> = emptyList()
)

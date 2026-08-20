package dev.helight.helix.workspace

import com.intellij.openapi.components.*
import com.intellij.openapi.project.Project
import com.intellij.configurationStore.Property

@Service(Service.Level.PROJECT)
@State(name = "WorkspaceSettings", storages = [Storage("workspacesettings.xml")])
internal class WorkspaceSettings(private val project: Project) :
    SerializablePersistentStateComponent<WorkspaceSettingsState>(WorkspaceSettingsState()) {
    companion object {
        fun getInstance(project: Project): WorkspaceSettings = project.service()
    }

    var value: String?
        get() = state.storeValue
        set(value) {
            updateState {
                it.copy(storeValue = value)
            }
        }
}

internal data class WorkspaceSettingsState(
    @JvmField @Property val storeValue: String? = null // @Property required for primitives
)
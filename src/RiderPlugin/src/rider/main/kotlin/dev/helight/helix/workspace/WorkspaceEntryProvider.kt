package dev.helight.helix.workspace

import com.intellij.ide.util.treeView.AbstractTreeNode
import com.intellij.openapi.extensions.ExtensionPointName
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import javax.swing.Icon

data class WorkspaceEntry(
    var typeId: String = "",
    var name: String = "",
    var showPath: Boolean = true,
    var options: Map<String, String> = emptyMap(),
)

/** SDK extension for adding configurable entry types to the HELIX Workspace pane. */
interface WorkspaceEntryProvider {
    val typeId: String
    val displayName: String
    val icon: Icon
    val allowMultiple: Boolean get() = true

    /** Returns the new value, or null when configuration was cancelled. */
    fun configure(project: Project, entry: WorkspaceEntry? = null): WorkspaceEntry?
    fun createNode(project: Project, entry: WorkspaceEntry, separatorAbove: Boolean): AbstractTreeNode<*>
    fun contains(project: Project, entry: WorkspaceEntry, file: VirtualFile): Boolean

    /** Whether a VFS path change can affect this entry. Used to invalidate the Workspace tree. */
    fun isAffectedByPath(project: Project, entry: WorkspaceEntry, path: String): Boolean = true

    companion object {
        @JvmField
        val EP_NAME: ExtensionPointName<WorkspaceEntryProvider> =
            ExtensionPointName.create("dev.helight.helix.workspaceEntryProvider")

        fun find(typeId: String): WorkspaceEntryProvider? =
            EP_NAME.extensionList.firstOrNull { it.typeId == typeId }
    }
}

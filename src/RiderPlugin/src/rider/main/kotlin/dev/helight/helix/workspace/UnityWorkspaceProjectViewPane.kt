package dev.helight.helix.workspace

import com.intellij.ide.SelectInTarget
import com.intellij.ide.impl.ProjectViewSelectInTarget
import com.intellij.ide.projectView.PresentationData
import com.intellij.ide.projectView.ProjectView
import com.intellij.openapi.actionSystem.ActionManager
import com.intellij.openapi.actionSystem.ActionUpdateThread
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.DefaultActionGroup
import com.intellij.openapi.components.service
import com.intellij.openapi.options.ShowSettingsUtil
import com.intellij.openapi.project.DumbAware
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.LocalFileSystem
import com.intellij.psi.PsiFileSystemItem
import com.jetbrains.rider.projectView.views.SolutionViewPaneBase
import com.jetbrains.rider.projectView.views.SolutionViewRootNodeBase
import dev.helight.helix.HelixMessagesBundle.message
import dev.helight.helix.HelixIcons
import java.nio.file.Files
import java.nio.file.Paths
import javax.swing.Icon

internal const val UNITY_WORKSPACE_PANE_ID = "HelixUnityWorkspace"
internal const val UNITY_WORKSPACE_REFRESH_ACTION_ID = "Helix.RefreshUnityWorkspace"

class UnityWorkspaceProjectViewPane(project: Project) :
    SolutionViewPaneBase(project, UnityWorkspaceRootNode(project)) {
    init {
        project.service<WorkspaceProjectViewRefreshService>()
    }
    override fun getTitle(): String = message("workspace.pane.title")
    override fun getId(): String = UNITY_WORKSPACE_PANE_ID
    override fun getIcon(): Icon = HelixIcons.Unity
    override fun getWeight(): Int = 20

    override fun createSelectInTarget(): SelectInTarget = UnityWorkspaceSelectInTarget(myProject)

    override fun isInitiallyVisible(): Boolean = isUnityProject(myProject)

    fun refreshWorkspace(refreshFileSystem: Boolean = false) {
        if (refreshFileSystem) LocalFileSystem.getInstance().refresh(false)
        updateFromRoot(true)
    }

    override fun addToolbarActions(actionGroup: DefaultActionGroup) {
        super.addToolbarActions(actionGroup)
        ActionManager.getInstance().getAction(UNITY_WORKSPACE_REFRESH_ACTION_ID)?.let(actionGroup::add)
        actionGroup.add(object : AnAction(
            message("workspace.action.configure"),
            message("workspace.action.configure.description"),
            HelixIcons.Settings,
        ) {
            override fun actionPerformed(event: AnActionEvent) {
                ShowSettingsUtil.getInstance().showSettingsDialog(myProject, WorkspaceSettingsConfigurable::class.java)
            }
        })
    }

    companion object {
        fun find(project: Project): UnityWorkspaceProjectViewPane? =
            ProjectView.getInstance(project).getProjectViewPaneById(UNITY_WORKSPACE_PANE_ID)
                as? UnityWorkspaceProjectViewPane
    }
}

class UnityWorkspaceRefreshAction : AnAction(), DumbAware {
    override fun getActionUpdateThread(): ActionUpdateThread = ActionUpdateThread.EDT

    override fun update(event: AnActionEvent) {
        val project = event.project
        event.presentation.isEnabledAndVisible = project != null &&
            ProjectView.getInstance(project).currentProjectViewPane is UnityWorkspaceProjectViewPane
    }

    override fun actionPerformed(event: AnActionEvent) {
        event.project?.let { UnityWorkspaceProjectViewPane.find(it)?.refreshWorkspace(refreshFileSystem = true) }
    }
}

private class UnityWorkspaceSelectInTarget(
    private val workspaceProject: Project,
) : ProjectViewSelectInTarget(workspaceProject), DumbAware {
    override fun toString(): String = message("workspace.pane.title")
    override fun getMinorViewId(): String = UNITY_WORKSPACE_PANE_ID
    override fun getWeight(): Float = 20f

    override fun canSelect(file: PsiFileSystemItem): Boolean {
        if (!file.virtualFile.isValid) return false
        return WorkspaceSettings.getInstance(workspaceProject).entries.any { entry ->
            WorkspaceEntryProvider.find(entry.typeId)?.contains(workspaceProject, entry, file.virtualFile) == true
        }
    }
}

private class UnityWorkspaceRootNode(project: Project) : SolutionViewRootNodeBase(project) {
    override fun calculateChildren() =
        WorkspaceSettings.getInstance(project).entries.mapIndexedNotNullTo(mutableListOf()) { index, entry ->
            WorkspaceEntryProvider.find(entry.typeId)?.createNode(project, entry, index > 0)
        }

    override fun update(presentation: PresentationData) {
        presentation.presentableText = message("workspace.pane.title")
        presentation.setIcon(HelixIcons.Unity)
    }
}

private fun isUnityProject(project: Project): Boolean {
    val root = project.basePath?.let(Paths::get) ?: return false
    return Files.isDirectory(root.resolve("Assets")) && Files.isDirectory(root.resolve("ProjectSettings"))
}

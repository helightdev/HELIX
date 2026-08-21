package dev.helight.helix.workspace

import com.intellij.openapi.Disposable
import com.intellij.openapi.application.EDT
import com.intellij.openapi.components.Service
import com.intellij.openapi.fileEditor.FileEditorManagerEvent
import com.intellij.openapi.fileEditor.FileEditorManagerListener
import com.intellij.ide.projectView.ProjectView
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFileManager
import com.intellij.openapi.vfs.newvfs.BulkFileListener
import com.intellij.openapi.vfs.newvfs.events.VFileEvent
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import kotlin.time.Duration.Companion.milliseconds

/** Gives the custom Workspace pane the VFS invalidation behavior of the normal project view. */
@Service(Service.Level.PROJECT)
internal class WorkspaceProjectViewRefreshService(
    private val project: Project,
    private val scope: CoroutineScope,
) : Disposable {
    private var pendingRefresh: Job? = null

    init {
        val connection = project.messageBus.connect(this)
        connection.subscribe(VirtualFileManager.VFS_CHANGES, object : BulkFileListener {
            override fun after(events: List<VFileEvent>) {
                if (events.any(::affectsWorkspace)) scheduleRefresh()
            }
        })
        connection.subscribe(FileEditorManagerListener.FILE_EDITOR_MANAGER, object : FileEditorManagerListener {
            override fun selectionChanged(event: FileEditorManagerEvent) {
                val file = event.newFile ?: return
                scope.launch(Dispatchers.EDT) {
                    UnityWorkspaceProjectViewPane.find(project)?.takeIf {
                        ProjectView.getInstance(project).currentProjectViewPane === it
                    }?.selectWorkspaceFile(file)
                }
            }
        })
    }

    private fun affectsWorkspace(event: VFileEvent): Boolean =
        WorkspaceSettings.getInstance(project).entries.any { entry ->
            WorkspaceEntryProvider.find(entry.typeId)?.isAffectedByPath(project, entry, event.path) == true
        }

    private fun scheduleRefresh() {
        pendingRefresh?.cancel()
        pendingRefresh = scope.launch {
            delay(REFRESH_DELAY)
            withContext(Dispatchers.EDT) {
                UnityWorkspaceProjectViewPane.find(project)?.refreshWorkspace()
            }
        }
    }

    override fun dispose() {
        pendingRefresh?.cancel()
    }

    private companion object {
        val REFRESH_DELAY = 100L.milliseconds
    }
}

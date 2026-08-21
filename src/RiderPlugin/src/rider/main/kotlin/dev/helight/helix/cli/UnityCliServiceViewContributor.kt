package dev.helight.helix.cli

import dev.helight.helix.HelixMessagesBundle.message
import dev.helight.helix.HelixIcons
import com.intellij.execution.services.ServiceViewContributor
import com.intellij.execution.services.ServiceViewDescriptor
import com.intellij.execution.services.SimpleServiceViewDescriptor
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.DefaultActionGroup
import com.intellij.openapi.application.EDT
import com.intellij.openapi.project.Project
import com.intellij.ui.components.JBLabel
import com.intellij.ui.components.JBPanel
import com.intellij.ui.components.JBScrollPane
import com.intellij.util.ui.JBUI
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.collectLatest
import kotlinx.coroutines.launch
import java.awt.BorderLayout
import javax.swing.JComponent
import javax.swing.JTextArea

internal class UnityCliServiceViewContributor : ServiceViewContributor<UnityCliProjectService> {
    override fun getViewDescriptor(project: Project): ServiceViewDescriptor =
        SimpleServiceViewDescriptor(message("unity.cli.name"), HelixIcons.Unity)

    override fun getServices(project: Project): List<UnityCliProjectService> =
        if (UnityCliProjectService.isUnityProject(project)) {
            listOf(UnityCliProjectService.getInstance(project))
        } else {
            emptyList()
        }

    override fun getServiceDescriptor(project: Project, service: UnityCliProjectService): ServiceViewDescriptor =
        UnityCliServiceViewDescriptor(service, service.coroutineScope)
}

private class UnityCliServiceViewDescriptor(
    private val service: UnityCliProjectService,
    scope: CoroutineScope,
) : SimpleServiceViewDescriptor(message("unity.cli.name"), HelixIcons.Unity) {
    private val stateLabel = JBLabel()
    private val details = JTextArea().apply {
        isEditable = false
        border = JBUI.Borders.empty(8)
    }
    private val content = JBPanel<JBPanel<*>>(BorderLayout()).apply {
        border = JBUI.Borders.empty(8)
        add(stateLabel, BorderLayout.NORTH)
        add(JBScrollPane(details), BorderLayout.CENTER)
    }

    init {
        scope.launch(Dispatchers.EDT) { service.status.collectLatest(::render) }
        service.refreshStatus()
    }

    override fun getContentComponent(): JComponent = content

    override fun getToolbarActions() =
        DefaultActionGroup(object : AnAction(
            message("unity.cli.action.refresh"),
            message("unity.cli.action.refresh.description"),
            HelixIcons.Refresh,
        ) {
            override fun actionPerformed(event: AnActionEvent) = service.refreshStatus()
        })

    private fun render(state: UnityCliStatusState) {
        stateLabel.icon = if (state.isUnityOnline()) HelixIcons.UnityOnline else HelixIcons.UnityOffline
        val (summary, body) = when (state) {
            UnityCliStatusState.NotLoaded -> message("unity.cli.status.not.loaded") to service.unityProjectPath
            is UnityCliStatusState.Loading -> message("unity.cli.status.refreshing") to snapshotText(state.previous)
            is UnityCliStatusState.Failed -> message("unity.cli.status.unavailable") to "${state.projectPath}\n\n${state.message}"
            is UnityCliStatusState.Loaded -> {
                val snapshot = state.snapshot
                val label = when {
                    !snapshot.success -> message("unity.cli.status.error", snapshot.exitCode.toString())
                    snapshot.instances.isEmpty() -> message("unity.cli.status.offline")
                    else -> message("unity.cli.status.online")
                }
                label to snapshotText(snapshot)
            }
        }
        stateLabel.text = summary
        details.text = body
        details.caretPosition = 0
    }

    private fun snapshotText(snapshot: UnityCliStatusSnapshot?): String = buildString {
        append(snapshot?.projectPath ?: service.unityProjectPath)
        snapshot ?: return@buildString
        snapshot.instances.firstOrNull()?.let { instance ->
            append("\n\n").append(message(
                "unity.cli.details.instance",
                instance.version,
                instance.state,
                instance.pid.toString(),
                instance.port.toString(),
            ))
        }
        snapshot.warnings.forEach { append("\n\n").append(message("unity.cli.details.warning", it.message)) }
        snapshot.errors.forEach { append("\n\n").append(message("unity.cli.details.error", it.message)) }
    }
}

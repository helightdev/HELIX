package dev.helight.helix.cli

import dev.helight.helix.HelixMessagesBundle.message
import com.intellij.openapi.project.Project
import com.intellij.openapi.application.EDT
import com.intellij.openapi.util.IconLoader
import com.intellij.openapi.wm.StatusBar
import com.intellij.openapi.wm.StatusBarWidget
import com.intellij.openapi.wm.StatusBarWidgetFactory
import com.intellij.util.Consumer
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.collectLatest
import kotlinx.coroutines.launch
import java.awt.event.MouseEvent
import javax.swing.Icon
import icons.UnityIcons

internal object UnityCliIcons {
    val online: Icon = IconLoader.getIcon(
        "/expui/resharper/Toolbar/UnityToolbarConnected.svg",
        UnityIcons::class.java,
    )
    val offline: Icon = IconLoader.getIcon(
        "/expui/resharper/Toolbar/UnityToolbarDisconnected.svg",
        UnityIcons::class.java,
    )
    val unity: Icon = UnityIcons.Icons.UnityLogo
}

internal class UnityCliStatusBarWidgetFactory : StatusBarWidgetFactory {
    override fun getId(): String = WIDGET_ID

    override fun getDisplayName(): String = message("unity.cli.name")

    override fun isAvailable(project: Project): Boolean =
        UnityCliProjectService.isUnityProject(project)

    override fun createWidget(project: Project, scope: CoroutineScope): StatusBarWidget =
        UnityCliStatusBarWidget(UnityCliProjectService.getInstance(project), scope)

    override fun isEnabledByDefault(): Boolean = true

    companion object {
        const val WIDGET_ID = "dev.helight.helix.UnityCliStatus"
    }
}

private class UnityCliStatusBarWidget(
    private val service: UnityCliProjectService,
    scope: CoroutineScope,
) : StatusBarWidget, StatusBarWidget.IconPresentation {
    private var statusBar: StatusBar? = null
    private var state: UnityCliStatusState = service.status.value

    init {
        scope.launch(Dispatchers.EDT) {
            service.status.collectLatest {
                state = it
                statusBar?.updateWidget(ID())
            }
        }
        service.refreshStatus()
    }

    override fun ID(): String = UnityCliStatusBarWidgetFactory.WIDGET_ID

    override fun getPresentation(): StatusBarWidget.WidgetPresentation = this

    override fun install(statusBar: StatusBar) {
        this.statusBar = statusBar
    }

    override fun dispose() {
        statusBar = null
    }

    override fun getIcon(): Icon = if (state.isUnityOnline()) UnityCliIcons.online else UnityCliIcons.offline

    override fun getTooltipText(): String = when (val current = state) {
        UnityCliStatusState.NotLoaded -> message("unity.cli.status.tooltip.not.loaded")
        is UnityCliStatusState.Loading -> message("unity.cli.status.tooltip.refreshing")
        is UnityCliStatusState.Failed -> message("unity.cli.status.tooltip.unavailable", current.message)
        is UnityCliStatusState.Loaded -> buildString {
            if (current.snapshot.instances.isEmpty()) {
                append(message("unity.cli.status.tooltip.no.instance", current.snapshot.projectPath))
            } else {
                val instance = current.snapshot.instances.first()
                append(message(
                    "unity.cli.status.tooltip.instance",
                    current.snapshot.projectPath,
                    instance.version,
                    instance.state,
                    instance.pid,
                ))
            }
            current.snapshot.errors.firstOrNull()?.let {
                replace(0, length, message("unity.cli.status.tooltip.error", toString(), it.message))
            }
        }
    }

    override fun getClickConsumer(): Consumer<MouseEvent> = Consumer { service.refreshStatus() }
}

internal fun UnityCliStatusState.isUnityOnline(): Boolean = when (this) {
    is UnityCliStatusState.Loaded -> snapshot.success && snapshot.instances.isNotEmpty()
    is UnityCliStatusState.Loading -> previous?.success == true && previous.instances.isNotEmpty()
    else -> false
}

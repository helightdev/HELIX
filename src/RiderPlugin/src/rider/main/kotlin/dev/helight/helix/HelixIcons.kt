package dev.helight.helix

import com.intellij.icons.AllIcons
import com.intellij.openapi.util.IconLoader
import icons.UnityIcons
import javax.swing.Icon

object HelixIcons {
    @JvmField val Unity: Icon = UnityIcons.Icons.UnityLogo
    @JvmField val UnityOnline: Icon = IconLoader.getIcon(
        "/expui/resharper/Toolbar/UnityToolbarConnected.svg",
        UnityIcons::class.java,
    )
    @JvmField val UnityOffline: Icon = IconLoader.getIcon(
        "/expui/resharper/Toolbar/UnityToolbarDisconnected.svg",
        UnityIcons::class.java,
    )
    @JvmField val UnityPackages: Icon = UnityIcons.Explorer.PackagesRoot
    @JvmField val Refresh: Icon = AllIcons.Actions.Refresh
    @JvmField val Settings: Icon = AllIcons.General.Settings
    @JvmField val Warning: Icon = AllIcons.General.Warning
    @JvmField val Folder: Icon = AllIcons.Nodes.Folder
}

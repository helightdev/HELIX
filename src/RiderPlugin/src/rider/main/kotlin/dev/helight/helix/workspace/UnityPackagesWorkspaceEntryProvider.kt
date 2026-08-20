package dev.helight.helix.workspace

import com.intellij.ide.projectView.PresentationData
import com.intellij.ide.util.treeView.AbstractTreeNode
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.LocalFileSystem
import com.intellij.openapi.vfs.VirtualFile
import com.jetbrains.rider.plugins.unity.explorer.PackagesRootNode
import dev.helight.helix.HelixMessagesBundle.message
import dev.helight.helix.HelixIcons
import java.nio.file.Paths
import javax.swing.Icon

/** Adds Rider Unity's package-model-backed Packages pseudo-directory to HELIX Workspace. */
class UnityPackagesWorkspaceEntryProvider : WorkspaceEntryProvider {
    override val typeId: String = TYPE_ID
    override val displayName: String get() = message("workspace.entry.unity.packages")
    override val icon: Icon = HelixIcons.UnityPackages
    override val allowMultiple: Boolean = false

    override fun configure(project: Project, entry: WorkspaceEntry?): WorkspaceEntry =
        entry ?: WorkspaceEntry(typeId = TYPE_ID, name = displayName, showPath = false)

    override fun createNode(project: Project, entry: WorkspaceEntry, separatorAbove: Boolean): AbstractTreeNode<*> {
        return packagesRoot(project) ?: MissingPackagesNode(project, separatorAbove)
    }

    override fun contains(project: Project, entry: WorkspaceEntry, file: VirtualFile): Boolean {
        val candidate = Paths.get(file.path).normalize()
        val physicalPackages = project.basePath?.let(Paths::get)?.resolve("Packages")?.normalize()
        if (physicalPackages != null && candidate.startsWith(physicalPackages)) return true
        return packagesRoot(project)?.contains(file) == true
    }

    override fun isAffectedByPath(project: Project, entry: WorkspaceEntry, path: String): Boolean {
        val projectRoot = project.basePath?.let(Paths::get)?.toAbsolutePath()?.normalize() ?: return false
        val changed = runCatching { Paths.get(path).toAbsolutePath().normalize() }.getOrNull() ?: return false
        return changed.startsWith(projectRoot.resolve("Packages")) ||
            changed.startsWith(projectRoot.resolve("Library/PackageCache"))
    }

    companion object {
        const val TYPE_ID = "unity-packages"

        private fun packagesRoot(project: Project): PackagesRootNode? {
            val packagesDirectory = project.basePath?.let(Paths::get)?.resolve("Packages") ?: return null
            val virtualDirectory = LocalFileSystem.getInstance().refreshAndFindFileByNioFile(packagesDirectory)
            return virtualDirectory?.takeIf(VirtualFile::isDirectory)?.let { PackagesRootNode(project, it) }
        }
    }

    private class MissingPackagesNode(
        project: Project,
        private val separatorAbove: Boolean,
    ) : AbstractTreeNode<String>(project, "Packages") {
        override fun getChildren(): Collection<AbstractTreeNode<*>> = emptyList()
        override fun isAlwaysLeaf(): Boolean = true
        override fun update(presentation: PresentationData) {
            presentation.presentableText = message("workspace.entry.unity.packages")
            presentation.locationString = message("workspace.entry.unity.packages.missing")
            presentation.setIcon(HelixIcons.Warning)
            presentation.setSeparatorAbove(separatorAbove)
        }
    }
}

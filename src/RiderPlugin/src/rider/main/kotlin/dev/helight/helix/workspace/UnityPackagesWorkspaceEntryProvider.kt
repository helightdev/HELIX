package dev.helight.helix.workspace

import com.intellij.ide.projectView.PresentationData
import com.intellij.ide.util.treeView.AbstractTreeNode
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.LocalFileSystem
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.openapi.vfs.VfsUtil
import com.intellij.platform.backend.workspace.WorkspaceModel
import com.intellij.psi.PsiManager
import com.jetbrains.rider.plugins.unity.workspace.getPackages
import com.jetbrains.rider.projectView.views.FileSystemNodeBase
import com.jetbrains.rider.projectView.views.NestingNode
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
        return packagesRoot(project, separatorAbove) ?: MissingPackagesNode(project, separatorAbove)
    }

    override fun contains(project: Project, entry: WorkspaceEntry, file: VirtualFile): Boolean {
        val candidate = Paths.get(file.path).normalize()
        val physicalPackages = project.basePath?.let(Paths::get)?.resolve("Packages")?.normalize()
        if (physicalPackages != null && candidate.startsWith(physicalPackages)) return true
        return packagesRoot(project, false)?.contains(file) == true
    }

    override fun isAffectedByPath(project: Project, entry: WorkspaceEntry, path: String): Boolean {
        val projectRoot = project.basePath?.let(Paths::get)?.toAbsolutePath()?.normalize() ?: return false
        val changed = runCatching { Paths.get(path).toAbsolutePath().normalize() }.getOrNull() ?: return false
        return changed.startsWith(projectRoot.resolve("Packages")) ||
            changed.startsWith(projectRoot.resolve("Library/PackageCache"))
    }

    companion object {
        const val TYPE_ID = "unity-packages"

        private fun packagesRoot(project: Project, separatorAbove: Boolean): WorkspacePackagesNode? {
            val packagesDirectory = project.basePath?.let(Paths::get)?.resolve("Packages") ?: return null
            val virtualDirectory = LocalFileSystem.getInstance().refreshAndFindFileByNioFile(packagesDirectory)
            return virtualDirectory?.takeIf(VirtualFile::isDirectory)?.let {
                WorkspacePackagesNode(project, it, separatorAbove = separatorAbove, includeModelPackages = true)
            }
        }
    }

    private class WorkspacePackagesNode(
        project: Project,
        file: VirtualFile,
        nestedFiles: List<NestingNode<VirtualFile>> = emptyList(),
        private val separatorAbove: Boolean = false,
        private val includeModelPackages: Boolean = false,
        private val isPackageRoot: Boolean = false,
    ) : FileSystemNodeBase(project, file, nestedFiles) {
        override fun createNode(
            virtualFile: VirtualFile,
            nestedFiles: List<NestingNode<VirtualFile>>,
        ): FileSystemNodeBase = WorkspacePackagesNode(
            project,
            virtualFile,
            nestedFiles,
            isPackageRoot = includeModelPackages && virtualFile.isDirectory,
        )

        override fun getVirtualFileChildren(): MutableList<VirtualFile> =
            super.getVirtualFileChildren()
                .filterNot { it.extension.equals("meta", ignoreCase = true) }
                .toMutableList()

        override fun calculateChildren(): MutableList<AbstractTreeNode<*>> {
            val children = super.calculateChildren()
            if (!includeModelPackages) return children

            val representedPaths = children.mapNotNullTo(mutableSetOf()) {
                (it as? FileSystemNodeBase)?.file?.path
            }
            WorkspaceModel.getInstance(project).getPackages().forEach { packageEntity ->
                val packageFolder = packageEntity.packageFolder ?: return@forEach
                if (representedPaths.add(packageFolder.path)) {
                    children.add(WorkspacePackagesNode(project, packageFolder, isPackageRoot = true))
                }
            }
            return children
        }

        override fun contains(file: VirtualFile): Boolean {
            if (super.contains(file)) return true
            return includeModelPackages && WorkspaceModel.getInstance(project).getPackages().any { packageEntity ->
                packageEntity.packageFolder?.let { VfsUtil.isAncestor(it, file, false) } == true
            }
        }

        override fun update(presentation: PresentationData) {
            presentation.presentableText = if (includeModelPackages) {
                message("workspace.entry.unity.packages")
            } else if (isPackageRoot) {
                file.name.substringBeforePackageSuffix()
            } else {
                file.name
            }
            presentation.setIcon(
                when {
                    includeModelPackages || isPackageRoot -> HelixIcons.UnityPackages
                    file.isDirectory -> HelixIcons.Folder
                    else -> PsiManager.getInstance(project).findFile(file)?.getIcon(0) ?: file.fileType.icon
                },
            )
            presentation.setSeparatorAbove(separatorAbove)
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

private fun String.substringBeforePackageSuffix(): String {
    val separator = lastIndexOf('@')
    return if (separator > 0) substring(0, separator) else this
}

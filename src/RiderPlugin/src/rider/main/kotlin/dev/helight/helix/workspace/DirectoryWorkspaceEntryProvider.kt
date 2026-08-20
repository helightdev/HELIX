package dev.helight.helix.workspace

import com.intellij.ide.projectView.PresentationData
import com.intellij.ide.util.treeView.AbstractTreeNode
import com.intellij.openapi.fileChooser.FileChooser
import com.intellij.openapi.fileChooser.FileChooserDescriptorFactory
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.DialogWrapper
import com.intellij.openapi.ui.ValidationInfo
import com.intellij.openapi.vfs.LocalFileSystem
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.psi.PsiManager
import com.intellij.ui.components.JBCheckBox
import com.intellij.ui.components.JBLabel
import com.intellij.openapi.ui.TextFieldWithBrowseButton
import com.intellij.util.ui.JBUI
import com.jetbrains.rider.projectView.views.FileSystemNodeBase
import com.jetbrains.rider.projectView.views.NestingNode
import dev.helight.helix.HelixMessagesBundle.message
import dev.helight.helix.HelixIcons
import java.awt.GridBagConstraints
import java.awt.GridBagLayout
import java.nio.file.Path
import java.nio.file.Paths
import javax.swing.Icon
import javax.swing.JComponent
import javax.swing.JPanel

class DirectoryWorkspaceEntryProvider : WorkspaceEntryProvider {
    override val typeId: String = TYPE_ID
    override val displayName: String get() = message("workspace.entry.directory")
    override val icon: Icon = HelixIcons.Folder

    override fun configure(project: Project, entry: WorkspaceEntry?): WorkspaceEntry? {
        val dialog = DirectoryEntryDialog(project, entry)
        return if (dialog.showAndGet()) dialog.result() else null
    }

    override fun createNode(project: Project, entry: WorkspaceEntry, separatorAbove: Boolean): AbstractTreeNode<*> {
        val configured = entry.options[PATH_OPTION].orEmpty()
        val path = resolve(project, configured)
        val directory = LocalFileSystem.getInstance().findFileByNioFile(path)
        return if (directory?.isDirectory == true) {
            DirectoryNode(
                project, directory, entry.name, configured, entry.showPath,
                entry.options[NESTING_OPTION]?.toBooleanStrictOrNull() ?: true,
                separatorAbove = separatorAbove,
            )
        } else {
            MissingDirectoryNode(project, entry, path, separatorAbove)
        }
    }

    override fun contains(project: Project, entry: WorkspaceEntry, file: VirtualFile): Boolean {
        val configured = entry.options[PATH_OPTION] ?: return false
        return runCatching { Paths.get(file.path).normalize().startsWith(resolve(project, configured)) }.getOrDefault(
            false
        )
    }

    override fun isAffectedByPath(project: Project, entry: WorkspaceEntry, path: String): Boolean {
        val configured = entry.options[PATH_OPTION] ?: return false
        val root = resolve(project, configured)
        return runCatching {
            val changed = Paths.get(path).toAbsolutePath().normalize()
            changed.startsWith(root) || root.startsWith(changed)
        }.getOrDefault(false)
    }

    companion object {
        const val TYPE_ID = "directory"
        private const val PATH_OPTION = "path"
        private const val NESTING_OPTION = "useFileNesting"

    }

    private class DirectoryEntryDialog(
        private val project: Project,
        entry: WorkspaceEntry?,
    ) : DialogWrapper(project) {
        private val nameField = javax.swing.JTextField(entry?.name.orEmpty(), 32)
        private val pathField = TextFieldWithBrowseButton().apply {
            text = entry?.options?.get(PATH_OPTION).orEmpty()
            addActionListener { chooseDirectory() }
        }
        private val showPath = JBCheckBox(message("workspace.entry.show.path"), entry?.showPath ?: true)
        private val useNesting = JBCheckBox(
            message("workspace.entry.directory.file.nesting"),
            entry?.options?.get(NESTING_OPTION)?.toBooleanStrictOrNull() ?: true,
        )

        init {
            title = message(if (entry == null) "workspace.entry.directory.add" else "workspace.entry.directory.edit")
            init()
        }

        override fun createCenterPanel(): JComponent = JPanel(GridBagLayout()).apply {
            val constraints = GridBagConstraints().apply {
                gridx = 0
                gridy = 0
                anchor = GridBagConstraints.WEST
                insets = JBUI.insets(4, 4, 4, 8)
            }
            add(JBLabel(message("workspace.entry.name")), constraints)
            constraints.gridx = 1
            constraints.weightx = 1.0
            constraints.fill = GridBagConstraints.HORIZONTAL
            add(nameField, constraints)
            constraints.gridx = 0
            constraints.gridy++
            constraints.weightx = 0.0
            constraints.fill = GridBagConstraints.NONE
            add(JBLabel(message("workspace.entry.directory.path")), constraints)
            constraints.gridx = 1
            constraints.weightx = 1.0
            constraints.fill = GridBagConstraints.HORIZONTAL
            add(pathField, constraints)
            constraints.gridx = 1
            constraints.gridy++
            add(showPath, constraints)
            constraints.gridy++
            add(useNesting, constraints)
        }

        override fun doValidate(): ValidationInfo? =
            if (pathField.text.isBlank()) ValidationInfo(message("workspace.entry.directory.path.required"), pathField)
            else null

        fun result(): WorkspaceEntry = WorkspaceEntry(
            typeId = TYPE_ID,
            name = nameField.text.trim(),
            showPath = showPath.isSelected,
            options = mapOf(PATH_OPTION to pathField.text.trim(), NESTING_OPTION to useNesting.isSelected.toString()),
        )

        private fun chooseDirectory() {
            FileChooser.chooseFile(
                FileChooserDescriptorFactory.createSingleFolderDescriptor()
                    .withTitle(message("workspace.settings.choose.directory")),
                project,
                null,
            )?.let { selected ->
                val selectedPath = Paths.get(selected.path).toAbsolutePath().normalize()
                val base = project.basePath?.let(Paths::get)?.toAbsolutePath()?.normalize()
                pathField.text =
                    if (base != null && selectedPath.startsWith(base)) base.relativize(selectedPath).toString()
                    else selectedPath.toString()
                if (nameField.text.isBlank()) nameField.text = selected.name
            }
        }
    }

    private class DirectoryNode(
        project: Project,
        file: VirtualFile,
        private val displayName: String = "",
        private val configuredPath: String? = null,
        private val showPath: Boolean = false,
        private val useFileNesting: Boolean = true,
        nestedFiles: List<NestingNode<VirtualFile>> = emptyList(),
        private val separatorAbove: Boolean = false,
    ) : FileSystemNodeBase(project, file, nestedFiles) {
        override fun createNode(
            virtualFile: VirtualFile,
            nestedFiles: List<NestingNode<VirtualFile>>
        ): FileSystemNodeBase =
            DirectoryNode(project, virtualFile, useFileNesting = useFileNesting, nestedFiles = nestedFiles)

        override fun getVirtualFileChildren(): MutableList<VirtualFile> =
            super.getVirtualFileChildren().filterNot { it.extension.equals("meta", ignoreCase = true) }.toMutableList()

        override fun calculateChildren(): MutableList<AbstractTreeNode<*>> {
            if (useFileNesting) return super.calculateChildren()
            if (!file.isDirectory) return mutableListOf()
            return getVirtualFileChildren().mapTo(mutableListOf()) { createNode(it, emptyList()) }
        }

        override fun update(presentation: PresentationData) {
            presentation.presentableText = displayName.ifBlank { file.name }
            presentation.locationString = configuredPath?.takeIf { showPath }
            presentation.setIcon(if (file.isDirectory) HelixIcons.Folder else fileIcon(file, project))
            presentation.setSeparatorAbove(separatorAbove)
        }
    }

    private class MissingDirectoryNode(
        project: Project,
        private val entry: WorkspaceEntry,
        private val resolvedPath: Path,
        private val separatorAbove: Boolean,
    ) : AbstractTreeNode<WorkspaceEntry>(project, entry) {
        override fun getChildren(): Collection<AbstractTreeNode<*>> = emptyList()
        override fun isAlwaysLeaf(): Boolean = true
        override fun update(presentation: PresentationData) {
            presentation.presentableText = entry.name.ifBlank { entry.options[PATH_OPTION].orEmpty() }
            presentation.locationString = if (entry.showPath) message("workspace.directory.missing", resolvedPath)
            else message("workspace.directory.missing.short")
            presentation.setIcon(HelixIcons.Warning)
            presentation.setSeparatorAbove(separatorAbove)
        }
    }
}


private fun fileIcon(file: VirtualFile, project: Project): Icon? =
    PsiManager.getInstance(project).findFile(file)?.getIcon(0) ?: file.fileType.icon

internal fun resolve(project: Project, configured: String): Path {
    val path = Paths.get(configured)
    return (if (path.isAbsolute) path else project.basePath?.let(Paths::get)?.resolve(path) ?: path)
        .toAbsolutePath().normalize()
}

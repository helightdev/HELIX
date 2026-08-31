package dev.helight.helix.mixin

import com.intellij.codeHighlighting.BackgroundEditorHighlighter
import com.intellij.codeInsight.daemon.impl.analysis.FileHighlightingSetting
import com.intellij.codeInsight.daemon.impl.analysis.HighlightingSettingsPerFile
import com.intellij.openapi.Disposable
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.Service
import com.intellij.openapi.components.service
import com.intellij.openapi.editor.Document
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.editor.event.DocumentEvent
import com.intellij.openapi.editor.event.DocumentListener
import com.intellij.openapi.fileEditor.FileDocumentManager
import com.intellij.openapi.fileEditor.FileEditor
import com.intellij.openapi.fileEditor.FileEditorLocation
import com.intellij.openapi.fileEditor.FileEditorPolicy
import com.intellij.openapi.fileEditor.FileEditorProvider
import com.intellij.openapi.fileEditor.FileEditorState
import com.intellij.openapi.fileEditor.FileEditorStateLevel
import com.intellij.openapi.fileEditor.NavigatableFileEditor
import com.intellij.openapi.fileEditor.OpenFileDescriptor
import com.intellij.openapi.fileEditor.TextEditor
import com.intellij.openapi.fileEditor.impl.text.TextEditorProvider
import com.intellij.openapi.project.DumbAware
import com.intellij.openapi.project.Project
import com.intellij.openapi.util.Disposer
import com.intellij.openapi.util.UserDataHolderBase
import com.intellij.openapi.vfs.LocalFileSystem
import com.intellij.openapi.vfs.VfsUtil
import com.intellij.openapi.vfs.VfsUtilCore
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.pom.Navigatable
import com.intellij.psi.PsiManager
import com.intellij.testFramework.LightVirtualFile
import com.intellij.util.SingleAlarm
import java.awt.BorderLayout
import java.beans.PropertyChangeListener
import java.beans.PropertyChangeSupport
import java.util.concurrent.ConcurrentHashMap
import java.util.concurrent.CopyOnWriteArrayList
import javax.swing.JButton
import javax.swing.JComponent
import javax.swing.JLabel
import javax.swing.JPanel

/** Directory-scoped IDE-only files keep edits detached from disk until the whole workspace is saved. */
@Service(Service.Level.PROJECT)
class HelixMixinDetachedWorkspaceService(private val project: Project) : Disposable {
    private val workspaces = ConcurrentHashMap<String, DetachedWorkspace>()

    fun workspaceFor(file: VirtualFile): DetachedWorkspace {
        val directory = normalise(file.parent?.path ?: file.path)
        return workspaces.computeIfAbsent(directory) { DetachedWorkspace(project, file.parent ?: file) }
    }

    fun detachedFile(originalPath: String): VirtualFile? = workspaces.values.asSequence()
        .mapNotNull { it.detachedFile(originalPath) }.firstOrNull()

    override fun dispose() {
        workspaces.values.forEach(Disposer::dispose)
        workspaces.clear()
    }

    companion object {
        fun getInstance(project: Project): HelixMixinDetachedWorkspaceService = project.service()
        private fun normalise(path: String) = path.replace('\\', '/')
    }
}

class DetachedWorkspace internal constructor(
    private val project: Project,
    private val sourceDirectory: VirtualFile
) : Disposable {
    data class Entry(
        val original: VirtualFile,
        val detached: VirtualFile,
        val document: Document,
        var savedText: String
    )

    private val languageService = HelixMixinSnapshotService.getInstance(project)
    private val entriesByOriginal = LinkedHashMap<String, Entry>()
    private val listeners = CopyOnWriteArrayList<() -> Unit>()
    private val analysisAlarm = SingleAlarm(Runnable { requestAnalysis() }, 180, this)

    init {
        val originals = sourceDirectory.children
            .filter { !it.isDirectory && HelixMixinFileType.isCanonical(it.name) }
            .sortedBy { it.name.lowercase() }
        for (original in originals) {
            val text = VfsUtilCore.loadText(original)
            val detached = LightVirtualFile(original.name, HelixMixinFileType, text).apply {
                charset = original.charset
            }
            detached.putUserData(HelixMixinSnapshotService.ORIGINAL_PATH, original.path)
            val document = FileDocumentManager.getInstance().getDocument(detached)
                ?: error("Unable to create detached mixin document for ${original.path}")
            PsiManager.getInstance(project).findFile(detached)?.let { psiFile ->
                HighlightingSettingsPerFile.getInstance(project).setHighlightingSettingForRoot(
                    psiFile,
                    FileHighlightingSetting.FORCE_HIGHLIGHTING
                )
            }
            val entry = Entry(original, detached, document, text)
            entriesByOriginal[normalise(original.path)] = entry
            languageService.openBuffer(original, text)
            document.addDocumentListener(object : DocumentListener {
                override fun documentChanged(event: DocumentEvent) {
                    languageService.updateOpenBuffer(original.path, event.document.text)
                    listeners.forEach { it() }
                    analysisAlarm.cancelAndRequest()
                }
            }, this)
        }
        requestAnalysis()
    }

    fun entryFor(original: VirtualFile): Entry = entriesByOriginal[normalise(original.path)]
        ?: error("Mixin ${original.path} is outside detached directory ${sourceDirectory.path}")

    fun detachedFile(originalPath: String): VirtualFile? =
        entriesByOriginal[normalise(originalPath)]?.detached

    fun addChangeListener(listener: () -> Unit): AutoCloseable {
        listeners += listener
        return AutoCloseable { listeners -= listener }
    }

    val isModified: Boolean get() = entriesByOriginal.values.any { it.document.text != it.savedText }
    val originals: List<VirtualFile> get() = entriesByOriginal.values.map { it.original }

    fun saveAll() {
        ApplicationManager.getApplication().runWriteAction {
            for (entry in entriesByOriginal.values) VfsUtil.saveText(entry.original, entry.document.text)
        }
        for (entry in entriesByOriginal.values) {
            entry.savedText = entry.document.text
            languageService.updateOpenBuffer(entry.original.path, entry.savedText)
        }
        listeners.forEach { it() }
        requestAnalysis()
    }

    fun requestAnalysis() {
        val origin = entriesByOriginal.values.firstOrNull() ?: return
        if (!project.isDisposed && origin.original.isValid)
            languageService.requestDirectory(origin.original, origin.document.text)
    }

    override fun dispose() {
        analysisAlarm.cancel()
        entriesByOriginal.values.forEach { languageService.closeOpenBuffer(it.original.path) }
        entriesByOriginal.clear()
    }

    private fun normalise(path: String) = path.replace('\\', '/')
}

class HelixMixinDetachedEditorProvider : FileEditorProvider, DumbAware {
    override fun accept(project: Project, file: VirtualFile): Boolean =
        !file.isDirectory && HelixMixinFileType.isCanonical(file.name)
    override fun createEditor(project: Project, file: VirtualFile): FileEditor {
        val originalPath = file.getUserData(HelixMixinSnapshotService.ORIGINAL_PATH)
        val original = if (originalPath == null) file else
            HelixMixinSnapshotService.getInstance(project).virtualFile(originalPath)
                ?: LocalFileSystem.getInstance().findFileByPath(originalPath)
                ?: error("Unable to find original mixin $originalPath")
        return HelixMixinDetachedEditor(project, original)
    }
    override fun getEditorTypeId(): String = "helix-mixin-directory-editor"
    override fun getPolicy(): FileEditorPolicy = FileEditorPolicy.HIDE_DEFAULT_EDITOR
}

private class HelixMixinDetachedEditor(
    private val project: Project,
    private val sourceFile: VirtualFile
) : UserDataHolderBase(), TextEditor, Disposable {
    private val propertyChanges = PropertyChangeSupport(this)
    private val workspace = HelixMixinDetachedWorkspaceService.getInstance(project).workspaceFor(sourceFile)
    private val entry = workspace.entryFor(sourceFile)
    private val delegate = TextEditorProvider.getInstance().createEditor(project, entry.detached) as TextEditor
    private val status = JLabel()
    private val save = JButton("Save all mixins")
    private val panel = JPanel(BorderLayout())
    private var previousModified = workspace.isModified
    private var workspaceListener: AutoCloseable? = null

    init {
        save.toolTipText = "Write every detached mixin in this directory to disk together"
        save.addActionListener {
            workspace.saveAll()
            status.text = "Saved all mixins — Unity may reimport the directory"
        }
        panel.add(JPanel(BorderLayout()).apply {
            add(status, BorderLayout.CENTER)
            add(JPanel().apply { add(save) }, BorderLayout.EAST)
        }, BorderLayout.NORTH)
        panel.add(delegate.component, BorderLayout.CENTER)
        workspaceListener = workspace.addChangeListener(::updateState)
        updateState()
    }

    private fun updateState() {
        val modified = workspace.isModified
        save.isEnabled = modified
        if (status.text.isNullOrEmpty() || modified) status.text = if (modified)
            "Detached mixin changes — save writes the whole directory" else "Ready"
        if (modified != previousModified) {
            propertyChanges.firePropertyChange(FileEditor.getPropModified(), previousModified, modified)
            previousModified = modified
        }
    }

    override fun getEditor(): Editor = delegate.editor
    override fun getComponent(): JComponent = panel
    override fun getPreferredFocusedComponent(): JComponent = delegate.preferredFocusedComponent ?: delegate.component
    override fun getName(): String = "Mixin directory"
    // The text editor and its PSI must agree on the edited virtual file. Returning the
    // on-disk source here makes Rider apply daemon policy to a different document.
    override fun getFile(): VirtualFile = entry.detached
    override fun getFilesToRefresh(): List<VirtualFile> = workspace.originals
    override fun isModified(): Boolean = workspace.isModified
    override fun isValid(): Boolean = sourceFile.isValid && delegate.isValid
    override fun isEditorLoaded(): Boolean = true
    override fun getState(level: FileEditorStateLevel): FileEditorState = delegate.getState(level)
    override fun setState(state: FileEditorState) = delegate.setState(state)
    override fun selectNotify() = delegate.selectNotify()
    override fun deselectNotify() = delegate.deselectNotify()
    override fun getBackgroundHighlighter(): BackgroundEditorHighlighter? = delegate.backgroundHighlighter
    override fun getCurrentLocation(): FileEditorLocation? = delegate.currentLocation
    override fun addPropertyChangeListener(listener: PropertyChangeListener) = propertyChanges.addPropertyChangeListener(listener)
    override fun removePropertyChangeListener(listener: PropertyChangeListener) = propertyChanges.removePropertyChangeListener(listener)

    override fun canNavigateTo(navigatable: Navigatable): Boolean =
        navigatable is OpenFileDescriptor && navigatable.file == sourceFile ||
            (delegate as NavigatableFileEditor).canNavigateTo(navigatable)

    override fun navigateTo(navigatable: Navigatable) {
        if (navigatable is OpenFileDescriptor && navigatable.file == sourceFile) {
            val offset = navigatable.offset.coerceIn(0, entry.document.textLength)
            (delegate as NavigatableFileEditor).navigateTo(OpenFileDescriptor(project, entry.detached, offset))
        } else (delegate as NavigatableFileEditor).navigateTo(navigatable)
    }

    override fun dispose() {
        workspaceListener?.close()
        workspaceListener = null
        Disposer.dispose(delegate)
    }
}

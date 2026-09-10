package dev.helight.helix.hix

import com.intellij.codeHighlighting.BackgroundEditorHighlighter
import com.intellij.codeInsight.daemon.impl.analysis.FileHighlightingSetting
import com.intellij.codeInsight.daemon.impl.analysis.HighlightingSettingsPerFile
import com.intellij.icons.AllIcons
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
import com.intellij.openapi.fileEditor.FileEditorManager
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
import com.intellij.ide.util.PropertiesComponent
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
import javax.swing.JCheckBox
import javax.swing.JComboBox
import javax.swing.JComponent
import javax.swing.JPanel
import com.intellij.openapi.ui.Messages

/** Directory-scoped IDE-only files keep edits detached from disk until the whole workspace is saved. */
@Service(Service.Level.PROJECT)
class HixDetachedWorkspaceService(private val project: Project) : Disposable {
    private val workspaces = ConcurrentHashMap<String, DetachedWorkspace>()

    fun workspaceFor(file: VirtualFile, buffered: Boolean): DetachedWorkspace {
        val directory = normalise(file.parent?.path ?: file.path)
        val key = "$directory|$buffered"
        return workspaces.computeIfAbsent(key) { DetachedWorkspace(project, file.parent ?: file, buffered) }
    }

    fun reset(file: VirtualFile) {
        val directory = normalise(file.parent?.path ?: file.path)
        workspaces.keys.filter { it.startsWith("$directory|") }.forEach { key ->
            workspaces.remove(key)?.let(Disposer::dispose)
        }
    }

    fun detachedFile(originalPath: String): VirtualFile? = workspaces.values.asSequence()
        .mapNotNull { it.detachedFile(originalPath) }.firstOrNull()

    override fun dispose() {
        workspaces.values.forEach(Disposer::dispose)
        workspaces.clear()
    }

    companion object {
        fun getInstance(project: Project): HixDetachedWorkspaceService = project.service()
        private fun normalise(path: String) = path.replace('\\', '/')
    }
}

class DetachedWorkspace internal constructor(
    private val project: Project,
    private val sourceDirectory: VirtualFile,
    val buffered: Boolean
) : Disposable {
    data class Entry(
        val original: VirtualFile,
        val detached: VirtualFile,
        val document: Document,
        var savedText: String,
        var loadedText: String
    )

    private val languageService = HixSnapshotService.getInstance(project)
    private val entriesByOriginal = LinkedHashMap<String, Entry>()
    private val listeners = CopyOnWriteArrayList<() -> Unit>()
    private val analysisAlarm = SingleAlarm(Runnable { requestAnalysis() }, 180, this)

    init {
        val originals = sourceDirectory.children
            .filter { !it.isDirectory && HixFileType.isCanonical(it.name) }
            .sortedBy { it.name.lowercase() }
        for (original in originals) {
            val text = VfsUtilCore.loadText(original)
            val detached = if (buffered) LightVirtualFile(original.name, HixFileType, text).apply {
                charset = original.charset
                putUserData(HixSnapshotService.ORIGINAL_PATH, original.path)
            } else original
            val document = FileDocumentManager.getInstance().getDocument(detached)
                ?: error("Unable to create detached mixin document for ${original.path}")
            PsiManager.getInstance(project).findFile(detached)?.let { psiFile ->
                HighlightingSettingsPerFile.getInstance(project).setHighlightingSettingForRoot(
                    psiFile,
                    FileHighlightingSetting.FORCE_HIGHLIGHTING
                )
            }
            val entry = Entry(original, detached, document, document.text, text)
            entriesByOriginal[normalise(original.path)] = entry
            languageService.openBuffer(original, document.text)
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

    val isModified: Boolean get() = entriesByOriginal.values.any {
        if (buffered) it.document.text != it.savedText
        else FileDocumentManager.getInstance().isDocumentUnsaved(it.document)
    }
    val originals: List<VirtualFile> get() = entriesByOriginal.values.map { it.original }

    fun saveAll() {
        if (buffered) ApplicationManager.getApplication().runWriteAction {
            for (entry in entriesByOriginal.values) VfsUtil.saveText(entry.original, entry.document.text)
        } else entriesByOriginal.values.forEach { FileDocumentManager.getInstance().saveDocument(it.document) }
        for (entry in entriesByOriginal.values) {
            entry.savedText = entry.document.text
            entry.loadedText = entry.document.text
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

    fun revalidate() {
        analysisAlarm.cancel()
        val origin = entriesByOriginal.values.firstOrNull() ?: return
        if (!project.isDisposed && origin.original.isValid) {
            languageService.refreshLanguageCatalog()
            languageService.revalidateDirectory(origin.original, origin.document.text)
        }
    }

    fun refreshFromDisk() {
        analysisAlarm.cancel()
        entriesByOriginal.values.forEach { it.original.refresh(false, false) }
        val diskTexts = entriesByOriginal.values.associateWith { VfsUtilCore.loadText(it.original) }
            .filter { (entry, diskText) -> diskText != entry.loadedText }
        ApplicationManager.getApplication().runWriteAction {
            diskTexts.forEach { (entry, diskText) ->
                if (entry.document.text != diskText) entry.document.setText(diskText)
                entry.savedText = diskText
                entry.loadedText = diskText
            }
        }
        diskTexts.forEach { (entry, diskText) ->
            languageService.updateOpenBuffer(entry.original.path, diskText)
        }
        listeners.forEach { it() }
        revalidate()
    }

    fun hasExternalChanges(): Boolean {
        entriesByOriginal.values.forEach { it.original.refresh(false, false) }
        return entriesByOriginal.values.any { VfsUtilCore.loadText(it.original) != it.loadedText }
    }

    fun discardChanges() {
        analysisAlarm.cancel()
        ApplicationManager.getApplication().runWriteAction {
            for (entry in entriesByOriginal.values) {
                if (entry.document.text != entry.savedText)
                    entry.document.setText(entry.savedText)
            }
        }
        for (entry in entriesByOriginal.values)
            languageService.updateOpenBuffer(entry.original.path, entry.savedText)
        listeners.forEach { it() }
        revalidate()
    }

    override fun dispose() {
        analysisAlarm.cancel()
        entriesByOriginal.values.forEach { languageService.closeOpenBuffer(it.original.path) }
        entriesByOriginal.clear()
    }

    private fun normalise(path: String) = path.replace('\\', '/')
}

class HixDetachedEditorProvider : FileEditorProvider, DumbAware {
    override fun accept(project: Project, file: VirtualFile): Boolean =
        !file.isDirectory && HixFileType.isCanonical(file.name)
    override fun createEditor(project: Project, file: VirtualFile): FileEditor {
        val originalPath = file.getUserData(HixSnapshotService.ORIGINAL_PATH)
        val original = if (originalPath == null) file else
            HixSnapshotService.getInstance(project).virtualFile(originalPath)
                ?: LocalFileSystem.getInstance().findFileByPath(originalPath)
                ?: error("Unable to find original mixin $originalPath")
        return HixDetachedEditor(project, original)
    }
    override fun getEditorTypeId(): String = "helix-mixin-directory-editor"
    override fun getPolicy(): FileEditorPolicy = FileEditorPolicy.HIDE_DEFAULT_EDITOR
}

private class HixDetachedEditor(
    private val project: Project,
    private val sourceFile: VirtualFile
) : UserDataHolderBase(), TextEditor, Disposable {
    private val propertyChanges = PropertyChangeSupport(this)
    private val properties = PropertiesComponent.getInstance(project)
    private val buffered = properties.getBoolean(BUFFERED_PROPERTY, true)
    private val workspace = HixDetachedWorkspaceService.getInstance(project).workspaceFor(sourceFile, buffered)
    private val entry = workspace.entryFor(sourceFile)
    private val delegate = TextEditorProvider.getInstance().createEditor(project, entry.detached) as TextEditor
    private val bufferedToggle = JCheckBox("Buffered", buffered)
    private val backend = JComboBox(HixSnapshotService.ANALYSIS_BACKENDS)
    private val save = object : JButton(AllIcons.Actions.MenuSaveall) {
        // Darcula paints default/action buttons with the primary action colour. Reporting this
        // directly avoids installing the button as the IDE window's Enter-key default action.
        override fun isDefaultButton(): Boolean = isEnabled
    }
    private val refresh = JButton(AllIcons.Actions.Refresh)
    private val discard = JButton(AllIcons.Actions.Rollback)
    private val panel = JPanel(BorderLayout())
    private var previousModified = workspace.isModified
    private var workspaceListener: AutoCloseable? = null

    init {
        val languageService = HixSnapshotService.getInstance(project)
        val storedBackend = properties.getValue(BACKEND_PROPERTY, HixSnapshotService.DEFAULT_BACKEND)
        languageService.useAnalysisBackend(storedBackend)
        backend.selectedItem = languageService.analysisBackend
        bufferedToggle.toolTipText = "Keep directory edits in virtual files until Save all mixins"
        bufferedToggle.addActionListener {
            val selected = bufferedToggle.isSelected
            if (selected == workspace.buffered) return@addActionListener
            if (workspace.isModified && Messages.showYesNoDialog(
                    project,
                    "Switching editor mode requires saving the current mixin changes first.",
                    "Switch Hix Editor Mode",
                    "Save and Switch",
                    "Cancel",
                    Messages.getQuestionIcon()) != Messages.YES) {
                bufferedToggle.isSelected = workspace.buffered
                return@addActionListener
            }
            if (workspace.isModified) workspace.saveAll()
            properties.setValue(BUFFERED_PROPERTY, selected, true)
            ApplicationManager.getApplication().invokeLater {
                val manager = FileEditorManager.getInstance(project)
                manager.closeFile(sourceFile)
                HixDetachedWorkspaceService.getInstance(project).reset(sourceFile)
                if (sourceFile.isValid) manager.openFile(sourceFile, true)
            }
        }
        backend.toolTipText = "Analyzer backend used for Hix diagnostics, documentation, and completion"
        backend.addActionListener {
            val selected = backend.selectedItem as? String ?: return@addActionListener
            properties.setValue(BACKEND_PROPERTY, selected, HixSnapshotService.DEFAULT_BACKEND)
            languageService.useAnalysisBackend(selected)
            workspace.revalidate()
        }
        save.toolTipText = "Write every detached mixin in this directory to disk together"
        save.addActionListener {
            workspace.saveAll()
        }
        refresh.toolTipText = "Reload the directory from disk and revalidate it"
        refresh.addActionListener {
            if (workspace.isModified && workspace.hasExternalChanges() && Messages.showYesNoDialog(
                    project,
                    "Files on disk changed while this directory also has unsaved edits. Reload the changed files from disk?",
                    "Refresh Hix Files",
                    "Reload",
                    "Cancel",
                    Messages.getWarningIcon()) != Messages.YES) return@addActionListener
            workspace.refreshFromDisk()
        }
        discard.toolTipText = "Restore every unsaved mixin in this directory"
        discard.addActionListener {
            workspace.discardChanges()
        }
        panel.add(JPanel(BorderLayout()).apply {
            add(JPanel().apply {
                add(backend)
                add(bufferedToggle)
            }, BorderLayout.WEST)
            add(JPanel().apply {
                add(refresh)
                add(discard)
                add(save)
            }, BorderLayout.EAST)
        }, BorderLayout.NORTH)
        panel.add(delegate.component, BorderLayout.CENTER)
        workspaceListener = workspace.addChangeListener(::updateState)
        updateState()
    }

    private fun updateState() {
        val modified = workspace.isModified
        save.isEnabled = modified
        discard.isEnabled = modified
        save.isVisible = workspace.buffered
        discard.isVisible = workspace.buffered
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

    companion object {
        private const val BUFFERED_PROPERTY = "helix.hix.editor.buffered"
        private const val BACKEND_PROPERTY = "helix.hix.analysis.backend"
    }
}

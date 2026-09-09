package dev.helight.helix.hix

import com.intellij.codeInsight.hints.ChangeListener
import com.intellij.codeInsight.hints.ImmediateConfigurable
import com.intellij.codeInsight.hints.InlayHintsCollector
import com.intellij.codeInsight.hints.InlayHintsProvider
import com.intellij.codeInsight.hints.InlayHintsSink
import com.intellij.codeInsight.hints.NoSettings
import com.intellij.codeInsight.hints.SettingsKey
import com.intellij.codeInsight.hints.presentation.PresentationFactory
import com.intellij.openapi.editor.Editor
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile
import javax.swing.JComponent
import javax.swing.JPanel

class HixInlayHintsProvider : InlayHintsProvider<NoSettings> {
    override val key = SettingsKey<NoSettings>("hix.inferred.types")
    override val name = "Hix inferred types"
    override val previewText = "local value = <text>"
    override fun createSettings() = NoSettings()
    override fun createConfigurable(settings: NoSettings) = object : ImmediateConfigurable {
        override fun createComponent(listener: ChangeListener): JComponent = JPanel()
    }

    override fun getCollectorFor(file: PsiFile, editor: Editor, settings: NoSettings,
                                 sink: InlayHintsSink): InlayHintsCollector {
        val presentation = PresentationFactory(editor)
        return object : InlayHintsCollector {
          override fun collect(element: PsiElement, editor: Editor, sink: InlayHintsSink): Boolean {
            if (element === file) {
                val service = HixSnapshotService.getInstance(file.project)
                val path = file.virtualFile?.path
                val snapshot = file.getUserData(HixSnapshotService.SEMANTIC_SNAPSHOT)
                    ?: service.snapshotForText(file.text, path)
                snapshot?.typeFacts?.filter { it.inlay && it.type != "any" }?.forEach { fact ->
                    val offset = fact.range.endOffset.coerceIn(0, file.textLength)
                    val text = if (fact.kind == "Coercion") "→ ${fact.type}" else ": ${fact.type}"
                    val boxed = presentation.roundWithBackground(
                        presentation.smallText(text))
                    sink.addInlineElement(offset, true,
                        presentation.withTooltip(fact.documentation, boxed), false)
                }
            }
            return true
          }
        }
    }
}

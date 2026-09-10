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
import dev.helight.helix.protocol.MixinTypeFact
import java.util.IdentityHashMap
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
        val service = HixSnapshotService.getInstance(file.project)
        val snapshot = file.getUserData(HixSnapshotService.SEMANTIC_SNAPSHOT)
            ?: service.snapshotForText(file.text, file.virtualFile?.path)
        val factsByElement = IdentityHashMap<PsiElement, MutableList<MixinTypeFact>>()
        snapshot?.typeFacts?.filter { it.inlay && it.type != "any" }?.forEach { fact ->
            owner(file, fact)?.let { factsByElement.computeIfAbsent(it) { mutableListOf() }.add(fact) }
        }
        return object : InlayHintsCollector {
          override fun collect(element: PsiElement, editor: Editor, sink: InlayHintsSink): Boolean {
            factsByElement[element]?.forEach { fact ->
                    val isLocalType = fact.kind == "Type"
                    val offset = if (isLocalType) localKeyword(element)?.textRange?.endOffset
                        ?: element.textRange.startOffset else element.textRange.endOffset
                    val text = if (isLocalType) " ${fact.type} " else "→ ${fact.type}"
                    val boxed = presentation.roundWithBackground(
                        presentation.smallText(text))
                    sink.addInlineElement(offset, !isLocalType,
                        presentation.withTooltip(fact.documentation, boxed), false)
            }
            return true
          }
        }
    }

    private fun owner(file: PsiFile, fact: MixinTypeFact): PsiElement? {
        val start = fact.range.startOffset
        val end = fact.range.endOffset
        if (start !in 0 until file.textLength || end !in (start + 1)..file.textLength) return null
        val candidates = generateSequence(file.findElementAt(start)) { it.parent }
            .takeWhile { it !== file.parent }
            .filter { it.textRange.startOffset == start && it.textRange.endOffset == end }
        return if (fact.kind == "Type") candidates.firstOrNull { it is HixDeclarationElement }
            else candidates.firstOrNull { it is HixPsiElement }
    }

    private fun localKeyword(owner: PsiElement): PsiElement? {
        val localDeclarationType = HelixAntlrTypes.rules[
            dev.helight.helix.hix.generated.HixParser.RULE_localDeclarationStatement]
        val declaration = generateSequence(owner.parent) { it.parent }
            .firstOrNull { it.node?.elementType == localDeclarationType } ?: return null
        fun leaf(element: PsiElement): PsiElement? {
            if (element.firstChild == null) return element.takeIf { it.text == "local" }
            var child = element.firstChild
            while (child != null) {
                leaf(child)?.let { return it }
                child = child.nextSibling
            }
            return null
        }
        return leaf(declaration)
    }
}

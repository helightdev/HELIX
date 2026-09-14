package dev.helight.helix.hix

import com.intellij.codeInsight.hints.declarative.InlayHintsCollector
import com.intellij.codeInsight.hints.declarative.InlayHintsProvider
import com.intellij.codeInsight.hints.declarative.InlayTreeSink
import com.intellij.codeInsight.hints.declarative.InlineInlayPosition
import com.intellij.codeInsight.hints.declarative.SharedBypassCollector
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.project.DumbAware
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile

/** Declarative inline hints use the platform settings, preview and context menu. */
class HixInlayHintsProvider : InlayHintsProvider, DumbAware {
    override fun createCollector(file: PsiFile, editor: Editor): InlayHintsCollector {
        // Hash-matched snapshots only: never attach stale compiler ranges to a changed buffer.
        val snapshot = HixSnapshotService.getInstance(file.project).snapshotForText(file.text, file.virtualFile?.path)
        val facts = snapshot?.typeFacts.orEmpty().filter {
            it.inlay && it.type != "any" && it.range.startOffset >= 0 &&
                it.range.endOffset in (it.range.startOffset + 1)..file.textLength
        }.distinctBy { Triple(it.range.endOffset, it.kind, it.type) }
        return object : SharedBypassCollector {
            override fun collectFromElement(element: PsiElement, sink: InlayTreeSink) {
                if (element !== file) return
                facts.forEach { fact ->
                    val inferred = fact.kind == "Type"
                    sink.whenOptionEnabled(if (inferred) "inferredTypes" else "conversions") {
                        sink.addPresentation(InlineInlayPosition(fact.range.endOffset, true),
                            tooltip = fact.documentation.takeIf { it.isNotBlank() }, hasBackground = true) {
                            text(if (inferred) ": ${fact.type}" else " → ${fact.type}")
                        }
                    }
                }
            }
        }
    }
}

package dev.helight.helix.hix

import com.intellij.lang.annotation.AnnotationHolder
import com.intellij.lang.annotation.Annotator
import com.intellij.lang.annotation.HighlightSeverity
import com.intellij.openapi.util.TextRange
import com.intellij.openapi.components.service
import com.intellij.openapi.editor.colors.EditorColorsManager
import java.awt.Font
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile

class HixAnnotator : Annotator {
    override fun annotate(element: PsiElement, holder: AnnotationHolder) {
        if (element !is PsiFile || element.language != HixLanguage) return
        HixAntlrSyntax.parse(element.text).diagnostics.distinct().forEach { diagnostic ->
            holder.newAnnotation(HighlightSeverity.ERROR, diagnostic.message)
                .range(TextRange(diagnostic.start, diagnostic.end)).create()
        }
        val service = element.project.service<HixSnapshotService>()
        val path = element.virtualFile?.path
        val snapshot = element.getUserData(HixSnapshotService.SEMANTIC_SNAPSHOT)
            ?: service.snapshotForText(element.text, path)
        snapshot?.diagnostics?.distinct()?.forEach { diagnostic ->
            val severity = when (diagnostic.severity.lowercase()) {
                "warning" -> HighlightSeverity.WARNING
                "information", "info" -> HighlightSeverity.INFORMATION
                else -> HighlightSeverity.ERROR
            }
            val start = diagnostic.range.startOffset.coerceIn(0, element.textLength)
            val end = diagnostic.range.endOffset.coerceIn(start, element.textLength)
            holder.newAnnotation(severity, diagnostic.message).range(TextRange(start, end)).create()
        }
        snapshot?.typeFacts?.filter { it.kind == "Call" && snapshot.typeFacts.none { dynamic ->
            dynamic.kind == "DynamicCall" && dynamic.range == it.range
        } }?.forEach { fact ->
            val start = fact.range.startOffset.coerceIn(0, element.textLength)
            val end = fact.range.endOffset.coerceIn(start, element.textLength)
            holder.newSilentAnnotation(HighlightSeverity.INFORMATION).range(TextRange(start, end))
                .textAttributes(HixColors.FUNCTION).create()
        }
        snapshot?.typeFacts?.filter { it.kind == "DynamicCall" }?.forEach { fact ->
            val start = fact.range.startOffset.coerceIn(0, element.textLength)
            val end = fact.range.endOffset.coerceIn(start, element.textLength)
            val attributes = EditorColorsManager.getInstance().globalScheme
                .getAttributes(HixColors.DIRECTIVE).clone().apply { fontType = Font.ITALIC }
            holder.newSilentAnnotation(HighlightSeverity.INFORMATION).range(TextRange(start, end))
                .enforcedTextAttributes(attributes).create()
        }
    }
}

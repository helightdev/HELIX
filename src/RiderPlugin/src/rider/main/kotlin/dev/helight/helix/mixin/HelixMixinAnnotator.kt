package dev.helight.helix.mixin

import com.intellij.lang.annotation.AnnotationHolder
import com.intellij.lang.annotation.Annotator
import com.intellij.lang.annotation.HighlightSeverity
import com.intellij.openapi.util.TextRange
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile

class HelixMixinAnnotator : Annotator {
    override fun annotate(element: PsiElement, holder: AnnotationHolder) {
        if (element !is PsiFile || element.language != HelixMixinLanguage) return
        val path = element.virtualFile.getUserData(HelixMixinSnapshotService.ORIGINAL_PATH)
            ?: element.virtualFile.path
        val snapshot = HelixMixinSnapshotService.getInstance(element.project)
            .snapshotForText(element.text, path) ?: return
        snapshot.diagnostics.forEach { diagnostic ->
            val start = diagnostic.range.startOffset.coerceIn(0, element.textLength)
            val end = diagnostic.range.endOffset.coerceIn(start, element.textLength)
            val severity = if (diagnostic.severity == "Warning") HighlightSeverity.WARNING else HighlightSeverity.ERROR
            holder.newAnnotation(severity, diagnostic.message).range(TextRange(start, end)).create()
        }
    }
}

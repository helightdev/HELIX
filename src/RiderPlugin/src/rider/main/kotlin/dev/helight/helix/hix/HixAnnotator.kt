package dev.helight.helix.hix

import com.intellij.lang.annotation.AnnotationHolder
import com.intellij.lang.annotation.Annotator
import com.intellij.lang.annotation.HighlightSeverity
import com.intellij.openapi.util.TextRange
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile

class HixAnnotator : Annotator {
    override fun annotate(element: PsiElement, holder: AnnotationHolder) {
        if (element !is PsiFile || element.language != HixLanguage) return
        HixAntlrSyntax.parse(element.text).diagnostics.distinct().forEach { diagnostic ->
            holder.newAnnotation(HighlightSeverity.ERROR, diagnostic.message)
                .range(TextRange(diagnostic.start, diagnostic.end)).create()
        }
    }
}

package dev.helight.helix.expression

import com.intellij.lang.annotation.AnnotationHolder
import com.intellij.lang.annotation.Annotator
import com.intellij.lang.annotation.HighlightSeverity
import com.intellij.psi.PsiElement

class HelixExpressionAnnotator : Annotator {
    override fun annotate(element: PsiElement, holder: AnnotationHolder) {
        val type = element.node.elementType
        val key = when (type) {
            HelixExpressionTypes.DIRECTIVE -> HelixExpressionSyntaxHighlighter.DIRECTIVE
            HelixExpressionTypes.VALUE -> HelixExpressionSyntaxHighlighter.VALUE
            HelixExpressionTypes.PATH -> HelixExpressionSyntaxHighlighter.PATH
            HelixExpressionTypes.FUNCTION -> HelixExpressionSyntaxHighlighter.FUNCTION
            HelixExpressionTypes.ARGUMENT -> HelixExpressionSyntaxHighlighter.ARGUMENT
            HelixExpressionTypes.PREDICATE,
            HelixExpressionTypes.NEGATED_PREDICATE,
            HelixExpressionTypes.INVOKE -> HelixExpressionSyntaxHighlighter.OPERATOR
            HelixExpressionTypes.ENCLOSED_START,
            HelixExpressionTypes.ENCLOSED_END -> HelixExpressionSyntaxHighlighter.BRACES
            HelixExpressionTypes.ESCAPED_AT -> HelixExpressionSyntaxHighlighter.ESCAPE
            HelixExpressionTypes.BAD_CHARACTER -> HelixExpressionSyntaxHighlighter.BAD_CHARACTER
            else -> null
        } ?: return

        holder.newSilentAnnotation(HighlightSeverity.INFORMATION)
            .range(element.textRange)
            .textAttributes(key)
            .create()
    }
}

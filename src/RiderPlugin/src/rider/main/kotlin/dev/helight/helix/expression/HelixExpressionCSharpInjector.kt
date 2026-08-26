package dev.helight.helix.expression

import com.intellij.lang.injection.MultiHostInjector
import com.intellij.lang.injection.MultiHostRegistrar
import com.intellij.openapi.util.TextRange
import com.intellij.psi.PsiElement
import com.intellij.psi.util.PsiTreeUtil
import com.jetbrains.rider.languages.fileTypes.csharp.psi.CSharpStringLiteralExpression
import com.jetbrains.rider.languages.fileTypes.csharp.psi.impl.CSharpElementTypes
import com.jetbrains.rider.languages.fileTypes.csharp.psi.impl.CSharpNonInterpolatedStringLiteralExpressionImpl
import dev.helight.helix.workspace.WorkspaceSettings

/** Injects HELIX expressions by inspecting Rider's local C# syntax tree. */
class HelixExpressionCSharpInjector : MultiHostInjector {
    override fun elementsToInjectIn(): List<Class<out PsiElement>> =
        listOf(CSharpNonInterpolatedStringLiteralExpressionImpl::class.java)

    override fun getLanguagesToInject(registrar: MultiHostRegistrar, context: PsiElement) {
        if (!WorkspaceSettings.getInstance(context.project).helixEnabled) return
        val literal = context as? CSharpStringLiteralExpression ?: return
        if (!literal.isValidHost) return

        val hostRange = literal.textRange ?: return
        val attribute = literal.parentsWithSelf()
            .firstOrNull { it.node?.elementType == CSharpElementTypes.ATTRIBUTE_DECLARATION }
            ?: return
        val attributeKind = helixAttributeKind(attribute.text.substringBefore('(')) ?: return
        val literals = PsiTreeUtil.findChildrenOfType(attribute, CSharpStringLiteralExpression::class.java)
            .sortedBy { it.textRange.startOffset }
        val expressionLiteral = when (attributeKind) {
            HelixAttributeKind.LIBRARY -> literals.firstOrNull()
            HelixAttributeKind.EXPRESSION -> literals.firstOrNull(::isNamedExpressionArgument)
                ?: literals.lastOrNull()
        }
        if (literal.textRange != expressionLiteral?.textRange) return

        // Rider's C# PSI returns a file-absolute content range, while addPlace requires offsets
        // relative to the injection host. Passing the absolute range is silently ignored after
        // the host has already been semantically matched.
        val range = toHostRelativeRange(literal.getContentRange(), hostRange.startOffset)
        if (range.isEmpty || range.startOffset < 0 || range.endOffset > literal.textLength) return
        registrar.startInjecting(HelixExpressionLanguage)
            .addPlace(null, null, literal, range)
            .doneInjecting()
    }

    private fun PsiElement.parentsWithSelf(): Sequence<PsiElement> =
        generateSequence(this) { it.parent }

    private fun isNamedExpressionArgument(literal: CSharpStringLiteralExpression): Boolean {
        val attribute = literal.parentsWithSelf()
            .firstOrNull { it.node?.elementType == CSharpElementTypes.ATTRIBUTE_DECLARATION }
            ?: return false
        val relativeStart = literal.textRange.startOffset - attribute.textRange.startOffset
        return EXPRESSION_ARGUMENT_SUFFIX.containsMatchIn(attribute.text.substring(0, relativeStart))
    }

    private companion object {
        val EXPRESSION_ARGUMENT_SUFFIX = Regex("expression\\s*:\\s*$")
    }
}

internal fun toHostRelativeRange(absoluteContentRange: TextRange, hostStartOffset: Int): TextRange =
    absoluteContentRange.shiftLeft(hostStartOffset)

internal enum class HelixAttributeKind { EXPRESSION, LIBRARY }

internal fun helixAttributeKind(attributeHeader: String): HelixAttributeKind? {
    val compact = attributeHeader.filterNot(Char::isWhitespace)
        .trimStart('[')
        .substringAfterLast(':')
        .removePrefix("global::")
    val simpleName = when {
        compact.startsWith("HELIX.") -> compact.removePrefix("HELIX.")
        '.' !in compact -> compact
        else -> return null
    }.removeSuffix("Attribute")
    return when (simpleName) {
        "MixinExpression" -> HelixAttributeKind.EXPRESSION
        "MixinLibrary" -> HelixAttributeKind.LIBRARY
        else -> null
    }
}

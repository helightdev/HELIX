package dev.helight.helix.expression

import com.intellij.lang.injection.MultiHostInjector
import com.intellij.lang.injection.MultiHostRegistrar
import com.intellij.psi.PsiElement
import com.jetbrains.rider.languages.fileTypes.csharp.psi.CSharpStringLiteralExpression

/** Injects HELIX expressions into the expression-bearing arguments of mixin attributes. */
class HelixExpressionCSharpInjector : MultiHostInjector {
    override fun elementsToInjectIn(): List<Class<out PsiElement>> =
        listOf(CSharpStringLiteralExpression::class.java)

    override fun getLanguagesToInject(registrar: MultiHostRegistrar, context: PsiElement) {
        val literal = context as? CSharpStringLiteralExpression ?: return
        if (!literal.isValidHost || !isMixinExpressionArgument(literal)) return
        val hostRange = literal.textRange ?: return
        val range = literal.getContentRange().shiftLeft(hostRange.startOffset)
        if (range.isEmpty || range.startOffset < 0 || range.endOffset > literal.textLength) return
        registrar.startInjecting(HelixExpressionLanguage)
            .addPlace(null, null, literal, range)
            .doneInjecting()
    }

    private fun isMixinExpressionArgument(literal: CSharpStringLiteralExpression): Boolean {
        val fileText = literal.containingFile.text
        val literalStart = literal.textRange.startOffset
        return isMixinExpressionArgument(fileText, literalStart, literal.textRange.endOffset)
    }

    companion object {
        internal fun isMixinExpressionArgument(fileText: String, literalStart: Int, literalEnd: Int): Boolean {
            val searchStart = (literalStart - MAX_ATTRIBUTE_LOOKBACK).coerceAtLeast(0)
            val prefix = fileText.substring(searchStart, literalStart)
            val attributeStart = prefix.lastIndexOf('[')
            if (attributeStart < 0 || prefix.lastIndexOf(']') > attributeStart) return false
            val attributeText = prefix.substring(attributeStart)
            val match = ATTRIBUTE.findAll(attributeText).lastOrNull() ?: return false
            val attributeName = match.groupValues[1]
            val openParen = searchStart + attributeStart + match.range.last
            if (attributeName == "MixinPrepareGlobal") return true

            val argumentIndex = countTopLevelCommas(fileText, openParen + 1, literalStart)
            if (argumentIndex >= 2) return true
            if (argumentIndex != 0) return false
            return !hasTopLevelCommaBeforeClose(fileText, literalEnd)
        }

        private fun countTopLevelCommas(text: String, from: Int, to: Int): Int {
            var depth = 0
            var commas = 0
            var i = from
            while (i < to) {
                when (text[i]) {
                    '(', '[', '{' -> depth++
                    ')', ']', '}' -> if (depth > 0) depth--
                    ',' -> if (depth == 0) commas++
                    '"' -> i = skipString(text, i, to)
                }
                i++
            }
            return commas
        }

        private fun hasTopLevelCommaBeforeClose(text: String, from: Int): Boolean {
            var depth = 0
            var i = from
            val end = (from + MAX_ATTRIBUTE_LOOKAHEAD).coerceAtMost(text.length)
            while (i < end) {
                when (text[i]) {
                    '(', '[', '{' -> depth++
                    ')' -> if (depth == 0) return false else depth--
                    ']', '}' -> if (depth > 0) depth--
                    ',' -> if (depth == 0) return true
                    '"' -> i = skipString(text, i, end)
                }
                i++
            }
            return false
        }

        private fun skipString(text: String, quote: Int, limit: Int): Int {
            var i = quote + 1
            while (i < limit) {
                if (text[i] == '"') return i
                if (text[i] == '\\') i++
                i++
            }
            return i
        }

        private const val MAX_ATTRIBUTE_LOOKBACK = 4096
        private const val MAX_ATTRIBUTE_LOOKAHEAD = 4096
        private val ATTRIBUTE = Regex("(?:HELIX\\s*\\.\\s*)?(MixinExpression|MixinPrepareGlobal)(?:Attribute)?\\s*\\(")
    }
}

//TODO: This is awful, this should be properly based on the PSI tree or interop with Roslyn, fix this later,
//      this also doesn't even properly match expression strings in later arguments
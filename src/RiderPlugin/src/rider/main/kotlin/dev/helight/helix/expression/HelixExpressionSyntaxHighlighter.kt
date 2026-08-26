package dev.helight.helix.expression

import com.intellij.lexer.Lexer
import com.intellij.openapi.editor.DefaultLanguageHighlighterColors
import com.intellij.openapi.editor.HighlighterColors
import com.intellij.openapi.editor.colors.TextAttributesKey
import com.intellij.openapi.fileTypes.SyntaxHighlighter
import com.intellij.openapi.fileTypes.SyntaxHighlighterBase
import com.intellij.openapi.fileTypes.SyntaxHighlighterFactory
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.psi.tree.IElementType

class HelixExpressionSyntaxHighlighter : SyntaxHighlighterBase() {
    override fun getHighlightingLexer(): Lexer = HelixExpressionLexer()

    override fun getTokenHighlights(tokenType: IElementType): Array<TextAttributesKey> = pack(when (tokenType) {
        HelixExpressionTypes.DIRECTIVE -> DIRECTIVE
        HelixExpressionTypes.VALUE -> VALUE
        HelixExpressionTypes.PATH -> PATH
        HelixExpressionTypes.FUNCTION -> FUNCTION
        HelixExpressionTypes.ARGUMENT -> ARGUMENT
        HelixExpressionTypes.PREDICATE,
        HelixExpressionTypes.NEGATED_PREDICATE,
        HelixExpressionTypes.INVOKE -> OPERATOR
        HelixExpressionTypes.ENCLOSED_START,
        HelixExpressionTypes.ENCLOSED_END -> BRACES
        HelixExpressionTypes.ESCAPED_AT -> ESCAPE
        HelixExpressionTypes.NEWLINE_CONTINUATION,
        HelixExpressionTypes.DIRECT_CONTINUATION -> OPERATOR
        HelixExpressionTypes.COMMENT -> COMMENT
        HelixExpressionTypes.BAD_CHARACTER -> BAD_CHARACTER
        else -> null
    })

    companion object {
        @JvmField val DIRECTIVE = TextAttributesKey.createTextAttributesKey(
            "HELIX_EXPRESSION_DIRECTIVE", DefaultLanguageHighlighterColors.KEYWORD)
        @JvmField val VALUE = TextAttributesKey.createTextAttributesKey(
            "HELIX_EXPRESSION_VALUE", DefaultLanguageHighlighterColors.KEYWORD)
        @JvmField val PATH = TextAttributesKey.createTextAttributesKey(
            "HELIX_EXPRESSION_PATH", DefaultLanguageHighlighterColors.INSTANCE_FIELD)
        @JvmField val FUNCTION = TextAttributesKey.createTextAttributesKey(
            "HELIX_EXPRESSION_FUNCTION", DefaultLanguageHighlighterColors.FUNCTION_CALL)
        @JvmField val ARGUMENT = TextAttributesKey.createTextAttributesKey(
            "HELIX_EXPRESSION_ARGUMENT", DefaultLanguageHighlighterColors.MARKUP_ATTRIBUTE)
        @JvmField val OPERATOR = TextAttributesKey.createTextAttributesKey(
            "HELIX_EXPRESSION_OPERATOR", DefaultLanguageHighlighterColors.FUNCTION_CALL)
        @JvmField val BRACES = TextAttributesKey.createTextAttributesKey(
            "HELIX_EXPRESSION_BRACES", DefaultLanguageHighlighterColors.PARENTHESES)
        @JvmField val ESCAPE = TextAttributesKey.createTextAttributesKey(
            "HELIX_EXPRESSION_ESCAPE", DefaultLanguageHighlighterColors.VALID_STRING_ESCAPE)
        @JvmField val COMMENT = TextAttributesKey.createTextAttributesKey(
            "HELIX_EXPRESSION_COMMENT", DefaultLanguageHighlighterColors.LINE_COMMENT)
        @JvmField val BAD_CHARACTER = TextAttributesKey.createTextAttributesKey(
            "HELIX_EXPRESSION_BAD_CHARACTER", HighlighterColors.BAD_CHARACTER)
    }
}

class HelixExpressionSyntaxHighlighterFactory : SyntaxHighlighterFactory() {
    override fun getSyntaxHighlighter(project: Project?, virtualFile: VirtualFile?): SyntaxHighlighter =
        HelixExpressionSyntaxHighlighter()
}

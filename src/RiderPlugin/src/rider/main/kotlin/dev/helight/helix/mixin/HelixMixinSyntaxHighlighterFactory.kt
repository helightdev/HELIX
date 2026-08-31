package dev.helight.helix.mixin

import com.intellij.openapi.fileTypes.SyntaxHighlighter
import com.intellij.openapi.fileTypes.SyntaxHighlighterFactory
import com.intellij.openapi.fileTypes.SyntaxHighlighterBase
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.lexer.Lexer
import com.intellij.openapi.editor.DefaultLanguageHighlighterColors
import com.intellij.openapi.editor.HighlighterColors
import com.intellij.openapi.editor.colors.TextAttributesKey
import com.intellij.psi.tree.IElementType

object HelixMixinColors {
    val DIRECTIVE = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_DIRECTIVE", DefaultLanguageHighlighterColors.KEYWORD)
    val VALUE = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_VALUE", DefaultLanguageHighlighterColors.INSTANCE_FIELD)
    val PATH = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_PATH", DefaultLanguageHighlighterColors.INSTANCE_METHOD)
    val FUNCTION = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_FUNCTION", DefaultLanguageHighlighterColors.FUNCTION_CALL)
    val ARGUMENT = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_ARGUMENT", DefaultLanguageHighlighterColors.STRING)
    val COMMENT = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_COMMENT", DefaultLanguageHighlighterColors.LINE_COMMENT)
    val ESCAPE = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_ESCAPE", DefaultLanguageHighlighterColors.VALID_STRING_ESCAPE)
    val BAD = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_BAD_CHARACTER", HighlighterColors.BAD_CHARACTER)
}

class HelixMixinSyntaxHighlighterFactory : SyntaxHighlighterFactory() {
    override fun getSyntaxHighlighter(project: Project?, virtualFile: VirtualFile?): SyntaxHighlighter =
        HelixMixinSyntaxHighlighter(project)
}

private class HelixMixinSyntaxHighlighter(private val project: Project?) : SyntaxHighlighterBase() {
    override fun getHighlightingLexer(): Lexer = HelixMixinLexer(project)

    override fun getTokenHighlights(tokenType: IElementType): Array<TextAttributesKey> = pack(when (tokenType) {
        HelixMixinTokenTypes.DIRECTIVE,
        HelixMixinTokenTypes.OPEN_ANGLE,
        HelixMixinTokenTypes.CLOSE_ANGLE,
        HelixMixinTokenTypes.CONTINUATION -> HelixMixinColors.DIRECTIVE
        HelixMixinTokenTypes.VALUE -> HelixMixinColors.VALUE
        HelixMixinTokenTypes.PATH -> HelixMixinColors.PATH
        HelixMixinTokenTypes.FUNCTION,
        HelixMixinTokenTypes.OPERATOR,
        HelixMixinTokenTypes.OPEN_PARENTHESIS,
        HelixMixinTokenTypes.CLOSE_PARENTHESIS -> HelixMixinColors.FUNCTION
        HelixMixinTokenTypes.ARGUMENT -> HelixMixinColors.ARGUMENT
        HelixMixinTokenTypes.COMMENT -> HelixMixinColors.COMMENT
        HelixMixinTokenTypes.ESCAPE -> HelixMixinColors.ESCAPE
        HelixMixinTokenTypes.INVALID -> HelixMixinColors.BAD
        else -> null
    })
}

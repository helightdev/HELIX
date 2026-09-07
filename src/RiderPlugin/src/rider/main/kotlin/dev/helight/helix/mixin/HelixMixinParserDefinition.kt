package dev.helight.helix.mixin

import com.intellij.lang.ASTNode
import com.intellij.lang.ParserDefinition
import com.intellij.lang.PsiParser
import com.intellij.lexer.Lexer
import com.intellij.openapi.project.Project
import com.intellij.psi.FileViewProvider
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile
import com.intellij.psi.tree.IFileElementType
import com.intellij.psi.tree.TokenSet
import dev.helight.helix.mixin.generated.MixinLexer

class HelixMixinParserDefinition : ParserDefinition {
    override fun createLexer(project: Project?): Lexer = HelixMixinLexer(project)
    override fun createParser(project: Project): PsiParser = HelixMixinParser(project)
    override fun getFileNodeType(): IFileElementType = HelixMixinElementTypes.FILE
    override fun getWhitespaceTokens(): TokenSet = TokenSet.create(HelixAntlrTypes.tokens[0])
    override fun getCommentTokens(): TokenSet = TokenSet.create(
        HelixAntlrTypes.tokens[MixinLexer.COMMENT], HelixAntlrTypes.tokens[MixinLexer.SLASH_COMMENT])
    override fun getStringLiteralElements(): TokenSet = TokenSet.create(HelixAntlrTypes.tokens[MixinLexer.ARGUMENT_TEXT])
    override fun createFile(viewProvider: FileViewProvider): PsiFile = HelixMixinFile(viewProvider)

    override fun createElement(node: ASTNode): PsiElement = when (node.elementType) {
        HelixMixinElementTypes.DECLARATION -> HelixMixinDeclarationElement(node)
        HelixMixinElementTypes.REFERENCE -> HelixMixinReferenceElement(node)
        else -> HelixMixinPsiElement(node)
    }
}

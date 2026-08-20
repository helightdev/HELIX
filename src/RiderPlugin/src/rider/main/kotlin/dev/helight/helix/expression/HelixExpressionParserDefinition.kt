package dev.helight.helix.expression

import com.intellij.extapi.psi.ASTWrapperPsiElement
import com.intellij.lang.ASTNode
import com.intellij.lang.ParserDefinition
import com.intellij.lexer.Lexer
import com.intellij.openapi.project.Project
import com.intellij.psi.FileViewProvider
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile
import com.intellij.psi.tree.IFileElementType
import com.intellij.psi.tree.TokenSet

class HelixExpressionParserDefinition : ParserDefinition {
    override fun createLexer(project: Project?): Lexer = HelixExpressionLexer()
    override fun createParser(project: Project?) = HelixExpressionParser()
    override fun getFileNodeType() = FILE
    override fun getWhitespaceTokens(): TokenSet = HelixExpressionTypes.WHITE_SPACES
    override fun getCommentTokens(): TokenSet = TokenSet.EMPTY
    override fun getStringLiteralElements(): TokenSet = TokenSet.create(HelixExpressionTypes.TEXT)
    override fun createElement(node: ASTNode): PsiElement = ASTWrapperPsiElement(node)
    override fun createFile(viewProvider: FileViewProvider): PsiFile = HelixExpressionFile(viewProvider)

    private companion object {
        val FILE = IFileElementType(HelixExpressionLanguage)
    }
}

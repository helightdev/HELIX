package dev.helight.helix.hix

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
import dev.helight.helix.hix.generated.HixLexer

class HixParserDefinition : ParserDefinition {
    override fun createLexer(project: Project?): Lexer = HixEditorLexer(project)
    override fun createParser(project: Project): PsiParser = HixPsiParser(project)
    override fun getFileNodeType(): IFileElementType = HixElementTypes.FILE
    override fun getWhitespaceTokens(): TokenSet = TokenSet.create(HelixAntlrTypes.tokens[0])
    override fun getCommentTokens(): TokenSet = TokenSet.create(
        HelixAntlrTypes.tokens[HixLexer.COMMENT], HelixAntlrTypes.tokens[HixLexer.SLASH_COMMENT])
    override fun getStringLiteralElements(): TokenSet = TokenSet.create(HelixAntlrTypes.tokens[HixLexer.ARGUMENT_TEXT])
    override fun createFile(viewProvider: FileViewProvider): PsiFile = HixFile(viewProvider)

    override fun createElement(node: ASTNode): PsiElement = when (node.elementType) {
        HixElementTypes.DECLARATION -> HixDeclarationElement(node)
        HixElementTypes.REFERENCE -> HixReferenceElement(node)
        else -> HixPsiElement(node)
    }
}

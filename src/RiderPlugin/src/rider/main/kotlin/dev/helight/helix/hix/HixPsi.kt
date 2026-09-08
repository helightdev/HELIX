package dev.helight.helix.hix

import com.intellij.extapi.psi.ASTWrapperPsiElement
import com.intellij.extapi.psi.PsiFileBase
import com.intellij.lang.ASTNode
import com.intellij.openapi.util.TextRange
import com.intellij.openapi.vfs.LocalFileSystem
import com.intellij.psi.FileViewProvider
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiManager
import com.intellij.psi.PsiNameIdentifierOwner
import com.intellij.psi.PsiReference
import com.intellij.psi.util.PsiTreeUtil
import com.intellij.psi.tree.IElementType
import com.intellij.psi.tree.IFileElementType
import com.intellij.util.IncorrectOperationException

class HixTokenType(debugName: String) : IElementType(debugName, HixLanguage)
class HixElementType(debugName: String) : IElementType(debugName, HixLanguage)

object HixTokenTypes {
    val PLAIN = HixTokenType("MIXIN_PLAIN")
    val WHITE_SPACE = HixTokenType("MIXIN_WHITE_SPACE")
    val TEXT_WHITE_SPACE = HixTokenType("MIXIN_TEXT_WHITE_SPACE")
    val NEW_LINE = HixTokenType("MIXIN_NEW_LINE")
    val COMMENT = HixTokenType("MIXIN_COMMENT")
    val DIRECTIVE = HixTokenType("MIXIN_DIRECTIVE")
    val OPEN_ANGLE = HixTokenType("MIXIN_OPEN_ANGLE")
    val CLOSE_ANGLE = HixTokenType("MIXIN_CLOSE_ANGLE")
    val DIRECTIVE_OPEN_ANGLE = HixTokenType("MIXIN_DIRECTIVE_OPEN_ANGLE")
    val DIRECTIVE_CLOSE_ANGLE = HixTokenType("MIXIN_DIRECTIVE_CLOSE_ANGLE")
    val FUNCTION_OPEN_ANGLE = HixTokenType("MIXIN_FUNCTION_OPEN_ANGLE")
    val FUNCTION_CLOSE_ANGLE = HixTokenType("MIXIN_FUNCTION_CLOSE_ANGLE")
    val OPEN_PARENTHESIS = HixTokenType("MIXIN_OPEN_PARENTHESIS")
    val CLOSE_PARENTHESIS = HixTokenType("MIXIN_CLOSE_PARENTHESIS")
    val DIRECTIVE_OPEN_PARENTHESIS = HixTokenType("MIXIN_DIRECTIVE_OPEN_PARENTHESIS")
    val DIRECTIVE_CLOSE_PARENTHESIS = HixTokenType("MIXIN_DIRECTIVE_CLOSE_PARENTHESIS")
    val FUNCTION_OPEN_PARENTHESIS = HixTokenType("MIXIN_FUNCTION_OPEN_PARENTHESIS")
    val FUNCTION_CLOSE_PARENTHESIS = HixTokenType("MIXIN_FUNCTION_CLOSE_PARENTHESIS")
    val VALUE = HixTokenType("MIXIN_VALUE")
    val PATH = HixTokenType("MIXIN_PATH")
    val FUNCTION = HixTokenType("MIXIN_FUNCTION")
    val OPERATOR = HixTokenType("MIXIN_OPERATOR")
    val CONTINUATION = HixTokenType("MIXIN_CONTINUATION")
    val ESCAPE = HixTokenType("MIXIN_ESCAPE")
    val ARGUMENT = HixTokenType("MIXIN_ARGUMENT")
    val TEXT = HixTokenType("MIXIN_TEXT")
    val INVALID = HixTokenType("MIXIN_INVALID")

    fun fromBackend(kind: String, text: CharSequence): IElementType = when (kind) {
        "Whitespace" -> WHITE_SPACE
        "TextWhitespace" -> TEXT_WHITE_SPACE
        "NewLine" -> NEW_LINE
        "Comment" -> COMMENT
        "Directive" -> DIRECTIVE
        "DirectiveArgumentDelimiter" -> if (text.firstOrNull() == '<') DIRECTIVE_OPEN_ANGLE else DIRECTIVE_CLOSE_ANGLE
        "ExpressionArgumentDelimiter" -> when (text.firstOrNull()) {
            '<' -> FUNCTION_OPEN_ANGLE
            '>' -> FUNCTION_CLOSE_ANGLE
            '(' -> OPEN_PARENTHESIS
            else -> CLOSE_PARENTHESIS
        }
        "EnclosedReferenceParenthesis", "Parenthesis" ->
            if (text.firstOrNull() == '(') OPEN_PARENTHESIS else CLOSE_PARENTHESIS
        "Value" -> VALUE
        "Path" -> PATH
        "Function" -> FUNCTION
        "Operator" -> OPERATOR
        "Continuation" -> CONTINUATION
        "Escape" -> ESCAPE
        "Argument" -> ARGUMENT
        "Text" -> TEXT
        "Invalid" -> INVALID
        else -> PLAIN
    }
}

object HixElementTypes {
    val FILE = IFileElementType("HIX_FILE", HixLanguage)
    val DOCUMENT = HixElementType("MIXIN_DOCUMENT")
    val DIRECTIVE = HixElementType("MIXIN_DIRECTIVE_NODE")
    val DIRECTIVE_NAME = HixElementType("MIXIN_DIRECTIVE_NAME")
    val DIRECTIVE_ARGUMENT = HixElementType("MIXIN_DIRECTIVE_ARGUMENT_NODE")
    val OPERAND = HixElementType("MIXIN_OPERAND")
    val REFERENCE_EXPRESSION = HixElementType("MIXIN_REFERENCE_EXPRESSION")
    val ROOT = HixElementType("MIXIN_ROOT")
    val MEMBER = HixElementType("MIXIN_MEMBER")
    val PATH = HixElementType("MIXIN_PATH_NODE")
    val FUNCTION_CALL = HixElementType("MIXIN_FUNCTION_CALL")
    val FUNCTION_ARGUMENT = HixElementType("MIXIN_FUNCTION_ARGUMENT")
    val COMMENT = HixElementType("MIXIN_COMMENT_NODE")
    val CONTINUATION = HixElementType("MIXIN_CONTINUATION_NODE")
    val ESCAPE = HixElementType("MIXIN_ESCAPE_NODE")
    val ERROR = HixElementType("MIXIN_ERROR_NODE")
    val DECLARATION = HixElementType("MIXIN_DECLARATION")
    val REFERENCE = HixElementType("MIXIN_REFERENCE")

    fun syntax(kind: String): IElementType = when (kind) {
        "Document" -> DOCUMENT
        "Directive" -> DIRECTIVE
        "DirectiveName" -> DIRECTIVE_NAME
        "DirectiveArgument", "DeclarationDirectiveArgument", "ReferenceDirectiveArgument",
        "DeclarationReferenceDirectiveArgument" -> DIRECTIVE_ARGUMENT
        "Operand" -> OPERAND
        "Reference", "ParenthesizedReference" -> REFERENCE_EXPRESSION
        "Root" -> ROOT
        "Member" -> MEMBER
        "Path" -> PATH
        "FunctionCall" -> FUNCTION_CALL
        "LiteralArgument", "ExpressionArgument" -> FUNCTION_ARGUMENT
        "Comment" -> COMMENT
        "Continuation" -> CONTINUATION
        "Escape" -> ESCAPE
        "Error" -> ERROR
        else -> ERROR
    }
}

class HixFile(viewProvider: FileViewProvider) : PsiFileBase(viewProvider, HixLanguage) {
    override fun getFileType() = HixFileType
    override fun toString() = "HELIX mixin file"
}

open class HixPsiElement(node: ASTNode) : ASTWrapperPsiElement(node)

class HixDeclarationElement(node: ASTNode) : HixPsiElement(node), PsiNameIdentifierOwner {
    override fun getNameIdentifier(): PsiElement = this
    override fun getName(): String = text
    override fun setName(name: String): PsiElement {
        val document = containingFile.viewProvider.document ?: return this
        document.replaceString(textRange.startOffset, textRange.endOffset, name)
        return this
    }
}

class HixReferenceElement(node: ASTNode) : HixPsiElement(node), PsiReference {
    override fun getReference(): PsiReference = this
    override fun getElement(): PsiElement = this
    override fun getRangeInElement(): TextRange = TextRange(0, textLength)
    override fun getCanonicalText(): String = text

    override fun resolve(): PsiElement? {
        val mixinType = HelixAntlrTypes.rules[dev.helight.helix.hix.generated.HixParser.RULE_mixinDeclaration]
        val functionType = HelixAntlrTypes.rules[dev.helight.helix.hix.generated.HixParser.RULE_funcDeclaration]
        val scope = generateSequence(parent) { it.parent }.firstOrNull { it.node.elementType == mixinType }
        val declarations = PsiTreeUtil.findChildrenOfType(containingFile, HixDeclarationElement::class.java)
            .filter { it.text == text && it.parent.node.elementType == functionType }
        val local = declarations.filter { declaration ->
            generateSequence(declaration.parent) { it.parent }.firstOrNull { it.node.elementType == mixinType } === scope
        }
        if (local.size == 1) return local.single()
        if (local.isNotEmpty()) return null
        return declarations.singleOrNull { declaration ->
            generateSequence(declaration.parent) { it.parent }.none { it.node.elementType == mixinType }
        }
    }

    override fun handleElementRename(newElementName: String): PsiElement {
        val document = containingFile.viewProvider.document ?: return this
        document.replaceString(textRange.startOffset, textRange.endOffset, newElementName)
        return this
    }
    override fun bindToElement(element: PsiElement): PsiElement =
        throw IncorrectOperationException("Mixin references cannot be rebound")
    override fun isReferenceTo(element: PsiElement): Boolean =
        resolve()?.let { manager.areElementsEquivalent(it, element) } ?: false
    override fun getVariants(): Array<Any> = emptyArray()
    override fun isSoft(): Boolean = false
}

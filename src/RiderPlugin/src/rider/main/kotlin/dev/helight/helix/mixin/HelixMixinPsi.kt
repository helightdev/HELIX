package dev.helight.helix.mixin

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

class HelixMixinTokenType(debugName: String) : IElementType(debugName, HelixMixinLanguage)
class HelixMixinElementType(debugName: String) : IElementType(debugName, HelixMixinLanguage)

object HelixMixinTokenTypes {
    val PLAIN = HelixMixinTokenType("MIXIN_PLAIN")
    val WHITE_SPACE = HelixMixinTokenType("MIXIN_WHITE_SPACE")
    val TEXT_WHITE_SPACE = HelixMixinTokenType("MIXIN_TEXT_WHITE_SPACE")
    val NEW_LINE = HelixMixinTokenType("MIXIN_NEW_LINE")
    val COMMENT = HelixMixinTokenType("MIXIN_COMMENT")
    val DIRECTIVE = HelixMixinTokenType("MIXIN_DIRECTIVE")
    val OPEN_ANGLE = HelixMixinTokenType("MIXIN_OPEN_ANGLE")
    val CLOSE_ANGLE = HelixMixinTokenType("MIXIN_CLOSE_ANGLE")
    val DIRECTIVE_OPEN_ANGLE = HelixMixinTokenType("MIXIN_DIRECTIVE_OPEN_ANGLE")
    val DIRECTIVE_CLOSE_ANGLE = HelixMixinTokenType("MIXIN_DIRECTIVE_CLOSE_ANGLE")
    val FUNCTION_OPEN_ANGLE = HelixMixinTokenType("MIXIN_FUNCTION_OPEN_ANGLE")
    val FUNCTION_CLOSE_ANGLE = HelixMixinTokenType("MIXIN_FUNCTION_CLOSE_ANGLE")
    val OPEN_PARENTHESIS = HelixMixinTokenType("MIXIN_OPEN_PARENTHESIS")
    val CLOSE_PARENTHESIS = HelixMixinTokenType("MIXIN_CLOSE_PARENTHESIS")
    val DIRECTIVE_OPEN_PARENTHESIS = HelixMixinTokenType("MIXIN_DIRECTIVE_OPEN_PARENTHESIS")
    val DIRECTIVE_CLOSE_PARENTHESIS = HelixMixinTokenType("MIXIN_DIRECTIVE_CLOSE_PARENTHESIS")
    val FUNCTION_OPEN_PARENTHESIS = HelixMixinTokenType("MIXIN_FUNCTION_OPEN_PARENTHESIS")
    val FUNCTION_CLOSE_PARENTHESIS = HelixMixinTokenType("MIXIN_FUNCTION_CLOSE_PARENTHESIS")
    val VALUE = HelixMixinTokenType("MIXIN_VALUE")
    val PATH = HelixMixinTokenType("MIXIN_PATH")
    val FUNCTION = HelixMixinTokenType("MIXIN_FUNCTION")
    val OPERATOR = HelixMixinTokenType("MIXIN_OPERATOR")
    val CONTINUATION = HelixMixinTokenType("MIXIN_CONTINUATION")
    val ESCAPE = HelixMixinTokenType("MIXIN_ESCAPE")
    val ARGUMENT = HelixMixinTokenType("MIXIN_ARGUMENT")
    val TEXT = HelixMixinTokenType("MIXIN_TEXT")
    val INVALID = HelixMixinTokenType("MIXIN_INVALID")

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

object HelixMixinElementTypes {
    val FILE = IFileElementType("MIXIN_FILE", HelixMixinLanguage)
    val DOCUMENT = HelixMixinElementType("MIXIN_DOCUMENT")
    val DIRECTIVE = HelixMixinElementType("MIXIN_DIRECTIVE_NODE")
    val DIRECTIVE_NAME = HelixMixinElementType("MIXIN_DIRECTIVE_NAME")
    val DIRECTIVE_ARGUMENT = HelixMixinElementType("MIXIN_DIRECTIVE_ARGUMENT_NODE")
    val OPERAND = HelixMixinElementType("MIXIN_OPERAND")
    val REFERENCE_EXPRESSION = HelixMixinElementType("MIXIN_REFERENCE_EXPRESSION")
    val ROOT = HelixMixinElementType("MIXIN_ROOT")
    val MEMBER = HelixMixinElementType("MIXIN_MEMBER")
    val PATH = HelixMixinElementType("MIXIN_PATH_NODE")
    val FUNCTION_CALL = HelixMixinElementType("MIXIN_FUNCTION_CALL")
    val FUNCTION_ARGUMENT = HelixMixinElementType("MIXIN_FUNCTION_ARGUMENT")
    val COMMENT = HelixMixinElementType("MIXIN_COMMENT_NODE")
    val CONTINUATION = HelixMixinElementType("MIXIN_CONTINUATION_NODE")
    val ESCAPE = HelixMixinElementType("MIXIN_ESCAPE_NODE")
    val ERROR = HelixMixinElementType("MIXIN_ERROR_NODE")
    val DECLARATION = HelixMixinElementType("MIXIN_DECLARATION")
    val REFERENCE = HelixMixinElementType("MIXIN_REFERENCE")

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

class HelixMixinFile(viewProvider: FileViewProvider) : PsiFileBase(viewProvider, HelixMixinLanguage) {
    override fun getFileType() = HelixMixinFileType
    override fun toString() = "HELIX mixin file"
}

open class HelixMixinPsiElement(node: ASTNode) : ASTWrapperPsiElement(node)

class HelixMixinDeclarationElement(node: ASTNode) : HelixMixinPsiElement(node), PsiNameIdentifierOwner {
    override fun getNameIdentifier(): PsiElement = this
    override fun getName(): String = text
    override fun setName(name: String): PsiElement {
        val document = containingFile.viewProvider.document ?: return this
        document.replaceString(textRange.startOffset, textRange.endOffset, name)
        return this
    }
}

class HelixMixinReferenceElement(node: ASTNode) : HelixMixinPsiElement(node), PsiReference {
    override fun getReference(): PsiReference = this
    override fun getElement(): PsiElement = this
    override fun getRangeInElement(): TextRange = TextRange(0, textLength)
    override fun getCanonicalText(): String = text

    override fun resolve(): PsiElement? {
        val mixinType = HelixAntlrTypes.rules[dev.helight.helix.mixin.generated.MixinParser.RULE_mixinDeclaration]
        val functionType = HelixAntlrTypes.rules[dev.helight.helix.mixin.generated.MixinParser.RULE_funcDeclaration]
        val scope = generateSequence(parent) { it.parent }.firstOrNull { it.node.elementType == mixinType }
        val declarations = PsiTreeUtil.findChildrenOfType(containingFile, HelixMixinDeclarationElement::class.java)
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

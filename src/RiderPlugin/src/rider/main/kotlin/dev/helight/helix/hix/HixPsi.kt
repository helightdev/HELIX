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

object HixElementTypes {
    val FILE = IFileElementType("HIX_FILE", HixLanguage)
    val DECLARATION = HixElementType("MIXIN_DECLARATION")
    val REFERENCE = HixElementType("MIXIN_REFERENCE")
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
        if (!isValid) return null
        val mixinType = HelixAntlrTypes.rules[dev.helight.helix.hix.generated.HixParser.RULE_mixinDeclaration]
        val functionType = HelixAntlrTypes.rules[dev.helight.helix.hix.generated.HixParser.RULE_funcDeclaration]
        fun enclosingMixin(element: PsiElement?): PsiElement? =
            generateSequence(element) { it.parent }.firstOrNull { it.node?.elementType == mixinType }
        val file = containingFile ?: return null
        val scope = enclosingMixin(parent)
        val declarations = PsiTreeUtil.findChildrenOfType(file, HixDeclarationElement::class.java)
            .filter { it.isValid && it.text == text && it.parent?.node?.elementType == functionType }
        val local = declarations.filter { declaration ->
            enclosingMixin(declaration.parent) === scope
        }
        if (local.size == 1) return local.single()
        if (local.isNotEmpty()) return null
        return declarations.singleOrNull { declaration ->
            enclosingMixin(declaration.parent) == null
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

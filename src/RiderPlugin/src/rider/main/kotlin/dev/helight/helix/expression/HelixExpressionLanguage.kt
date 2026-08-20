package dev.helight.helix.expression

import com.intellij.extapi.psi.PsiFileBase
import com.intellij.lang.Language
import com.intellij.openapi.fileTypes.LanguageFileType
import com.intellij.psi.FileViewProvider
import javax.swing.Icon

object HelixExpressionLanguage : Language("HelixMixinExpression")

object HelixExpressionFileType : LanguageFileType(HelixExpressionLanguage) {
    override fun getName(): String = "HELIX Mixin Expression"
    override fun getDescription(): String = "HELIX mixin expression"
    override fun getDefaultExtension(): String = "hxmixin"
    override fun getIcon(): Icon? = null
}

class HelixExpressionFile(viewProvider: FileViewProvider) : PsiFileBase(viewProvider, HelixExpressionLanguage) {
    override fun getFileType() = HelixExpressionFileType
    override fun toString(): String = "HELIX mixin expression"
}

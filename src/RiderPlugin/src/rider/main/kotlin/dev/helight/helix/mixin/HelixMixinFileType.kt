package dev.helight.helix.mixin

import com.intellij.lang.Language
import com.intellij.openapi.fileTypes.LanguageFileType

object HelixMixinLanguage : Language("HelixMixin")

object HelixMixinFileType : LanguageFileType(HelixMixinLanguage) {
    override fun getDefaultExtension() = "HelixSourceGenerator.additionalfile"
    override fun getDescription() = "HELIX source-generator mixin library"
    override fun getIcon() = null
    override fun getName() = "HelixMixin"

    fun isCanonical(name: String): Boolean =
        name.endsWith(".HelixSourceGenerator.additionalfile", ignoreCase = true)
}

package dev.helight.helix.hix

import com.intellij.lang.Language
import com.intellij.openapi.fileTypes.LanguageFileType

object HixLanguage : Language("Hix")

object HixFileType : LanguageFileType(HixLanguage) {
    override fun getDefaultExtension() = "HelixSourceGenerator.additionalfile"
    override fun getDescription() = "Hix source-generator library"
    override fun getIcon() = null
    override fun getName() = "Hix"

    fun isCanonical(name: String): Boolean =
        name.endsWith(".HelixSourceGenerator.additionalfile", ignoreCase = true)
}

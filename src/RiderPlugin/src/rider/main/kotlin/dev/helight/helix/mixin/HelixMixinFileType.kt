package dev.helight.helix.mixin

import com.intellij.lang.Language
import com.jetbrains.rider.ideaInterop.fileTypes.RiderLanguageFileTypeBase

object HelixMixinLanguage : Language("HelixMixin")

object HelixMixinFileType : RiderLanguageFileTypeBase(HelixMixinLanguage) {
    override fun getDefaultExtension() = "HelixSourceGenerator.additionalfile"
    override fun getDescription() = "HELIX source-generator mixin library"
    override fun getIcon() = null
    override fun getName() = "HelixMixin"
}

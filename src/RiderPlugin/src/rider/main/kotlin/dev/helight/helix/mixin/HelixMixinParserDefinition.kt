package dev.helight.helix.mixin

import com.intellij.lexer.DummyLexer
import com.intellij.openapi.project.Project
import com.intellij.psi.tree.IElementType
import com.jetbrains.rider.ideaInterop.fileTypes.RiderFileElementType
import com.jetbrains.rider.ideaInterop.fileTypes.RiderParserDefinitionBase

class HelixMixinParserDefinition : RiderParserDefinitionBase(HelixMixinFileElementType, HelixMixinFileType) {
    companion object {
        val HelixMixinElementType = IElementType("RIDER_HELIX_MIXIN", HelixMixinLanguage)
        val HelixMixinFileElementType = RiderFileElementType(
            "RIDER_HELIX_MIXIN_FILE", HelixMixinLanguage, HelixMixinElementType
        )
    }

    override fun createLexer(project: Project?) = DummyLexer(HelixMixinFileElementType)
}

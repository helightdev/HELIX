package dev.helight.helix.expression

import com.intellij.openapi.editor.colors.TextAttributesKey
import com.intellij.openapi.fileTypes.SyntaxHighlighter
import com.intellij.openapi.options.colors.AttributesDescriptor
import com.intellij.openapi.options.colors.ColorDescriptor
import com.intellij.openapi.options.colors.ColorSettingsPage
import javax.swing.Icon

class HelixExpressionColorSettingsPage : ColorSettingsPage {
    override fun getDisplayName(): String = "HELIX Mixin Expression"
    override fun getIcon(): Icon? = null
    override fun getHighlighter(): SyntaxHighlighter = HelixExpressionSyntaxHighlighter()
    override fun getAdditionalHighlightingTagToDescriptorMap(): Map<String, TextAttributesKey>? = null
    override fun getAttributeDescriptors(): Array<AttributesDescriptor> = DESCRIPTORS
    override fun getColorDescriptors(): Array<ColorDescriptor> = ColorDescriptor.EMPTY_ARRAY

    override fun getDemoText(): String =
        """
        @FUNC<MixinCallbackImpl>
          @LOCAL<Name> @attr#target:unwrap
          @SCOPE
            @MATCH @local#Name:eq<null>
            @ASSERT @target:name:matches<^On.*>
            @LOCAL<IsImplicit> true
            @Local<Name> $@target:name:replaceFirst<^On><>
          @END
        @END
        """.trimIndent()

    companion object {
        private val DESCRIPTORS = arrayOf(
            AttributesDescriptor("Directive", HelixExpressionSyntaxHighlighter.DIRECTIVE),
            AttributesDescriptor("Value", HelixExpressionSyntaxHighlighter.VALUE),
            AttributesDescriptor("Path", HelixExpressionSyntaxHighlighter.PATH),
            AttributesDescriptor("Function", HelixExpressionSyntaxHighlighter.FUNCTION),
            AttributesDescriptor("Argument", HelixExpressionSyntaxHighlighter.ARGUMENT),
            AttributesDescriptor("Operator", HelixExpressionSyntaxHighlighter.OPERATOR),
            AttributesDescriptor("Braces", HelixExpressionSyntaxHighlighter.BRACES),
            AttributesDescriptor("Escape", HelixExpressionSyntaxHighlighter.ESCAPE),
            AttributesDescriptor("Bad character", HelixExpressionSyntaxHighlighter.BAD_CHARACTER),
        )
    }
}

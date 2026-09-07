package dev.helight.helix.mixin

import com.intellij.execution.process.ConsoleHighlighter
import com.intellij.lexer.Lexer
import com.intellij.openapi.editor.DefaultLanguageHighlighterColors
import com.intellij.openapi.editor.HighlighterColors
import com.intellij.openapi.editor.colors.TextAttributesKey
import com.intellij.openapi.editor.markup.TextAttributes
import com.intellij.openapi.fileTypes.SyntaxHighlighter
import com.intellij.openapi.fileTypes.SyntaxHighlighterBase
import com.intellij.openapi.fileTypes.SyntaxHighlighterFactory
import com.intellij.openapi.options.colors.AttributesDescriptor
import com.intellij.openapi.options.colors.ColorDescriptor
import com.intellij.openapi.options.colors.ColorSettingsPage
import com.intellij.openapi.options.colors.pages.DefaultLanguageColorsPage
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.psi.tree.IElementType
import com.jetbrains.rider.ideaInterop.settings.colors.demoTexts.RiderDefaultLanguageColorsDemoText
import javax.swing.Icon
import dev.helight.helix.mixin.generated.MixinLexer

object HelixMixinColors {
    val DIRECTIVE = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_DIRECTIVE", DefaultLanguageHighlighterColors.KEYWORD
    )
    val VALUE = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_VALUE", DefaultLanguageHighlighterColors.NUMBER
    )
    val PATH = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_PATH", DefaultLanguageHighlighterColors.INSTANCE_FIELD
    )
    val FUNCTION = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_FUNCTION", DefaultLanguageHighlighterColors.FUNCTION_CALL
    )
    val ARGUMENT = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_ARGUMENT", DefaultLanguageHighlighterColors.STRING
    )
    val LABEL = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_LABEL", DefaultLanguageHighlighterColors.LABEL
    )
    val LOCAL = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_LOCAL", DefaultLanguageHighlighterColors.INSTANCE_FIELD
    )
    val VARIABLE = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_VARIABLE", DefaultLanguageHighlighterColors.INSTANCE_FIELD
    )
    val FUNCTION_IDENTIFIER = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_FUNCTION_IDENTIFIER", DefaultLanguageHighlighterColors.FUNCTION_DECLARATION
    )
    val TYPE = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_CSHARP_TYPE", DefaultLanguageHighlighterColors.CLASS_NAME
    )
    val COMMENT = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_COMMENT", DefaultLanguageHighlighterColors.LINE_COMMENT
    )
    val ESCAPE = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_ESCAPE", DefaultLanguageHighlighterColors.VALID_STRING_ESCAPE
    )
    val BAD = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_BAD_CHARACTER", HighlighterColors.BAD_CHARACTER
    )
    val INACTIVE = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_INACTIVE", ConsoleHighlighter.DARKGRAY
    )

    val TEMPLATE = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_TEMPLATE", DefaultLanguageHighlighterColors.TEMPLATE_LANGUAGE_COLOR
    )

    val DELEGATE = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_DELEGATE", TextAttributesKey.createTextAttributesKey("ReSharper.DELEGATE_IDENTIFIER")
    )
}

class HelixMixinSyntaxHighlighterFactory : SyntaxHighlighterFactory() {
    override fun getSyntaxHighlighter(project: Project?, virtualFile: VirtualFile?): SyntaxHighlighter =
        HelixMixinSyntaxHighlighter(project)
}

class HelixMixinSyntaxHighlighter(private val project: Project?) : SyntaxHighlighterBase() {
    override fun getHighlightingLexer(): Lexer = HelixMixinLexer(project)

    override fun getTokenHighlights(tokenType: IElementType): Array<TextAttributesKey> {
        val type = (tokenType as? HelixAntlrTokenType)?.antlrType ?: return emptyArray()
        val name = MixinLexer.VOCABULARY.getSymbolicName(type).orEmpty()
        return pack(when {
            name.startsWith("KEYWORD_") -> HelixMixinColors.DIRECTIVE
            type == MixinLexer.ROOT_IDENTIFIER || type == MixinLexer.VALUE_SMART_ROOT -> HelixMixinColors.VALUE
            type == MixinLexer.FUNCTION_IDENTIFIER -> HelixMixinColors.FUNCTION
            type == MixinLexer.MEMBER_IDENTIFIER -> HelixMixinColors.PATH
            type == MixinLexer.LABEL_IDENTIFIER -> HelixMixinColors.LABEL
            type in setOf(MixinLexer.ARGUMENT_TEXT, MixinLexer.BEGIN_ARGUMENT, MixinLexer.ARGUMENT_END) -> HelixMixinColors.ARGUMENT
            type in setOf(MixinLexer.CONTENT_TEXT, MixinLexer.BEGIN_CONTENT) -> HelixMixinColors.TEMPLATE
            type in setOf(MixinLexer.COMMENT, MixinLexer.SLASH_COMMENT) -> HelixMixinColors.COMMENT
            type in setOf(MixinLexer.CONTENT_WRAP, MixinLexer.CONTENT_LINEBREAK, MixinLexer.VALUE_WRAP) -> HelixMixinColors.INACTIVE
            type in setOf(MixinLexer.ESCAPE, MixinLexer.ESCAPE_HEX, MixinLexer.ESCAPE_LITERAL, MixinLexer.ESCAPE_MACRO) -> HelixMixinColors.ESCAPE
            type == MixinLexer.ERROR_TOKEN -> HelixMixinColors.BAD
            else -> null
        })
    }
}

class HelixMixinColorSettingsPage : ColorSettingsPage {
    override fun getDisplayName(): String = "HELIX Mixin"
    override fun getIcon(): Icon? = null
    override fun getHighlighter(): SyntaxHighlighter = HelixMixinSyntaxHighlighter(null)
    override fun getAttributeDescriptors(): Array<AttributesDescriptor> = arrayOf(
        AttributesDescriptor("Directive", HelixMixinColors.DIRECTIVE),
        AttributesDescriptor("Expression root", HelixMixinColors.VALUE),
        AttributesDescriptor("Member path", HelixMixinColors.PATH),
        AttributesDescriptor("Built-in function", HelixMixinColors.FUNCTION),
        AttributesDescriptor("Literal argument", HelixMixinColors.ARGUMENT),
        AttributesDescriptor("Label", HelixMixinColors.LABEL),
        AttributesDescriptor("Local", HelixMixinColors.LOCAL),
        AttributesDescriptor("Variable", HelixMixinColors.VARIABLE),
        AttributesDescriptor("Function identifier", HelixMixinColors.FUNCTION_IDENTIFIER),
        AttributesDescriptor("C# type", HelixMixinColors.TYPE),
        AttributesDescriptor("Comment", HelixMixinColors.COMMENT),
        AttributesDescriptor("Escape", HelixMixinColors.ESCAPE),
        AttributesDescriptor("Invalid syntax", HelixMixinColors.BAD),
        AttributesDescriptor("Inactive", HelixMixinColors.INACTIVE),
        AttributesDescriptor("Template", HelixMixinColors.TEMPLATE),
        AttributesDescriptor("Delegate", HelixMixinColors.DELEGATE)
    )

    override fun getColorDescriptors(): Array<ColorDescriptor> = ColorDescriptor.EMPTY_ARRAY
    override fun getAdditionalHighlightingTagToDescriptorMap(): Map<String, TextAttributesKey> = mapOf(
        "label" to HelixMixinColors.LABEL,
        "local" to HelixMixinColors.LOCAL,
        "variable" to HelixMixinColors.VARIABLE,
        "functionId" to HelixMixinColors.DELEGATE,
        "type" to HelixMixinColors.TYPE
    )

    override fun getDemoText(): String = """// HELIX mixin language
pure func Describe sig @{name=string} -> string {
  return(<Hello [param#name]>)
}
mixin HELIX.Compose.ExampleAttribute {
  prelude expression {
    carry Name @= target:name;
  }
  expression {
    emit @> // {{carry#Name}}
  }
}
"""
}

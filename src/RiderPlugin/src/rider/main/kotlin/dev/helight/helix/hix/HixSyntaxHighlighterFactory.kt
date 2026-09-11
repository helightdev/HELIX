package dev.helight.helix.hix

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
import dev.helight.helix.hix.generated.HixLexer

object HixColors {
    val KEYWORD = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_DIRECTIVE", DefaultLanguageHighlighterColors.KEYWORD
    )
    val VALUE = TextAttributesKey.createTextAttributesKey(
        "HELIX_MIXIN_VALUE", DefaultLanguageHighlighterColors.NUMBER
    )
    val NUMBER = TextAttributesKey.createTextAttributesKey(
        "HIX_NUMBER", DefaultLanguageHighlighterColors.NUMBER
    )
    val BOOLEAN = TextAttributesKey.createTextAttributesKey(
        "HIX_BOOLEAN", DefaultLanguageHighlighterColors.KEYWORD
    )
    val NULL = TextAttributesKey.createTextAttributesKey(
        "HIX_NULL", DefaultLanguageHighlighterColors.KEYWORD
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
    val PARAMETER = TextAttributesKey.createTextAttributesKey(
        "HIX_PARAMETER", DefaultLanguageHighlighterColors.PARAMETER
    )
    val FIELD = TextAttributesKey.createTextAttributesKey(
        "HIX_FIELD", DefaultLanguageHighlighterColors.INSTANCE_FIELD
    )
    val MIXIN = TextAttributesKey.createTextAttributesKey(
        "HIX_MIXIN_DECLARATION", DefaultLanguageHighlighterColors.CLASS_NAME
    )
    val METADATA = TextAttributesKey.createTextAttributesKey(
        "HIX_METADATA", DefaultLanguageHighlighterColors.METADATA
    )
    val PATTERN_METADATA = TextAttributesKey.createTextAttributesKey(
        "HIX_PATTERN_METADATA", DefaultLanguageHighlighterColors.METADATA
    )
    val OPERATOR = TextAttributesKey.createTextAttributesKey(
        "HIX_OPERATOR", DefaultLanguageHighlighterColors.OPERATION_SIGN
    )
    val SEPARATOR = TextAttributesKey.createTextAttributesKey(
        "HIX_SEPARATOR", DefaultLanguageHighlighterColors.SEMICOLON
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

class HixSyntaxHighlighterFactory : SyntaxHighlighterFactory() {
    override fun getSyntaxHighlighter(project: Project?, virtualFile: VirtualFile?): SyntaxHighlighter =
        HixSyntaxHighlighter(project)
}

class HixSyntaxHighlighter(private val project: Project?) : SyntaxHighlighterBase() {
    override fun getHighlightingLexer(): Lexer = HixEditorLexer(project)

    override fun getTokenHighlights(tokenType: IElementType): Array<TextAttributesKey> {
        val type = (tokenType as? HelixAntlrTokenType)?.antlrType ?: return emptyArray()
        val name = HixLexer.VOCABULARY.getSymbolicName(type).orEmpty()
        return pack(when {
            name.startsWith("KEYWORD_") -> HixColors.KEYWORD
            type == HixLexer.BEGIN_LAMBDA_BLOCK || type == HixLexer.BEGIN_LAMBDA_ARROW -> HixColors.KEYWORD
            type in metadataTokens -> HixColors.METADATA
            type == HixLexer.NUMBER -> HixColors.NUMBER
            type == HixLexer.BOOLEAN -> HixColors.BOOLEAN
            type == HixLexer.NULL || type == HixLexer.MISSING -> HixColors.NULL
            type == HixLexer.ROOT_IDENTIFIER || type == HixLexer.VALUE_SMART_ROOT ||
                type == HixLexer.TOPLEVEL_DERIVATION -> HixColors.VALUE
            type == HixLexer.FUNCTION_IDENTIFIER -> HixColors.FUNCTION
            type == HixLexer.VALUE_MEMBER -> HixColors.PATH
            type == HixLexer.MEMBER_IDENTIFIER -> HixColors.PATH
            type == HixLexer.LABEL_IDENTIFIER -> HixColors.LABEL
            type in operatorTokens -> HixColors.OPERATOR
            type in separatorTokens -> HixColors.SEPARATOR
            type in setOf(HixLexer.ARGUMENT_TEXT, HixLexer.BEGIN_ARGUMENT, HixLexer.ARGUMENT_END) -> HixColors.ARGUMENT
            type == HixLexer.CONTENT_TEXT -> HixColors.TEMPLATE
            type in setOf(HixLexer.COMMENT, HixLexer.SLASH_COMMENT) -> HixColors.COMMENT
            type in setOf(HixLexer.CONTENT_WRAP, HixLexer.CONTENT_LINEBREAK, HixLexer.VALUE_WRAP) -> HixColors.INACTIVE
            type in setOf(HixLexer.ESCAPED_START, HixLexer.ESCAPE, HixLexer.ESCAPE_HEX, HixLexer.ESCAPE_LITERAL, HixLexer.ESCAPE_MACRO) -> HixColors.ESCAPE
            type == HixLexer.ERROR_TOKEN -> HixColors.BAD
            else -> null
        })
    }

    private companion object {
        val metadataTokens = setOf(HixLexer.METADATA_PREFIX, HixLexer.BEGIN_METADATA_VALUE)
        val operatorTokens = setOf(
            HixLexer.ASSIGN, HixLexer.ARROW, HixLexer.FAT_ARROW, HixLexer.VALUE_ASSIGN,
            HixLexer.VALUE_EXPAND, HixLexer.VALUE_FUNCTION, HixLexer.VALUE_PREDICATE,
            HixLexer.NOT_VALUE, HixLexer.QUESTION, HixLexer.VALUE_ELVIS
        )
        val separatorTokens = setOf(
            HixLexer.SECTION_DELIMITER, HixLexer.SEMICOLON, HixLexer.COMMA, HixLexer.VALUE_DELIMITER
        )
    }
}

class HixColorSettingsPage : ColorSettingsPage {
    override fun getDisplayName(): String = "Hix"
    override fun getIcon(): Icon? = null
    override fun getHighlighter(): SyntaxHighlighter = HixSyntaxHighlighter(null)
    override fun getAttributeDescriptors(): Array<AttributesDescriptor> = arrayOf(
        AttributesDescriptor("Keyword", HixColors.KEYWORD),
        AttributesDescriptor("Expression root", HixColors.VALUE),
        AttributesDescriptor("Number", HixColors.NUMBER),
        AttributesDescriptor("Boolean", HixColors.BOOLEAN),
        AttributesDescriptor("Null", HixColors.NULL),
        AttributesDescriptor("Member path", HixColors.PATH),
        AttributesDescriptor("Built-in function", HixColors.FUNCTION),
        AttributesDescriptor("Literal argument", HixColors.ARGUMENT),
        AttributesDescriptor("Label", HixColors.LABEL),
        AttributesDescriptor("Local", HixColors.LOCAL),
        AttributesDescriptor("Variable", HixColors.VARIABLE),
        AttributesDescriptor("Function declaration", HixColors.FUNCTION_IDENTIFIER),
        AttributesDescriptor("Type", HixColors.TYPE),
        AttributesDescriptor("Parameter", HixColors.PARAMETER),
        AttributesDescriptor("Field", HixColors.FIELD),
        AttributesDescriptor("Mixin declaration", HixColors.MIXIN),
        AttributesDescriptor("Metadata", HixColors.METADATA),
        AttributesDescriptor("Pattern metadata", HixColors.PATTERN_METADATA),
        AttributesDescriptor("Operator", HixColors.OPERATOR),
        AttributesDescriptor("Separator", HixColors.SEPARATOR),
        AttributesDescriptor("Comment", HixColors.COMMENT),
        AttributesDescriptor("Escape", HixColors.ESCAPE),
        AttributesDescriptor("Invalid syntax", HixColors.BAD),
        AttributesDescriptor("Inactive", HixColors.INACTIVE),
        AttributesDescriptor("Template", HixColors.TEMPLATE),
        AttributesDescriptor("Delegate", HixColors.DELEGATE)
    )

    override fun getColorDescriptors(): Array<ColorDescriptor> = ColorDescriptor.EMPTY_ARRAY
    override fun getAdditionalHighlightingTagToDescriptorMap(): Map<String, TextAttributesKey> = mapOf(
        "label" to HixColors.LABEL,
        "local" to HixColors.LOCAL,
        "variable" to HixColors.VARIABLE,
        "functionId" to HixColors.DELEGATE,
        "type" to HixColors.TYPE,
        "parameter" to HixColors.PARAMETER,
        "field" to HixColors.FIELD,
        "metadata" to HixColors.METADATA,
        "patternMetadata" to HixColors.PATTERN_METADATA,
        "mixin" to HixColors.MIXIN
    )

    override fun getDemoText(): String = """// Hix language
%type<Example>
%guid<12345678-1234-1234-1234-123456789012>
---
type Person = @{string name, %optional number age}
pure func Describe(Person person, string greeting) -> string {
  return(<[greeting], [person#name]!>)
}
mixin <HELIX.Compose.ExampleAttribute> {
  prelude expression {
    local name = target#name;
  }
  expression {
    emit @> // {{name}}
  }
}
"""
}

package dev.helight.helix.expression

import com.intellij.psi.tree.IElementType
import com.intellij.psi.tree.TokenSet

class HelixExpressionTokenType(debugName: String) : IElementType(debugName, HelixExpressionLanguage)
class HelixExpressionElementType(debugName: String) : IElementType(debugName, HelixExpressionLanguage)

object HelixExpressionTypes {
    @JvmField val DIRECTIVE = HelixExpressionTokenType("DIRECTIVE")
    @JvmField val VALUE = HelixExpressionTokenType("VALUE")
    @JvmField val PATH = HelixExpressionTokenType("PATH")
    @JvmField val FUNCTION = HelixExpressionTokenType("FUNCTION")
    @JvmField val ARGUMENT = HelixExpressionTokenType("ARGUMENT")
    @JvmField val PREDICATE = HelixExpressionTokenType("PREDICATE")
    @JvmField val NEGATED_PREDICATE = HelixExpressionTokenType("NEGATED_PREDICATE")
    @JvmField val INVOKE = HelixExpressionTokenType("INVOKE")
    @JvmField val ENCLOSED_START = HelixExpressionTokenType("ENCLOSED_START")
    @JvmField val ENCLOSED_END = HelixExpressionTokenType("ENCLOSED_END")
    @JvmField val ESCAPED_AT = HelixExpressionTokenType("ESCAPED_AT")
    @JvmField val TEXT = HelixExpressionTokenType("TEXT")
    @JvmField val WHITE_SPACE = HelixExpressionTokenType("WHITE_SPACE")
    @JvmField val NEW_LINE = HelixExpressionTokenType("NEW_LINE")
    @JvmField val BAD_CHARACTER = HelixExpressionTokenType("BAD_CHARACTER")

    @JvmField val FILE = HelixExpressionElementType("FILE")
    @JvmField val STATEMENT = HelixExpressionElementType("STATEMENT")
    @JvmField val DIRECTIVE_CALL = HelixExpressionElementType("DIRECTIVE_CALL")
    @JvmField val EXPRESSION = HelixExpressionElementType("EXPRESSION")
    @JvmField val REFERENCE = HelixExpressionElementType("REFERENCE")
    @JvmField val FUNCTION_CALL = HelixExpressionElementType("FUNCTION_CALL")

    @JvmField val WHITE_SPACES: TokenSet = TokenSet.create(WHITE_SPACE)
}

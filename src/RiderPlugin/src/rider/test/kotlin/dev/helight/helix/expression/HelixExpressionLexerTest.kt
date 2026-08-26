package dev.helight.helix.expression

import com.intellij.psi.tree.IElementType
import kotlin.test.Test
import kotlin.test.assertEquals

class HelixExpressionLexerTest {
    @Test
    fun `lexes canonical directive and value chains`() {
        val lexer = HelixExpressionLexer()
        lexer.start("@MATCH @attr#required:!?eq<false>\n@MIXIN<\$Init> Call(@(target:name));")
        val tokens = mutableListOf<Pair<IElementType, String>>()
        while (lexer.tokenType != null) {
            tokens += lexer.tokenType!! to lexer.bufferSequence.subSequence(lexer.tokenStart, lexer.tokenEnd).toString()
            lexer.advance()
        }

        assertEquals(
            listOf(
                HelixExpressionTypes.DIRECTIVE to "@MATCH",
                HelixExpressionTypes.VALUE to "@attr",
                HelixExpressionTypes.PATH to "#required",
                HelixExpressionTypes.NEGATED_PREDICATE to ":!?",
                HelixExpressionTypes.FUNCTION to "eq",
                HelixExpressionTypes.ARGUMENT to "<false>",
                HelixExpressionTypes.NEW_LINE to "\n",
                HelixExpressionTypes.DIRECTIVE to "@MIXIN",
                HelixExpressionTypes.ARGUMENT to "<\$Init>",
                HelixExpressionTypes.TEXT to "Call(",
                HelixExpressionTypes.ENCLOSED_START to "@(",
                HelixExpressionTypes.TEXT to "target",
                HelixExpressionTypes.INVOKE to ":",
                HelixExpressionTypes.FUNCTION to "name",
                HelixExpressionTypes.ENCLOSED_END to ")",
                HelixExpressionTypes.TEXT to ")",
                HelixExpressionTypes.TEXT to ";",
            ),
            tokens.filterNot { it.first == HelixExpressionTypes.WHITE_SPACE },
        )
    }

    @Test
    fun `lexes continuations and comments at indented line starts`() {
        val lexer = HelixExpressionLexer()
        lexer.start("@CODE First(\n  @+second)\n\t@\\third\n @# ignored @target:name")
        val tokens = mutableListOf<Pair<IElementType, String>>()
        while (lexer.tokenType != null) {
            tokens += lexer.tokenType!! to lexer.bufferSequence
                .subSequence(lexer.tokenStart, lexer.tokenEnd).toString()
            lexer.advance()
        }

        assertEquals(
            listOf(
                HelixExpressionTypes.DIRECTIVE to "@CODE",
                HelixExpressionTypes.TEXT to "First(",
                HelixExpressionTypes.NEW_LINE to "\n",
                HelixExpressionTypes.DIRECT_CONTINUATION to "@+",
                HelixExpressionTypes.TEXT to "second",
                HelixExpressionTypes.TEXT to ")",
                HelixExpressionTypes.NEW_LINE to "\n",
                HelixExpressionTypes.NEWLINE_CONTINUATION to "@\\",
                HelixExpressionTypes.TEXT to "third",
                HelixExpressionTypes.NEW_LINE to "\n",
                HelixExpressionTypes.COMMENT to "@# ignored @target:name",
            ),
            tokens.filterNot { it.first == HelixExpressionTypes.WHITE_SPACE },
        )
    }
}

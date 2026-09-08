package dev.helight.helix.hix

import dev.helight.helix.hix.generated.HixLexer
import dev.helight.helix.hix.generated.HixParser
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

class HixLocalSyntaxTest {
    @Test
    fun `generated parser accepts nested declarations and typed values`() {
        val source = "pure func describe sig @{name=string} -> string { return(<Hello [param#name]>) }\n" +
            "mixin Example { prelude expression { carry local name @= target:name; } expression { emit(describe(local#name)) } }" +
            "\npure func answer => 42;" +
            "\nmixin Lambdas { expression { local values = map(@[<a>], func => <[$0]!>); local other = func { return($0) } } }"
        val parsed = HixAntlrSyntax.parse(source)
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertTrue(HixAntlrSyntax.rules(parsed.tree).any { it is HixParser.FuncDeclarationContext })
        assertTrue(HixAntlrSyntax.rules(parsed.tree).any { it is HixParser.ExpressionDeclarationContext })
    }

    @Test
    fun `number literals have their own token and highlighting`() {
        val source = "mixin E { expression { emit(-12.5) } }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertTrue(parsed.tokens.any { it.type == HixLexer.NUMBER && source.substring(it.start, it.end) == "-12.5" })
        assertTrue(HixSyntaxHighlighter(null)
            .getTokenHighlights(HelixAntlrTypes.tokens[HixLexer.NUMBER]).isNotEmpty())
    }

    @Test
    fun `boolean literals have their own token and highlighting`() {
        val source = "mixin E { expression { emit(true); emit(false) } }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertTrue(parsed.tokens.count { it.type == HixLexer.BOOLEAN } == 2)
        assertTrue(HixSyntaxHighlighter(null)
            .getTokenHighlights(HelixAntlrTypes.tokens[HixLexer.BOOLEAN]).isNotEmpty())
    }

    @Test
    fun `null literal has its own token and highlighting`() {
        val source = "pure func empty sig null -> null { return(null) }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertTrue(parsed.tokens.count { it.type == HixLexer.NULL } == 3)
        assertTrue(HixSyntaxHighlighter(null)
            .getTokenHighlights(HelixAntlrTypes.tokens[HixLexer.NULL]).isNotEmpty())
    }

    @Test
    fun `lexer preserves all source including skipped whitespace`() {
        val source = "mixin E {\n expression { local x = @[<a>, @{name=<b>}] }\n}"
        val parsed = HixAntlrSyntax.parse(source)
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(source, parsed.tokens.joinToString("") { source.substring(it.start, it.end) })
        assertTrue(parsed.tokens.any { it.type == HixLexer.BEGIN_TUPLE })
        assertTrue(parsed.tokens.any { it.type == HixLexer.BEGIN_TABLE })
    }

    @Test
    fun `content interpolation is structural and continuations remain tokens`() {
        val source = "mixin E { expression { emit @> Hello {{this:name}}\n@+!\n} }"
        val parsed = HixAntlrSyntax.parse(source)
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertTrue(HixAntlrSyntax.rules(parsed.tree).any { it is HixParser.ContentInterpolateContext })
        assertTrue(parsed.tokens.any { it.type == HixLexer.CONTENT_WRAP })
    }

    @Test
    fun `content operator is not highlighted as template content`() {
        val highlighter = HixSyntaxHighlighter(null)

        assertTrue(highlighter.getTokenHighlights(HelixAntlrTypes.tokens[HixLexer.BEGIN_CONTENT]).isEmpty())
        assertTrue(highlighter.getTokenHighlights(HelixAntlrTypes.tokens[HixLexer.CONTENT_TEXT]).isNotEmpty())
    }

    @Test
    fun `UTF16 ranges survive supplementary characters`() {
        val source = "mixin E { expression { emit(<😀[true]>) } }"
        val parsed = HixAntlrSyntax.parse(source)
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        val boolean = parsed.tokens.single { it.type == HixLexer.BOOLEAN }
        assertEquals("true", source.substring(boolean.start, boolean.end))
    }

    @Test
    fun `incomplete input reports diagnostics without losing source coverage`() {
        val source = "mixin E { expression { emit(<hello ["
        val parsed = HixAntlrSyntax.parse(source)
        assertTrue(parsed.diagnostics.isNotEmpty())
        assertEquals(source, parsed.tokens.joinToString("") { source.substring(it.start, it.end) })
    }

    @Test
    fun `incremental restart reproduces full lexer tokens`() {
        val source = "mixin E { expression { emit(<Hello [true]>) } }"
        val lexer = HixEditorLexer(null)
        lexer.start(source, 0, source.length, 0)
        val starts = ArrayList<Int>()
        while (lexer.tokenType != null) { starts += lexer.tokenStart; lexer.advance() }
        for (start in starts) {
            lexer.start(source, start, source.length, 0)
            assertEquals(start, lexer.tokenStart)
            var position = start
            while (lexer.tokenType != null) {
                assertEquals(position, lexer.tokenStart)
                position = lexer.tokenEnd
                lexer.advance()
            }
            assertEquals(source.length, position)
        }
    }

    @Test
    fun `legacy syntax is rejected`() {
        assertTrue(HixAntlrSyntax.parse("@FUNC<Build>\n@END").diagnostics.isNotEmpty())
    }
}

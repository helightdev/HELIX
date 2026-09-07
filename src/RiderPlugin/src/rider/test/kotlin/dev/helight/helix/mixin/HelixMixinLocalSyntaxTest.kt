package dev.helight.helix.mixin

import dev.helight.helix.mixin.generated.MixinLexer
import dev.helight.helix.mixin.generated.MixinParser
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

class HelixMixinLocalSyntaxTest {
    @Test
    fun `generated parser accepts nested declarations and typed values`() {
        val source = "pure func describe sig @{name=string} -> string { return(<Hello [param#name]>) }\n" +
            "mixin Example { prelude expression { carry name @= target:name; } expression { emit(describe(carry#name)) } }"
        val parsed = HelixMixinAntlrSyntax.parse(source)
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertTrue(HelixMixinAntlrSyntax.rules(parsed.tree).any { it is MixinParser.FuncDeclarationContext })
        assertTrue(HelixMixinAntlrSyntax.rules(parsed.tree).any { it is MixinParser.ExpressionDeclarationContext })
    }

    @Test
    fun `lexer preserves all source including skipped whitespace`() {
        val source = "mixin E {\n expression { local x = @[<a>, @{name=<b>}] }\n}"
        val parsed = HelixMixinAntlrSyntax.parse(source)
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(source, parsed.tokens.joinToString("") { source.substring(it.start, it.end) })
        assertTrue(parsed.tokens.any { it.type == MixinLexer.BEGIN_TUPLE })
        assertTrue(parsed.tokens.any { it.type == MixinLexer.BEGIN_TABLE })
    }

    @Test
    fun `content interpolation is structural and continuations remain tokens`() {
        val source = "mixin E { expression { emit @> Hello {{this:name}}\n@+!\n} }"
        val parsed = HelixMixinAntlrSyntax.parse(source)
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertTrue(HelixMixinAntlrSyntax.rules(parsed.tree).any { it is MixinParser.ContentInterpolateContext })
        assertTrue(parsed.tokens.any { it.type == MixinLexer.CONTENT_WRAP })
    }

    @Test
    fun `UTF16 ranges survive supplementary characters`() {
        val source = "mixin E { expression { emit(<😀[true]>) } }"
        val parsed = HelixMixinAntlrSyntax.parse(source)
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        val root = parsed.tokens.single { it.type == MixinLexer.ROOT_IDENTIFIER }
        assertEquals("true", source.substring(root.start, root.end))
    }

    @Test
    fun `incomplete input reports diagnostics without losing source coverage`() {
        val source = "mixin E { expression { emit(<hello ["
        val parsed = HelixMixinAntlrSyntax.parse(source)
        assertTrue(parsed.diagnostics.isNotEmpty())
        assertEquals(source, parsed.tokens.joinToString("") { source.substring(it.start, it.end) })
    }

    @Test
    fun `incremental restart reproduces full lexer tokens`() {
        val source = "mixin E { expression { emit(<Hello [true]>) } }"
        val lexer = HelixMixinLexer(null)
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
        assertTrue(HelixMixinAntlrSyntax.parse("@FUNC<Build>\n@END").diagnostics.isNotEmpty())
    }
}

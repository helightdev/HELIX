package dev.helight.helix.hix

import dev.helight.helix.hix.generated.HixLexer
import dev.helight.helix.hix.generated.HixParser
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue
import kotlin.test.assertSame

class HixLocalSyntaxTest {
    @Test
    fun `hal edits do not invalidate hix analysis`() {
        assertTrue(HixSnapshotService.affectsHixAnalysis("/project/manifest.hix"))
        assertTrue(!HixSnapshotService.affectsHixAnalysis("/project/assets/person.hal"))
    }

    @Test
    fun `canonical source-generator files and standalone hix files use the hix file type`() {
        assertEquals("hix", HixFileType.defaultExtension)
        assertTrue(HixFileType.isCanonical("manifest.hix"))
        assertTrue(HixFileType.isCanonical("Library.HIX"))
        assertTrue(HixFileType.isCanonical("Core.HelixSourceGenerator.additionalfile"))
        assertTrue(HixFileType.isCanonical("Core.HELIXSOURCEGENERATOR.ADDITIONALFILE"))
    }

    @Test
    fun `current language tokens use dedicated highlighting categories`() {
        val highlighter = HixSyntaxHighlighter(null)

        assertSame(HixColors.METADATA,
            highlighter.getTokenHighlights(HelixAntlrTypes.tokens[HixLexer.METADATA_PREFIX]).single())
        assertSame(HixColors.OPERATOR,
            highlighter.getTokenHighlights(HelixAntlrTypes.tokens[HixLexer.ARROW]).single())
        assertSame(HixColors.SEPARATOR,
            highlighter.getTokenHighlights(HelixAntlrTypes.tokens[HixLexer.SECTION_DELIMITER]).single())
    }

    @Test
    fun `patterns delegates and typed function parameters parse locally`() {
        val source = "type Person = @{string name, %optional number age}\n" +
            "type Handler = delegate(Person self, string value) -> null\n" +
            "pure func describe(Person self, string value) -> string { return(param#value) }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(setOf("Person", "Handler", "describe"),
            HixAntlrSyntax.declarationNames(parsed.tree).map { it.text }.toSet())
        assertTrue(HixAntlrSyntax.rules(parsed.tree).any { it is HixParser.DelegatePatternContext })
    }

    @Test
    fun `generated parser accepts nested declarations and typed values`() {
        val source = "pure func describe(string name) -> string { return(<Hello [\$name]>) }\n" +
            "mixin Example { prelude expression { carry local name @= target:name; } expression { emit(describe(local#name)) } }" +
            "\npure func answer => 42;" +
            "\nmixin Lambdas { expression { local values = map(@[<a>], func => <[$0]!>); local other = func { return($0) } } }"
        val parsed = HixAntlrSyntax.parse(source)
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertTrue(HixAntlrSyntax.rules(parsed.tree).any { it is HixParser.FuncDeclarationContext })
        assertTrue(HixAntlrSyntax.rules(parsed.tree).any { it is HixParser.ExpressionDeclarationContext })
    }

    @Test
    fun `functions and mixins accept argument based declaration names`() {
        val source = "pure func <answer with spaces> => 42;\nmixin <HELIX.Example-Type> { expression { emit(<ok>) } }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertTrue(HixAntlrSyntax.rules(parsed.tree).any {
            it is HixParser.FunctionDeclarationIdentifierContext && it.argumentValue() != null
        })
        assertTrue(HixAntlrSyntax.rules(parsed.tree).any {
            it is HixParser.MixinIdentifierContext && it.argumentValue() != null
        })
    }

    @Test
    fun `top level and table field metadata parse structurally`() {
        val source = "%deprecated\n%since(<2.0>)\n%[<future>]\n" +
            "pure func annotated(%optional string value) -> string { " +
            "return(@{%[<field-note>] value=\$value}) }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(5, HixAntlrSyntax.rules(parsed.tree).count { it is HixParser.MetadataContext })
        assertEquals(2, HixAntlrSyntax.rules(parsed.tree).count { it is HixParser.MetadataValueContext })
    }

    @Test
    fun `metadata can appear inline before a declaration`() {
        val source = "%deprecated %since(<2.0>) func Build { return(null) }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(2, HixAntlrSyntax.rules(parsed.tree).count { it is HixParser.MetadataContext })
    }

    @Test
    fun `named field metadata ends at whitespace`() {
        val source = "pure func typed(%many<string> tuple test, %const<test> string another) " +
            "-> string { return(@{%anyOf<string><test> test=<yes>}) }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(3, HixAntlrSyntax.rules(parsed.tree).count { it is HixParser.MetadataContext })
    }

    @Test
    fun `section delimiter separates file metadata from declarations`() {
        val source = "%type<Item>\n%guid<12345678-1234-1234-1234-123456789012>\n" +
            "%name<My Custom Item>\n%interaction<something>\n---\nmixin Test { }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertTrue(HixAntlrSyntax.rules(parsed.tree).any { it is HixParser.FileMetadataSectionContext })
        assertEquals(1, parsed.tokens.count { it.type == HixLexer.SECTION_DELIMITER })
    }

    @Test
    fun `comma separated values accept a trailing comma`() {
        val source = "pure func trailing(string first, string second,) -> string { " +
            "local tuple = @[<a>, <b>,]; local table = @{first=<a>, second=<b>,}; return(join(<a>, <b>,)) }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
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
        val source = "pure func empty (null value) -> null { return(null) }"
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
    fun `malformed percent parenthesis never underflows lexer modes`() {
        listOf(
            "%(",
            "mixin E { %( }",
            "mixin E { expression { emit(<before>) } }\n%("
        ).forEach { source ->
            val parsed = HixAntlrSyntax.parse(source)
            assertTrue(parsed.diagnostics.isNotEmpty())
            assertEquals(source, parsed.tokens.joinToString("") { source.substring(it.start, it.end) })
        }
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
    fun `editor lexer exposes distinct brace pairs and expands empty parameters`() {
        val source = "() @{name=<value>} @[<value>] [<value>] %[<value>]"
        val lexer = HixEditorLexer(null)
        lexer.start(source, 0, source.length, 0)
        val tokens = buildList {
            while (lexer.tokenType != null) {
                add(Triple(lexer.tokenType, lexer.tokenStart, lexer.tokenEnd))
                lexer.advance()
            }
        }

        assertEquals(HelixAntlrTypes.tokens[HixLexer.BEGIN_PARAMETERS], tokens[0].first)
        assertEquals(HelixAntlrTypes.tokens[HixLexer.END_PARAMETERS], tokens[1].first)
        assertEquals("(", source.substring(tokens[0].second, tokens[0].third))
        assertEquals(")", source.substring(tokens[1].second, tokens[1].third))
        assertTrue(tokens.any { it.first == HixEditorTokenTypes.TABLE_END })
        assertTrue(tokens.any { it.first == HixEditorTokenTypes.TUPLE_END })
        assertTrue(tokens.any { it.first == HixEditorTokenTypes.INLINE_END })
        assertTrue(tokens.any { it.first == HixEditorTokenTypes.METADATA_VALUE_END })
    }

    @Test
    fun `brace matcher has one closing token identity per hix construct`() {
        val pairs = HixBraceMatcher().pairs
        assertEquals(pairs.size, pairs.map { it.rightBraceType }.distinct().size)
        assertTrue(pairs.any { it.leftBraceType == HelixAntlrTypes.tokens[HixLexer.BEGIN_ARGUMENT] }.not())
    }

    @Test
    fun `legacy syntax is rejected`() {
        assertTrue(HixAntlrSyntax.parse("@FUNC<Build>\n@END").diagnostics.isNotEmpty())
    }

    @Test
    fun `editor recovery keeps declarations after malformed lines`() {
        val source = "type Before = string\nthis is not a declaration\ntype After = number"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isNotEmpty())
        assertEquals(setOf("Before", "After"), HixAntlrSyntax.declarationNames(parsed.tree)
            .map { it.text }.toSet())
    }

    @Test
    fun `unclosed argument does not leave the rest of the editor in string mode`() {
        val source = "type Before = string\nfunc broken => <unfinished\ntype After = number"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isNotEmpty())
        assertTrue(HixAntlrSyntax.declarationNames(parsed.tree).any { it.text == "After" })
        val after = source.indexOf("After")
        assertTrue(parsed.tokens.any { it.start == after && it.type == HixLexer.IDENTIFIER })
    }

    @Test
    fun `unclosed value mode does not consume following declarations`() {
        val source = "type Before = string\nfunc broken(string value -> string\ntype After = number"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isNotEmpty())
        assertTrue(HixAntlrSyntax.declarationNames(parsed.tree).any { it.text == "After" })
    }

    @Test
    fun `incomplete header metadata does not taint the editor tree`() {
        val source = "%\n---\ntype After = number"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isNotEmpty())
        assertTrue(HixAntlrSyntax.declarationNames(parsed.tree).any { it.text == "After" })
    }

    @Test
    fun `analysis backend defaults to standalone and can be selected in file metadata`() {
        assertEquals("Standalone", HixSnapshotService.backendFor("mixin Example {}"))
        assertEquals("Unity", HixSnapshotService.backendFor("%backend<Unity>\n---\nmixin Example {}"))
        assertEquals("Standalone", HixSnapshotService.backendFor("---\n%backend<Unity>\nmixin Example {}"),
            "backend metadata is only read from the file header")
    }
}

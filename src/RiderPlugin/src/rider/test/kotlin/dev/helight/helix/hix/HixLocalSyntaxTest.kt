package dev.helight.helix.hix

import dev.helight.helix.hix.generated.HixLexer
import dev.helight.helix.hix.generated.HixParser
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue
import kotlin.test.assertSame

class HixLocalSyntaxTest {
    @Test
    fun `smart and inline values start statement chains`() {
        val parsed = HixAntlrSyntax.parse(
            "func main { local component = ${'$'}user:Named; " +
                "${'$'}user:Named:get<name>; []:myFunction; [<Ada>]:echo; " +
                "@{name = <Ada>}:get<name>; 42:echo; ${'$'}component = [<a>]; }"
        )
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        val offset = parsed.source.indexOf("[]:myFunction") + "[]:my".length
        assertTrue(HixLookup.site(parsed, offset).chained)
    }

    @Test
    fun `trait types compose fields and expose data constructors`() {
        val parsed = HixAntlrSyntax.parse(
            "trait type Named = @{string name}; type Entity = @{string id}; " +
                "type User = %inherit<Entity> %trait<Named> @{string email}; " +
                "compiler meta trait(pattern component) for type pattern; " +
                "func main { local user = User(id = <u>, email = <e>, \"${'$'}Named\" = @{name = <Ada>}); return(as(${'$'}user, Named)) }"
        )
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(listOf("Named", "Entity", "User"), HixLookup.constructors(parsed).map { it.name })
        assertEquals(listOf("id", "email", "Named"), HixLookup.constructors(parsed).last().fields.map { it.name })
        assertSame(HixColors.KEYWORD, HixSyntaxHighlighter(null)
            .getTokenHighlights(HelixAntlrTypes.tokens[HixLexer.KEYWORD_TRAIT]).single())
    }

    @Test
    fun `statement inline metadata and receiver first casts parse locally`() {
        val parsed = HixAntlrSyntax.parse(
            "type path.Record = @{string name}; pure func path.echo(string value) -> string => ${'$'}value; " +
                "func main() -> string { %inline local value = path.echo(<a>); " +
                "return(${'$'}value:as(string):path.echo()) }"
        )
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
    }

    @Test
    fun `metadata declarations accept retention modifiers`() {
        val parsed = HixAntlrSyntax.parse(
            "compiler meta erased(string text) for type;\n" +
                "bytecode meta transient(string text) for function;\n" +
                "runtime meta retained(string text) for function;\n" +
                "%synthetic pure func generated -> number => 1;"
        )
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(listOf("compiler", "bytecode", "runtime"),
            HixAntlrSyntax.rules(parsed.tree).filterIsInstance<HixParser.MetaModifierContext>().map { it.text }.toList())
        for (token in listOf(HixLexer.KEYWORD_COMPILER, HixLexer.KEYWORD_BYTECODE, HixLexer.KEYWORD_RUNTIME)) {
            assertSame(HixColors.KEYWORD,
                HixSyntaxHighlighter(null).getTokenHighlights(HelixAntlrTypes.tokens[token]).single())
        }
    }

    @Test
    fun `namespace identifiers work in declarations values and metadata`() {
        val parsed = HixAntlrSyntax.parse(
            "%using<path><p>\n---\nmeta docs.title(string text) for function;\n" +
                "type path.Text = string;\n%docs.title<Join>\n" +
                "pure func path.join(path.Text first) -> path.Text => ${'$'}first;\n" +
                "pure func main -> string { local value = <a> return(${'$'}value:p.join()) }"
        )
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
    }

    @Test
    fun `nested callable patterns in external function parameters parse locally`() {
        val parsed = HixAntlrSyntax.parse(
            "%generic<T> func spawn(delegate() -> T callback) -> %task<T> any external hostSpawn;\n" +
                "pure func apply(delegate(number value) -> delegate() -> string callback) -> function external hostApply;"
        )
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(3, HixAntlrSyntax.rules(parsed.tree).count { it is HixParser.DelegatePatternContext })
    }

    @Test
    fun `decompiled inline blocks and explicit dispatch parse locally`() {
        val source = "pure func main { local result = [do { local value = 1 } => local#value]; " +
            "@= [plus(1, 2) @static(\"plus(number 0, number 1) -> number\")]; " +
            "%inline return(call(main)) }"
        val parsed = HixAntlrSyntax.parse(source)
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(1, HixAntlrSyntax.rules(parsed.tree).count { it is HixParser.InlineBlockValueContext })
        assertEquals(1, HixAntlrSyntax.rules(parsed.tree).count { it is HixParser.CallBindingContext })
        assertSame(HixColors.KEYWORD,
            HixSyntaxHighlighter(null).getTokenHighlights(HelixAntlrTypes.tokens[HixLexer.BEGIN_INLINE_BLOCK]).single())
    }

    @Test
    fun `table and tuple declarations expose constructor fields`() {
        val parsed = HixAntlrSyntax.parse(
            "type Person = @{string name, %optional number age, string title = <friend>}\n" +
                "type Pair = @[string, number]\ntype Alias = Person"
        )

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        val constructors = HixLookup.constructors(parsed).associateBy { it.name }
        assertEquals(listOf("name", "age", "title"), constructors.getValue("Person").fields.map { it.name })
        assertTrue(constructors.getValue("Person").fields[1].optional)
        assertTrue(constructors.getValue("Person").fields[2].defaulted)
        assertEquals(listOf("0", "1"), constructors.getValue("Pair").fields.map { it.name })
        assertTrue(constructors.getValue("Pair").tuple)
        assertEquals(constructors.getValue("Person").fields, constructors.getValue("Alias").fields)
    }

    @Test
    fun `constructor call lookup reports assigned named fields`() {
        val source = "type Person = @{string name, number age}\n" +
            "pure func build => Person(age = 4, name = <Ada>);"
        val parsed = HixAntlrSyntax.parse(source)
        val offset = source.indexOf("name =")

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(HixLookup.ConstructorCall("Person", setOf("age", "name")),
            HixLookup.constructorCallAt(parsed, offset))
    }

    @Test
    fun `hal edits do not invalidate hix analysis`() {
        assertTrue(HixSnapshotService.affectsHixAnalysis("/project/manifest.hal"))
        assertTrue(!HixSnapshotService.affectsHixAnalysis("/project/assets/person.hal"))
    }

    @Test
    fun `canonical source-generator files and standalone hix files use the hix file type`() {
        assertEquals("hix", HixFileType.defaultExtension)
        assertTrue(!HixFileType.isCanonical("manifest.hal"))
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
    fun `nullable patterns parse as structural question suffixes`() {
        val source = "type Contact = @{string? name, %optional number? age}\n" +
            "pure func describe(Contact? contact) -> string | null? { return(null) }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(4, parsed.tokens.count { it.type == HixLexer.QUESTION })
        assertEquals(1, parsed.tokens.count { it.type == HixLexer.PIPE })
        assertEquals(4, HixAntlrSyntax.rules(parsed.tree).sumOf {
            if (it is HixParser.PatternTermContext && it.QUESTION() != null) 1 else 0
        })
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
    fun `quoted strings are atomic value literals in both lexer modes`() {
        val source = "pure func values => @[\"top\\nlevel\", identity(\"value \\\"literal\\\"\")] ;"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(2, parsed.tokens.count { it.type == HixLexer.STRING })
        assertTrue(parsed.tokens.none { it.type == HixLexer.BEGIN_VALUE_INTERPOLATE })
        assertTrue(HixSyntaxHighlighter(null)
            .getTokenHighlights(HelixAntlrTypes.tokens[HixLexer.STRING]).isNotEmpty())
    }

    @Test
    fun `quoted strings cannot form argument chains`() {
        val parsed = HixAntlrSyntax.parse("pure func invalid => \"literal\"<argument>;")

        assertTrue(parsed.diagnostics.isNotEmpty())
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
    fun `missing is a literal and pattern kind`() {
        val source = "pure func absent (missing value) -> bool { return(\$value:eq(missing)) }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.toString())
        assertEquals(2, parsed.tokens.count { it.type == HixLexer.MISSING })
        assertTrue(HixSyntaxHighlighter(null)
            .getTokenHighlights(HelixAntlrTypes.tokens[HixLexer.MISSING]).isNotEmpty())
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
        assertEquals("Test", HixSnapshotService.backendFor("%test\nfunc example {}", "Test"))
        assertEquals("Unity", HixSnapshotService.backendFor("%backend<Unity>\n---\nmixin Example {}"))
        assertEquals("Standalone", HixSnapshotService.backendFor("---\n%backend<Unity>\nmixin Example {}"),
            "backend metadata is only read from the file header")
    }
}

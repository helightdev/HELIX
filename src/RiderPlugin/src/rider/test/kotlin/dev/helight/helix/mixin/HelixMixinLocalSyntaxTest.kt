package dev.helight.helix.mixin

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

class HelixMixinLocalSyntaxTest {
    @Test
    fun `lexer keeps directive expression and opaque text distinct`() {
        val source = "@MIXIN<\$PostConstruct><0> global::UnityEngine.Type(@target:name)\n"
        val tokens = HelixMixinLocalLexer.lex(source)

        assertEquals("Directive", tokens.first().kind)
        assertEquals(listOf("DirectiveArgumentDelimiter", "DirectiveArgumentDelimiter",
            "DirectiveArgumentDelimiter", "DirectiveArgumentDelimiter"),
            tokens.filter { it.kind == "DirectiveArgumentDelimiter" }.map { it.kind })
        assertTrue(tokens.any { it.kind == "Value" && source.substring(it.start, it.end) == "@target" })
        assertTrue(tokens.none { it.kind == "Path" && source.substring(it.start, it.end).contains("UnityEngine") })
    }

    @Test
    fun `parser remains structural for incomplete directive arguments`() {
        val source = "@RETURN @table:put<name><(@local#Name)>\n@END\n"
        val root = HelixMixinLocalParser.parse(source)
        val first = root.children.first()

        assertEquals("Directive", first.kind)
        assertTrue(first.children.any { it.kind == "DirectiveName" })
        assertTrue(first.children.any { it.kind == "Operand" })
        assertTrue(flatten(first).any { it.kind == "FunctionCall" })
        assertTrue(flatten(first).any { it.kind == "Member" })
    }

    @Test
    fun `parenthesized references own roots calls and closing parenthesis`() {
        val source = "@RETURN @(var#CompanionName:format<(@local#Name)>)\n"
        val reference = flatten(HelixMixinLocalParser.parse(source))
            .first { it.kind == "ParenthesizedReference" }

        assertEquals("@(var#CompanionName:format<(@local#Name)>)", source.substring(reference.start, reference.end))
        assertEquals("var", source.substring(reference.children.first { it.kind == "Root" }.start,
            reference.children.first { it.kind == "Root" }.end))
        assertEquals("CompanionName", source.substring(reference.children.first { it.kind == "Member" }.start,
            reference.children.first { it.kind == "Member" }.end))
        val call = flatten(HelixMixinLocalParser.parse(source)).first { it.kind == "FunctionCall" }
        assertEquals(":format<(@local#Name)>", source.substring(call.start, call.end))
        assertTrue(call.children.single().children.any { it.kind == "Reference" })
    }

    @Test
    fun `multiline table expression retains nested argument structure`() {
        val source = "@RETURN @table:put<assignment><(this.@local#Name = @local#Name;)>\n"
        val nodes = flatten(HelixMixinLocalParser.parse(source)).toList()

        assertEquals(2, nodes.count { it.kind == "LiteralArgument" || it.kind == "ExpressionArgument" })
        assertEquals(2, nodes.count { it.kind == "Reference" && source.substring(it.start, it.end) == "@local#Name" })
    }

    @Test
    fun `declaration and reference directive roles are available without backend`() {
        val source = "@FUNC<Build>\n@CALL<Build> value\n"
        val nodes = flatten(HelixMixinLocalParser.parse(source)).toList()

        assertTrue(nodes.any { it.kind == "DeclarationDirectiveArgument" })
        assertTrue(nodes.any { it.kind == "ReferenceDirectiveArgument" })
    }

    @Test
    fun `completion contexts are available synchronously while typing`() {
        val directive = HelixMixinLocalCompletionParser.at("  @SCO", 6)!!
        val function = HelixMixinLocalCompletionParser.at("@RETURN @value:for", 18)!!
        val root = HelixMixinLocalCompletionParser.at("@RETURN @loc", 12)!!

        assertEquals("Directive", directive.kind)
        assertEquals("SCO", "  @SCO".substring(directive.replacementStart, directive.replacementEnd))
        assertEquals("Function", function.kind)
        assertEquals("Root", root.kind)
    }

    @Test
    fun `symbol and branch completion contexts come from local syntax`() {
        val local = HelixMixinLocalCompletionParser.at("@RETURN @local#Com", 18)!!
        val label = HelixMixinLocalCompletionParser.at("@GOTO<Gen", 9)!!
        val callback = HelixMixinLocalCompletionParser.at("@CALL<Bui", 9)!!

        assertEquals("Local", local.kind)
        assertEquals("Com", "@RETURN @local#Com".substring(local.replacementStart, local.replacementEnd))
        assertEquals("Label", label.kind)
        assertEquals("DeclaredFunction", callback.kind)
    }

    @Test
    fun `logical continuations map nested syntax back to physical lines`() {
        val source = "@RETURN @table:put<name>\n@+<(@local#Name)>\n"
        val nodes = flatten(HelixMixinLocalParser.parse(source)).toList()
        val member = nodes.single { it.kind == "Member" }

        assertEquals("Name", source.substring(member.start, member.end))
        assertTrue(nodes.any { it.kind == "Continuation" && source.substring(it.start, it.end) == "@+" })
    }

    @Test
    fun `compiler expression ranges match the shared syntax range fixture`() {
        val source = "@(local#Value:replace<Old><(@param#value)>)"
        val reference = HelixExpressionParser.parseExpressionValues(source).single().reference!!
        val property = reference.properties.single()

        assertEquals("local", source.substring(reference.rootRange.start, reference.rootRange.end))
        assertEquals("Value", source.substring(reference.memberRange.start, reference.memberRange.end))
        assertEquals("replace", source.substring(property.nameRange.start, property.nameRange.end))
        assertEquals("<Old>", source.substring(property.parsedArguments[0].sourceRange.start,
            property.parsedArguments[0].sourceRange.end))
        assertEquals("<(@param#value)>", source.substring(property.parsedArguments[1].sourceRange.start,
            property.parsedArguments[1].sourceRange.end))
    }

    @Test
    fun `named null root uses the parser supplied range`() {
        val source = "@RETURN @null:eq<null>"
        val root = flatten(HelixMixinLocalParser.parse(source)).single { it.kind == "Root" }

        assertEquals("null", source.substring(root.start, root.end))
    }

    @Test
    fun `psi lexer classifies both parenthesized references and dynamic argument wrappers`() {
        val source = "@RETURN @(var#Name:put<(@local#Value)>)"
        val lexer = HelixMixinLexer(null)
        lexer.start(source, 0, source.length, 0)
        val parentheses = ArrayList<Pair<String, Any?>>()
        while (lexer.tokenType != null) {
            val text = source.substring(lexer.tokenStart, lexer.tokenEnd)
            if (text == "(" || text == ")") parentheses += text to lexer.tokenType
            lexer.advance()
        }

        assertEquals(4, parentheses.size)
        parentheses.forEach { (text, type) ->
            assertEquals(if (text == "(") HelixMixinTokenTypes.OPEN_PARENTHESIS
                else HelixMixinTokenTypes.CLOSE_PARENTHESIS, type)
        }
    }

    @Test
    fun `psi lexer keeps directive and function angle delimiters paired`() {
        val source = "@LOCAL<Name> @table:put<key><value>"
        val lexer = HelixMixinLexer(null)
        lexer.start(source, 0, source.length, 0)
        val angles = ArrayList<Pair<String, Any?>>()
        while (lexer.tokenType != null) {
            val text = source.substring(lexer.tokenStart, lexer.tokenEnd)
            if (text == "<" || text == ">") angles += text to lexer.tokenType
            lexer.advance()
        }

        assertEquals(6, angles.size)
        angles.forEach { (text, type) ->
            assertEquals(if (text == "<") HelixMixinTokenTypes.OPEN_ANGLE
                else HelixMixinTokenTypes.CLOSE_ANGLE, type)
        }
    }

    private fun flatten(node: HelixLocalNode): Sequence<HelixLocalNode> = sequence {
        yield(node)
        node.children.forEach { yieldAll(flatten(it)) }
    }
}

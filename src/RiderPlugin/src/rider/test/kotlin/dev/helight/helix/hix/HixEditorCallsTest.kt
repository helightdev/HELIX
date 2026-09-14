package dev.helight.helix.hix

import dev.helight.helix.protocol.MixinLanguageDefinition
import kotlin.test.*

class HixEditorCallsTest {
    @Test
    fun `open parameter popup updates inferred generic arguments and return types`() {
        val template = HixEditorSignature("identity", listOf(HixEditorParameter("value", "T")), "T", bounds = mapOf("T" to "any"))
        val initial = template.copy(parameters = listOf(HixEditorParameter("value", "number")), result = "number")
        val item = HixParameterInfoItem(initial)
        val resolved = template.copy(parameters = listOf(HixEditorParameter("value", "string")), result = "string")
        item.update(HixEditorCall("identity", 0, 1, 0, emptyList(), resolved = resolved))
        assertEquals("string", item.signature.parameters.single().type)
        assertEquals("string", item.signature.result)
        assertTrue(resolved.instantiates(template))
        assertFalse(resolved.instantiates(template.copy(declaredTypes = listOf("number"))))
    }

    @Test
    fun `nested calls and named arguments select and highlight the correct parameter`() {
        val source = "func main { local x = choose(inner(@[1, 2]), fallback = <Ada>) }"
        val calls = HixEditorCalls.hix(source)
        val inner = HixEditorCalls.at(calls, source.indexOf("2]"))!!
        assertEquals("inner", inner.name)
        val outer = HixEditorCalls.at(calls, source.indexOf("Ada"))!!
        assertEquals("choose", outer.name)
        assertEquals(1, outer.currentArgument(source.indexOf("Ada")))
        val signature = HixEditorSignature("choose", listOf(HixEditorParameter("value", "any"),
            HixEditorParameter("fallback", "string", true, "<default>")), "string")
        val presentation = HixParameterInfoHandler.presentation(signature, outer, 1, true)
        assertEquals("fallback: string = <default>", presentation.text.substring(presentation.start, presentation.end))
        assertEquals(listOf("value"), HixParameterHintsProvider.hints(outer, listOf(signature)).map { it.first })
        assertNull(HixEditorCalls.at(calls, source.indexOf(") }") + 1))
    }

    @Test
    fun `parameter information survives incomplete arguments and trailing commas`() {
        val source = "func main { local x = choose(1, "
        val call = HixEditorCalls.at(HixEditorCalls.hix(source), source.length)!!
        assertEquals("choose", call.name)
        assertEquals(1, call.currentArgument(source.length))
        assertEquals(source.indexOf('('), call.anchor)
    }

    @Test
    fun `chain receivers do not consume explicit argument hints`() {
        val source = "func main { local x = ${'$'}item:lookup(<key>, fallback = 0) }"
        val call = HixEditorCalls.hix(source).single { it.name == "lookup" }
        assertTrue(call.chained)
        val signature = HixEditorSignature("lookup", listOf(HixEditorParameter("value", "table"),
            HixEditorParameter("key", "string"), HixEditorParameter("fallback", "any", true, "missing")), "any")
        assertEquals(1, signature.parameter(call, 0))
        assertEquals(2, signature.parameter(call, 1))
        assertEquals(listOf("key"), HixParameterHintsProvider.hints(call, listOf(signature)).map { it.first })
        val presentation = HixParameterInfoHandler.presentation(signature, call, 0, false)
        assertEquals("key: string", presentation.text.substring(presentation.start, presentation.end))
    }

    @Test
    fun `overload ambiguity and variadic calls do not produce misleading labels`() {
        val args = listOf(HixEditorArgument(1, 2, text = "1"), HixEditorArgument(3, 4, text = "2"))
        val call = HixEditorCall("sum", 0, 5, 0, args)
        val one = HixEditorSignature("sum", listOf(HixEditorParameter("values", "number", variadic = true)), "number")
        assertEquals(listOf("values" to 1), HixParameterHintsProvider.hints(call, listOf(one)))
        assertEquals(emptyList(), HixParameterHintsProvider.hints(call, listOf(one, one.copy(parameters =
            listOf(HixEditorParameter("items", "number", variadic = true))))))
        assertEquals(0, one.parameter(call, 10))
    }

    @Test
    fun `compiler catalog preserves parameter names generic bounds and default values`() {
        val definition = MixinLanguageDefinition("convert", "Function", 2, false, "", "T", "T",
            arrayOf("T", "bool"), "description", arrayOf("value", "strict"), booleanArrayOf(false, true),
            arrayOf("", "false"), arrayOf("T"), arrayOf("number"), arrayOf("T", "bool"))
        val signature = HixEditorCalls.from(definition)
        assertEquals("convert(value: T, strict: bool = false) -> T", signature.display)
        assertEquals(mapOf("T" to "number"), signature.bounds)
    }

    @Test
    fun `constructors inherit defaults and expose trait argument aliases`() {
        val source = "trait type Named = @{string name}; type Base = @{string id = <default>}; " +
            "type User = %inherit<Base> %trait<Named> @{number age};"
        val user = HixEditorCalls.local(source).single { it.name == "User" }
        assertEquals(listOf("id", "age", "Named"), user.parameters.map { it.name })
        assertEquals("<default>", user.parameters.first().default)
        assertEquals(2, user.parameter(HixEditorCall("User", 0, 1, 0,
            listOf(HixEditorArgument(0, 1, name = "${'$'}Named"))), 0))
    }

    @Test
    fun `function and variable completion respects lexical scope and declaration order`() {
        val source = "func before(number param) -> any { local outer = 1; when(true) { local inner = 2; } " +
            "local result = echo(1); local later = 3; } func other { local foreign = 4; }"
        val offset = source.indexOf("echo")
        assertEquals(setOf("param", "outer"), HixCompletionScope.variables(HixAntlrSyntax.parseIncomplete(source), offset, null).map { it.name }.toSet())
        val mixins = "mixin A { func own => <A>; expression { emit(1) } } mixin B { func foreign => <B>; }"
        assertEquals(listOf("own"), HixEditorCalls.local(mixins, mixins.indexOf("emit")).map { it.name })
    }

    @Test
    fun `ranking separates exact generic unknown convertible and impossible types`() {
        val match = HixCompletionCompatibility::match
        assertEquals(HixCompletionCompatibility.Match.Exact, match("number", "number", emptyMap()))
        assertEquals(HixCompletionCompatibility.Match.Compatible, match("number", "T", mapOf("T" to "any")))
        assertEquals(HixCompletionCompatibility.Match.Unknown, match("Undeclared", "number", emptyMap()))
        assertEquals(HixCompletionCompatibility.Match.Conversion, match("number", "string", emptyMap()))
        assertEquals(HixCompletionCompatibility.Match.Impossible, match("number", "opaque", emptyMap()))
        assertEquals(HixCompletionCompatibility.Match.Impossible, match("String", "string", mapOf("String" to "table")))
        assertEquals(HixCompletionCompatibility.Match.Compatible, match("null", "string?", emptyMap()))
    }
}

package dev.helight.helix.hal

import dev.helight.helix.hix.HixEditorCalls
import kotlin.test.*

class HalEditorInsightTest {
    private val schema = "type User = @{string name, %optional number age}; type Other = @{number count}; type Pair = @[number a, number b];"
    @Test
    fun `reference call tracks named parameters and incomplete input`() {
        val source = "--- User &user\nname = Ada\nage = ref(section = user)\n"
        val call = HixEditorCalls.hal(source).single()
        assertEquals("ref", call.name)
        assertEquals("section", call.arguments.single().name)
        assertEquals("user", call.arguments.single().text)
        val incomplete = "--- User\nname = ref("
        assertEquals("ref", HixEditorCalls.at(HixEditorCalls.hal(incomplete), incomplete.length)?.name)
    }
    @Test
    fun `field hints use schema types without inventing unknown fields`() {
        val source = "--- User\nname = Ada\nage = 30\nunknown = x\n"
        val hints = HalInlayHintsProvider.fieldHints(source, schema)
        assertEquals(listOf("string", "number"), hints.map { it.second })
        assertEquals(listOf(source.indexOf("name") + 4, source.indexOf("age") + 3), hints.map { it.first })
    }
    @Test
    fun `completion ranks required fields above optional fields and omits assigned fields`() {
        val source = "--- User\nna"
        val items = HalCompletion.items(source, source.length, schema, false)
        assertEquals(setOf("name", "age"), items.map { it.name }.toSet())
        assertTrue(items.single { it.name == "name" }.priority > items.single { it.name == "age" }.priority)
        val assigned = "--- User\nname = Ada\n"
        assertEquals(listOf("age"), HalCompletion.items(assigned, assigned.length, schema, false).map { it.name })
    }
    @Test
    fun `header and reference completion do not offer unrelated fields or unsupported functions`() {
        val header = "--- U"
        assertEquals(setOf("table", "tuple", "User", "Other", "Pair"), HalCompletion.items(header, header.length, schema, false).map { it.name }.toSet())
        val reference = "--- User &user\nname = Ada\nage = ref(u)\n--- Other &other\ncount = 1\n"
        val items = HalCompletion.items(reference, reference.indexOf("ref(u") + 5, schema, false)
        assertEquals(setOf("user", "other"), items.map { it.name }.toSet())
        assertTrue(items.all { it.kind == "Reference" })
        val comment = "--- User\n// na"
        assertTrue(HalCompletion.items(comment, comment.length, schema, false).isEmpty())
        val scalar = "--- User\nname = "
        assertFalse(HalCompletion.items(scalar, scalar.length, schema, false).any { it.kind == "Constructor" })
    }
    @Test
    fun `many union lists suggest element constructors without outer fields`() {
        val schema = "type Pet = %union<Cat><Dog>; %tagged type Cat = @{string name}; %tagged type Dog = @{string name}; type House = @{%optional string tag, %many<Pet> pets};"
        for (tail in listOf("\n  ", "\n  C", "\n  Cat { name = Miso }\n  ")) {
            val source = "--- House\npets = [" + tail
            val items = HalCompletion.items(source, source.length, schema, false)
            assertEquals(setOf("Cat", "Dog"), items.filter { it.kind == "Constructor" }.map { it.name }.toSet())
            assertFalse(items.any { it.kind == "Field" })
        }
        val source = "--- House\npets = [\n Cat {\n  "
        assertEquals(listOf("name"), HalCompletion.items(source, source.length, schema, false).map { it.name })
    }
    @Test
    fun `empty space inside a closed many list offers union members`() {
        val schema = "type Pet = %union<Cat><Dog>; %tagged type Cat = @{string name}; %tagged type Dog = @{string name}; type House = @{%optional string tag, %many Pet pets};"
        for (body in listOf("pets = [\n  |\n]", "pets [\n  |\n]", "pets = [ | ]")) {
            val marked = "--- House\n" + body + "\ntag = home\n"
            val offset = marked.indexOf('|')
            val source = marked.replace("|", "")
            val items = HalCompletion.items(source, offset, schema, false)
            assertEquals(setOf("Cat", "Dog"), items.filter { it.kind == "Constructor" }.map { it.name }.toSet(), body + items)
            assertFalse(items.any { it.kind == "Field" })
        }
    }
}

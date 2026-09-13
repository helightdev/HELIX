package dev.helight.helix.hal

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class HalLanguageTest {
    @Test
    fun `trait component fields and inherited fields are available to hal`() {
        val source = "trait type Named = @{string name}; type Entity = @{string id}; " +
            "type User = %inherit<Entity> %trait<Named> @{string email};"
        assertEquals(listOf("Entity", "User"), HalManifest.types(source))
        assertEquals(listOf("id", "email", "${'$'}Named"), HalManifest.fields(source, "User"))
        assertEquals(listOf("name"), HalManifest.fields(source, "Named"))
    }

    @Test
    fun `rider exposes the manifest project schema`() {
        assertEquals(
            listOf("name", "version", "description", "repository", "license", "author",
                "applicationMain", "sources", "dependencies"),
            HalManifest.manifestFields.map { it.name },
        )
        assertTrue(HalManifest.manifestFields.single { it.name == "name" }.required)
        assertEquals(listOf("main", "test", "modules"), HalManifest.sourceFields.map { it.name })
        assertEquals(listOf("uri", "version"), HalManifest.dependencyFields("PathDependency").map { it.name })
        assertEquals(
            listOf("repository", "branch", "rev", "tag", "version"),
            HalManifest.dependencyFields("GitDependency").map { it.name },
        )
        assertEquals(listOf("version"), HalManifest.dependencyFields("ModuleDependency").map { it.name })
    }

    @Test
    fun `lexer recognizes sections typed containers references and comments`() {
        val source = """
            %uid<example>
            --- Material
            color = Color { r = 1 }
            values = Numbers [1, 2]
            source = ref(other)#value
            // comment
        """.trimIndent()
        val lexer = HalLexer(); lexer.start(source)
        val tokens = buildList {
            while (lexer.tokenType != null) { add(lexer.tokenType.toString() to source.substring(lexer.tokenStart, lexer.tokenEnd)); lexer.advance() }
        }
        assertTrue(tokens.any { it.first.contains("SECTION") && it.second == "---" })
        assertTrue(tokens.any { it.first.contains("HASH") })
        assertTrue(tokens.any { it.first.contains("COMMENT") })
        assertTrue(tokens.any { it.first.contains("PERCENT") && it.second == "%" })
        assertFalse(tokens.any { it.first.contains("ERROR_TOKEN") })
    }

    @Test
    fun `hal file type is registered for hal extension`() {
        assertEquals("hal", HalFileType.defaultExtension)
        assertEquals("Hix Asset Language", HalFileType.description)
    }

    @Test
    fun `manifest types are discovered after inline metadata`() {
        val manifest = """
            %backend<Standalone>
            ---

            %tagged type Person = @{
              string name,
              number age,
              bool married = [false],
              %optional string tag
            }
        """.trimIndent()
        assertEquals(listOf("Person"), HalManifest.types(manifest))
        assertEquals(listOf("name", "age", "married", "tag"), HalManifest.fields(manifest, "Person"))
        assertEquals(
            listOf(
                HalManifest.Field("string", "name", true),
                HalManifest.Field("number", "age", true),
                HalManifest.Field("bool", "married", false),
                HalManifest.Field("string", "tag", false)
            ),
            HalManifest.fieldDefinitions(manifest, "Person")
        )
    }

    @Test
    fun `manifest collection fields retain their element pattern`() {
        val manifest = """
            type Person = @{string name}
            type People = %many<Person>
            type House = @{%many<Person> inhabitants, People residents}
        """.trimIndent()
        assertEquals(
            listOf(
                HalManifest.Field("Person", "inhabitants", true, true),
                HalManifest.Field("People", "residents", true)
            ),
            HalManifest.fieldDefinitions(manifest, "House")
        )
        assertEquals("Person", HalManifest.manyElement(manifest, "People"))
    }

    @Test
    fun `manifest unions retain their member patterns`() {
        val manifest = """
            type Pet = %union<Dog><Cat>;
            %tagged type Dog = @{string name};
            %tagged type Cat = @{string name};
        """.trimIndent()
        assertEquals(listOf("Dog", "Cat"), HalManifest.unionMembers(manifest, "Pet"))
        assertEquals(listOf("Dog", "Cat"), HalManifest.types(manifest).filter { it != "Pet" })
    }

    @Test
    fun `section assignments retain duplicates but exclude nested fields`() {
        val source = """
            --- Person
            name = Alex
            name = 123
            transform = Transform {
              x = 1
            }
            married = true
        """.trimIndent()
        val section = HalStructure.sections(source).single()
        assertEquals("Person", section.type)
        assertEquals(
            listOf("name", "name", "transform", "married"),
            HalStructure.assignments(source, section).map(HalAssignment::name)
        )
    }

    @Test
    fun `antlr tree contains nested typed table and list rules`() {
        val parsed = HalAntlrSyntax.parse("""
            --- House
            owner Person { "name" = Ada, }
            inhabitants People [
              Person { name = Grace, },
              Person { name = Linus }
            ],
        """.trimIndent())
        assertTrue(parsed.diagnostics.isEmpty(), parsed.diagnostics.joinToString { it.message })
        val section = parsed.tree.sectionBlock().single().section()
        assertEquals(2, section.sectionEntry().size)
        assertEquals("Person", section.sectionEntry(0).field().collectionValue().IDENTIFIER().text)
        assertEquals("name", fieldName(section.sectionEntry(0).field().collectionValue().typedContainer().table()
            .tableEntry(0).fieldKey()))
        assertEquals(2, section.sectionEntry(1).field().collectionValue().typedContainer().list().value().size)
        assertEquals("Person", HalStructure.completionScope(parsed.source,
            parsed.source.indexOf("name = Grace") + 2)?.first)
    }
}

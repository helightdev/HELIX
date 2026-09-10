package dev.helight.helix.hix

import dev.helight.helix.protocol.MixinLanguageDefinition
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class HixLookupTest {
    @Test
    fun `semantic roles distinguish kinds and pattern metadata from functions`() {
        val source = "type Values = %many string"
        val parsed = HixAntlrSyntax.parse(source)

        assertEquals(HixLookup.SemanticRole.PatternMetadata, HixLookup.semanticRoleAt(parsed, source.indexOf("many")))
        assertEquals(HixLookup.SemanticRole.Pattern, HixLookup.semanticRoleAt(parsed, source.indexOf("string")))
        val argumentSource = "type Values = %many<DerivedPropertyEntry>"
        val argument = HixLookup.metadataArgumentAt(HixAntlrSyntax.parse(argumentSource),
            argumentSource.indexOf("DerivedPropertyEntry"))
        assertEquals(HixLookup.MetadataArgument("many", 0), argument)
        val incomplete = "type Values = %"
        assertEquals(HixLookup.SemanticRole.PatternMetadata,
            HixLookup.semanticRoleAt(HixAntlrSyntax.parse(incomplete), incomplete.lastIndex))
        val fieldSource = "type Values = @{%optional string value}"
        assertEquals("Field", HixLookup.patternMetadataTargetAt(HixAntlrSyntax.parse(fieldSource),
            fieldSource.indexOf("optional")))
        assertEquals("Pattern", HixLookup.patternMetadataTargetAt(parsed, source.indexOf("many")))
        val header = "%pragma<PROFILE>\n%vm<PROFILE>\n---\nmixin Test { }"
        val headerParse = HixAntlrSyntax.parse(header)
        assertEquals(HixLookup.SemanticRole.FileMetadata,
            HixLookup.semanticRoleAt(headerParse, header.indexOf("pragma")))
        assertEquals(HixLookup.SemanticRole.FileMetadata,
            HixLookup.semanticRoleAt(headerParse, header.indexOf("vm")))
        val incompleteHeader = "%\n---\nmixin Test { }"
        assertEquals(HixLookup.SemanticRole.FileMetadata,
            HixLookup.semanticRoleAt(HixAntlrSyntax.parse(incompleteHeader), 0))
        val partiallyTypedHeader = "%back\n---\nmixin Test { }"
        assertEquals(HixLookup.SemanticRole.FileMetadata,
            HixLookup.semanticRoleAt(HixAntlrSyntax.parse(partiallyTypedHeader),
                partiallyTypedHeader.indexOf("back") + 3))
    }

    @Test
    fun `statement and value completion contexts remain distinct`() {
        val source = "mixin Test { expression { emit(local#value); missing } }"
        val parsed = HixAntlrSyntax.parse(source)

        assertTrue(HixLookup.isValueContextAt(parsed, source.indexOf("local")))
        assertTrue(HixLookup.isStatementBlockAt(parsed, source.indexOf("missing")))
        assertFalse(HixLookup.isValueContextAt(parsed, source.indexOf("missing")))
    }

    private fun method(receiver: String) = MixinLanguageDefinition("method", "Function", 1, false,
        "", receiver, "String", arrayOf(receiver), "Returns the selected value's name.")

    @Test
    fun `chained lookup uses canonical operators and literal receiver types`() {
        val source = "mixin Test { expression { emit(42:na) } }"
        val offset = source.indexOf(":na") + 3
        val parsed = HixAntlrSyntax.parse(source)
        val site = HixLookup.site(parsed, offset)
        assertTrue(site.chained)
        assertFalse(site.member)
        assertEquals("number", HixLookup.receiverType(parsed, site.receiverEnd, null))
        assertFalse(HixLookup.acceptsReceiver(method("Symbol"), "number"))
        assertTrue(HixLookup.acceptsReceiver(method("Number"), "number"))
        assertTrue(HixLookup.acceptsReceiver(method("Any"), "number"))
    }

    @Test
    fun `member completion is distinct from method completion`() {
        val source = "mixin Test { expression { emit(local#va) } }"
        val site = HixLookup.site(HixAntlrSyntax.parse(source), source.indexOf("#va") + 3)
        assertTrue(site.member)
        assertFalse(site.chained)
    }

    @Test
    fun `lookup retains individual signatures and documentation`() {
        val definition = method("Symbol")
        assertEquals("method(symbol) -> string", HixLookup.signature(definition))
        assertEquals("Returns the selected value's name.", definition.documentation)
        assertFalse(HixLookup.acceptsReceiver(definition.copy(argumentTypes = emptyArray()), "symbol"))
    }
}

package dev.helight.helix.hix

import dev.helight.helix.protocol.MixinLanguageDefinition
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class HixLookupTest {
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

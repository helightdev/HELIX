package dev.helight.helix.hix

import kotlin.test.*

class HixBracketEditingTest {
    @Test fun `enter recognizes all structural bracket pairs`() {
        for ((left, right) in listOf('[' to ']', '{' to '}', '(' to ')')) assertTrue(HixBracketEnterDelegate.paired(left, right))
        assertFalse(HixBracketEnterDelegate.paired('[', '}'))
    }
    @Test fun `backspace retains indentation of the previous empty line`() {
        for (close in listOf(']', '}', ')')) {
            val source = "  [\n    \n  $close"
            val offset = source.lastIndex
            val plan = assertNotNull(HixBracketBackspaceDelegate.join(source, offset))
            assertEquals("  [\n    $close", source.removeRange(plan.first, plan.second))
            assertEquals(8, plan.first)
        }
        assertNull(HixBracketBackspaceDelegate.join("[\n  value\n]", 10))
        assertNull(HixBracketBackspaceDelegate.join("[]", 1))
    }
}

package dev.helight.helix.hix

import kotlin.test.Test
import kotlin.test.assertEquals

class HixLineIndentProviderTest {
    private fun indent(source: String): String {
        val lines = source.lines()
        return HixLineIndentProvider.calculateIndent(lines, lines.lastIndex, 2)
    }

    @Test
    fun `indents a new line in a function block`() {
        assertEquals("  ", indent("func main {\n"))
        assertEquals("  ", indent("func main {\n  print<hello>\n"))
    }

    @Test
    fun `indents tables and chopped calls`() {
        assertEquals("  ", indent("type Person = @{\n"))
        assertEquals("    ", indent("func main {\n  local person = Person(\n"))
        assertEquals("    ", indent("func main {\n  local person = Person(\n    name = <test>,\n"))
    }

    @Test
    fun `content continuations retain their current indentation`() {
        assertEquals("  ", indent("func main {\n  print @> content\n"))
        assertEquals("    ", indent("func main {\n    @+ continued content\n"))
    }

    @Test
    fun `closing delimiters outdent`() {
        assertEquals("  ", indent("func main {\n  local person = Person(\n    name = <test>\n  )"))
        assertEquals("", indent("func main {\n  print<hello>\n}"))
    }
}

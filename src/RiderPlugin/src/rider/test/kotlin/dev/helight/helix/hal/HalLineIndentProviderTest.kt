package dev.helight.helix.hal

import kotlin.test.Test
import kotlin.test.assertEquals

class HalLineIndentProviderTest {
    private fun indent(source: String): String {
        val lines = source.lines()
        return HalLineIndentProvider.calculateIndent(lines, lines.lastIndex, 2)
    }

    @Test
    fun `indents table entries`() {
        assertEquals("  ", indent("sources = {\n"))
        assertEquals("  ", indent("sources = {\n  main = <src/**>\n"))
    }

    @Test
    fun `indents nested lists and tagged values`() {
        assertEquals("    ", indent("dependencies = {\n  alpha = PathDependency(\n"))
        assertEquals("    ", indent("sources = {\n  main = [\n"))
    }

    @Test
    fun `closing delimiters outdent`() {
        assertEquals("  ", indent("dependencies = {\n  alpha = PathDependency(\n    uri = <../alpha>\n  )"))
        assertEquals("", indent("sources = {\n  main = <src/**>\n}"))
    }

    @Test
    fun `blank lines retain surrounding indentation`() {
        assertEquals("  ", indent("sources = {\n  main = <src/**>\n\n"))
    }
}

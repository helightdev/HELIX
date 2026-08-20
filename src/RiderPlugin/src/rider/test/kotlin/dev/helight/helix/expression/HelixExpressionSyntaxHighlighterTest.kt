package dev.helight.helix.expression

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

class HelixExpressionSyntaxHighlighterTest {
    @Test
    fun `returns expected token highlights`() {
        val highlighter = HelixExpressionSyntaxHighlighter()

        assertEquals(listOf(HelixExpressionSyntaxHighlighter.DIRECTIVE), highlighter.getTokenHighlights(HelixExpressionTypes.DIRECTIVE).toList())
        assertEquals(listOf(HelixExpressionSyntaxHighlighter.VALUE), highlighter.getTokenHighlights(HelixExpressionTypes.VALUE).toList())
        assertEquals(listOf(HelixExpressionSyntaxHighlighter.PATH), highlighter.getTokenHighlights(HelixExpressionTypes.PATH).toList())
        assertEquals(listOf(HelixExpressionSyntaxHighlighter.FUNCTION), highlighter.getTokenHighlights(HelixExpressionTypes.FUNCTION).toList())
        assertEquals(listOf(HelixExpressionSyntaxHighlighter.ARGUMENT), highlighter.getTokenHighlights(HelixExpressionTypes.ARGUMENT).toList())
        assertEquals(listOf(HelixExpressionSyntaxHighlighter.OPERATOR), highlighter.getTokenHighlights(HelixExpressionTypes.PREDICATE).toList())
        assertEquals(listOf(HelixExpressionSyntaxHighlighter.OPERATOR), highlighter.getTokenHighlights(HelixExpressionTypes.NEGATED_PREDICATE).toList())
        assertEquals(listOf(HelixExpressionSyntaxHighlighter.OPERATOR), highlighter.getTokenHighlights(HelixExpressionTypes.INVOKE).toList())
        assertEquals(listOf(HelixExpressionSyntaxHighlighter.BRACES), highlighter.getTokenHighlights(HelixExpressionTypes.ENCLOSED_START).toList())
        assertEquals(listOf(HelixExpressionSyntaxHighlighter.BRACES), highlighter.getTokenHighlights(HelixExpressionTypes.ENCLOSED_END).toList())
        assertEquals(listOf(HelixExpressionSyntaxHighlighter.ESCAPE), highlighter.getTokenHighlights(HelixExpressionTypes.ESCAPED_AT).toList())
        assertEquals(listOf(HelixExpressionSyntaxHighlighter.BAD_CHARACTER), highlighter.getTokenHighlights(HelixExpressionTypes.BAD_CHARACTER).toList())
        assertTrue(highlighter.getTokenHighlights(HelixExpressionTypes.TEXT).isEmpty())
    }

    @Test
    fun `color settings page provides descriptors and highlighter`() {
        val page = HelixExpressionColorSettingsPage()
        assertEquals("HELIX Mixin Expression", page.displayName)
        assertTrue(page.attributeDescriptors.isNotEmpty())
        assertTrue(page.demoText.contains("@FUNC<MixinCallbackImpl>"))
    }
}

package dev.helight.helix.expression

import kotlin.test.Test
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class HelixExpressionCSharpInjectorTest {
    @Test
    fun `injects single expression overload`() {
        assertTrue(isInjected("[MixinExpression(\"@CALL<emit>\")]", "\"@CALL"))
    }

    @Test
    fun `injects only third argument of targeted overload`() {
        val source = "[HELIX.MixinExpression(\"\$Init\", 10, \"@CODE Run();\")]"
        assertFalse(isInjected(source, "\"\$Init"))
        assertTrue(isInjected(source, "\"@CODE"))
    }

    @Test
    fun `injects global prepared expressions`() {
        assertTrue(isInjected("[assembly: MixinPrepareGlobal(\"@FUNC<x>\\n@END\")]", "\"@FUNC"))
    }

    @Test
    fun `injects global prepared verbatim expressions`() {
        val source = """
            [assembly: HELIX.MixinPrepareGlobal(
              @"
            @FUNC<MixinCallbackImpl>
            @END
            "
            )]
        """.trimIndent()
        assertTrue(isInjected(source, "@\""))
    }

    @Test
    fun `does not inject unrelated strings`() {
        assertFalse(isInjected("var text = \"@CODE NotAnExpression();\";", "\"@CODE"))
    }

    private fun isInjected(source: String, literalPrefix: String): Boolean {
        val start = source.indexOf(literalPrefix)
        require(start >= 0)
        val end = source.indexOf('"', start + 1).let { if (it < 0) source.length else it + 1 }
        return HelixExpressionCSharpInjector.isMixinExpressionArgument(source, start, end)
    }
}

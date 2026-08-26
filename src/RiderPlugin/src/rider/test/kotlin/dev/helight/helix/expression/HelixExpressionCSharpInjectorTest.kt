package dev.helight.helix.expression

import com.intellij.openapi.util.TextRange
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNull

class HelixExpressionCSharpInjectorTest {
    @Test
    fun `converts Rider absolute content range to injection host offsets`() {
        assertEquals(TextRange(1, 20), toHostRelativeRange(TextRange(235, 254), 234))
    }

    @Test
    fun `recognizes supported mixin attribute spellings`() {
        assertEquals(HelixAttributeKind.EXPRESSION, helixAttributeKind("MixinExpression"))
        assertEquals(HelixAttributeKind.EXPRESSION, helixAttributeKind("MixinExpressionAttribute"))
        assertEquals(HelixAttributeKind.EXPRESSION, helixAttributeKind("HELIX.MixinExpression"))
        assertEquals(
            HelixAttributeKind.LIBRARY,
            helixAttributeKind("global::HELIX.MixinLibraryAttribute"),
        )
    }

    @Test
    fun `rejects similarly named attributes in other namespaces`() {
        assertNull(helixAttributeKind("Other.MixinExpression"))
        assertNull(helixAttributeKind("MixinExpressionFactory"))
    }
}

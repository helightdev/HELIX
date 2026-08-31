package dev.helight.helix.mixin

import com.intellij.psi.PsiReference
import kotlin.test.Test
import kotlin.test.assertTrue

class HelixMixinPsiReferenceTest {
    @Test
    fun `reference PSI owns its reference implementation`() {
        assertTrue(PsiReference::class.java.isAssignableFrom(HelixMixinReferenceElement::class.java))
        assertTrue(HelixMixinReferenceElement::class.java.declaredMethods.any {
            it.name == "resolve" && it.parameterCount == 0
        })
    }

    @Test
    fun `find usages relies on snapshot PSI rather than a second lexer`() {
        val provider = HelixMixinFindUsagesProvider()
        assertTrue(provider.wordsScanner.javaClass.simpleName == "SimpleWordsScanner")
        assertTrue(provider.javaClass.methods.any { it.name == "canFindUsagesFor" })
    }
}

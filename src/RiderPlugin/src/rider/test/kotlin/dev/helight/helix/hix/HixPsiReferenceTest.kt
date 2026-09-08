package dev.helight.helix.hix

import com.intellij.psi.PsiReference
import kotlin.test.Test
import kotlin.test.assertTrue

class HixPsiReferenceTest {
    @Test
    fun `reference PSI owns its reference implementation`() {
        assertTrue(PsiReference::class.java.isAssignableFrom(HixReferenceElement::class.java))
        assertTrue(HixReferenceElement::class.java.declaredMethods.any {
            it.name == "resolve" && it.parameterCount == 0
        })
    }

    @Test
    fun `find usages relies on snapshot PSI rather than a second lexer`() {
        val provider = HixFindUsagesProvider()
        assertTrue(provider.wordsScanner.javaClass.simpleName == "SimpleWordsScanner")
        assertTrue(provider.javaClass.methods.any { it.name == "canFindUsagesFor" })
    }
}

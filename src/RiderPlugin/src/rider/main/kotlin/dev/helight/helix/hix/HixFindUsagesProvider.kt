package dev.helight.helix.hix

import com.intellij.lang.cacheBuilder.WordsScanner
import com.intellij.lang.cacheBuilder.SimpleWordsScanner
import com.intellij.lang.findUsages.FindUsagesProvider
import com.intellij.psi.PsiElement

/**
 * Exposes snapshot-backed declarations to IntelliJ's standard navigation, usage, and rename
 * infrastructure. Reference discovery still comes from PSI reference nodes constructed from
 * backend ranges. The generic scanner only gives IntelliJ candidate words for its usage index;
 * it does not classify or parse mixin syntax.
 */
class HixFindUsagesProvider : FindUsagesProvider {
    override fun getWordsScanner(): WordsScanner = SimpleWordsScanner()

    override fun canFindUsagesFor(psiElement: PsiElement): Boolean =
        psiElement is HixDeclarationElement

    override fun getHelpId(psiElement: PsiElement): String? = null

    override fun getType(element: PsiElement): String =
        if (element is HixDeclarationElement) "mixin declaration" else "mixin symbol"

    override fun getDescriptiveName(element: PsiElement): String =
        (element as? HixDeclarationElement)?.name ?: element.text

    override fun getNodeText(element: PsiElement, useFullName: Boolean): String =
        getDescriptiveName(element)
}

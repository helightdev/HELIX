package dev.helight.helix.expression

import com.intellij.codeInsight.daemon.LineMarkerInfo
import com.intellij.codeInsight.daemon.LineMarkerProvider
import com.intellij.openapi.components.service
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiNameIdentifierOwner
import com.intellij.psi.util.PsiTreeUtil
import com.intellij.util.Function
import com.jetbrains.rider.languages.fileTypes.csharp.psi.CSharpDeclaration
import dev.helight.helix.HelixIcons

class HelixMixinContributionLineMarkerProvider : LineMarkerProvider {
    override fun getLineMarkerInfo(element: PsiElement): LineMarkerInfo<*>? {
        if (element.firstChild != null) return null
        val file = element.containingFile ?: return null
        val path = file.virtualFile?.path ?: return null
        if (!path.endsWith(".cs", ignoreCase = true)) return null
        val declaration = PsiTreeUtil.getParentOfType(element, CSharpDeclaration::class.java, false)
        val isDeclarationIdentifier = (declaration as? PsiNameIdentifierOwner)?.nameIdentifier === element
        val cache = element.project.service<HelixMixinContributionCache>()
        val contributions = buildList {
            addAll(cache.contributionsAt(path, element.textRange.startOffset))
            if (isDeclarationIdentifier) addAll(cache.contributionsFor(path, declaration))
        }.distinct()
        if (contributions.isEmpty()) return null
        val count = contributions.size
        val tooltip = if (count == 1) "1 mixin hook" else "$count mixin hooks"
        return LineMarkerInfo(
            element,
            element.textRange,
            HelixIcons.MixinContribution,
            Function { tooltip },
            { _, clickedElement ->
                HelixMixinContributionPopup.show(
                    clickedElement.project,
                    clickedElement,
                    contributions.first().target,
                    contributions,
                )
            },
            com.intellij.openapi.editor.markup.GutterIconRenderer.Alignment.LEFT,
            { tooltip },
        )
    }
}

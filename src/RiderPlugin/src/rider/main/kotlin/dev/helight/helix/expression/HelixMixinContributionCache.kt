package dev.helight.helix.expression

import com.intellij.codeInsight.daemon.DaemonCodeAnalyzer
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.Service
import com.intellij.openapi.project.Project
import com.intellij.psi.util.PsiTreeUtil
import com.jetbrains.rider.languages.fileTypes.csharp.psi.CSharpDeclaration
import com.jetbrains.rider.languages.fileTypes.csharp.psi.impl.CSharpParameterDeclaration
import dev.helight.helix.protocol.MixinContribution
import java.util.concurrent.ConcurrentHashMap

@Service(Service.Level.PROJECT)
class HelixMixinContributionCache(private val project: Project) {
    private val contributionsByFile = ConcurrentHashMap<String, Map<Int, List<MixinContribution>>>()

    fun contributionsAt(filePath: String, offset: Int): List<MixinContribution> =
        contributionsByFile[filePath]?.get(offset).orEmpty()

    fun contributionsFor(filePath: String, declaration: CSharpDeclaration): List<MixinContribution> {
        val name = declaration.declaredName ?: return emptyList()
        val className = declaration.javaClass.simpleName
        val parameterCount by lazy {
            PsiTreeUtil.findChildrenOfType(declaration, CSharpParameterDeclaration::class.java)
                .count { parameter ->
                    PsiTreeUtil.getParentOfType(parameter, CSharpDeclaration::class.java, true) === declaration
                }
        }
        return contributionsByFile[filePath].orEmpty().values.asSequence().flatten()
            .filter { contribution ->
                val sourceTypeName = contribution.sourceType
                    .substringAfterLast('.')
                    .substringAfterLast('+')
                    .substringBefore('<')
                if (contribution.sourceMember.isEmpty()) {
                    contribution.sourceKind == "NamedType" && name == sourceTypeName
                } else {
                    val expectedName = when (contribution.sourceMember) {
                        ".ctor", ".cctor" -> sourceTypeName
                        else -> contribution.sourceMember
                    }
                    name == expectedName && belongsToType(declaration, sourceTypeName) &&
                        kindMatches(className, contribution.sourceKind) &&
                        (contribution.sourceKind != "Method" ||
                            parameterCount == contribution.sourceParameterCount)
                }
            }
            .distinct()
            .toList()
    }

    private fun belongsToType(declaration: CSharpDeclaration, sourceTypeName: String): Boolean =
        generateSequence(declaration.parent) { it.parent }
            .filterIsInstance<CSharpDeclaration>()
            .any { it.declaredName == sourceTypeName }

    private fun kindMatches(psiClassName: String, symbolKind: String): Boolean = when (symbolKind) {
        "Method" -> "Method" in psiClassName || "Constructor" in psiClassName || "Delegate" in psiClassName
        "Field" -> "Field" in psiClassName || "Const" in psiClassName || "EnumMember" in psiClassName
        "Property" -> "Property" in psiClassName || "Indexer" in psiClassName
        "Event" -> "Event" in psiClassName
        else -> true
    }

    fun update(filePath: String, contributions: List<MixinContribution>) {
        val grouped = contributions.groupBy(MixinContribution::offset)
        if (contributionsByFile.put(filePath, grouped) == grouped) return
        ApplicationManager.getApplication().invokeLater {
            if (!project.isDisposed) DaemonCodeAnalyzer.getInstance(project).restart()
        }
    }
}

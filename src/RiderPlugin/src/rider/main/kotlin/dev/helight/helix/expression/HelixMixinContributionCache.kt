package dev.helight.helix.expression

import com.intellij.codeInsight.daemon.DaemonCodeAnalyzer
import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.components.Service
import com.intellij.openapi.project.Project
import com.intellij.psi.util.PsiTreeUtil
import com.jetbrains.rider.projectView.solution
import com.jetbrains.rider.languages.fileTypes.csharp.psi.CSharpDeclaration
import com.jetbrains.rider.languages.fileTypes.csharp.psi.impl.CSharpParameterDeclaration
import dev.helight.helix.protocol.MixinContribution
import dev.helight.helix.protocol.MixinExpressionRequest
import dev.helight.helix.protocol.helixExpressionModel
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import java.util.concurrent.ConcurrentHashMap

@Service(Service.Level.PROJECT)
class HelixMixinContributionCache(
    private val project: Project,
    private val coroutineScope: CoroutineScope,
) {
    private val contributionsByFile = ConcurrentHashMap<String, Map<Int, List<MixinContribution>>>()
    private val requestsInFlight = ConcurrentHashMap.newKeySet<String>()
    private val requestedRevision = ConcurrentHashMap<String, Long>()
    private val requestedSourceText = ConcurrentHashMap<String, String>()
    private val completedRevision = ConcurrentHashMap<String, Long>()

    fun request(filePath: String, revision: Long, sourceText: String): List<MixinContribution> {
        requestedRevision[filePath] = revision
        requestedSourceText[filePath] = sourceText
        if (completedRevision[filePath] == revision) {
            return contributionsByFile[filePath].orEmpty().values.flatten()
        }
        if (requestsInFlight.add(filePath)) {
            coroutineScope.launch {
                try {
                    var activeRevision = requestedRevision[filePath]
                    var attempt = 0
                    while (attempt < RETRY_COUNT) {
                        val contributions = project.solution.helixExpressionModel.getMixinContributions
                            .startSuspending(
                                MixinExpressionRequest(
                                    filePath,
                                    requestedSourceText[filePath].orEmpty(),
                                    activeRevision ?: revision,
                                )
                            )
                            .contributions
                            .toList()
                        val newestRevision = requestedRevision[filePath]
                        if (newestRevision != activeRevision) {
                            activeRevision = newestRevision
                            attempt = 0
                            continue
                        }

                        attempt++
                        // Roslyn briefly reports no generator diagnostics while rebuilding.
                        // Preserve the previous gutter state until the final retry confirms it.
                        if (contributions.isNotEmpty() || attempt == RETRY_COUNT) {
                            update(filePath, contributions)
                        }
                        if (attempt < RETRY_COUNT) {
                            delay(RETRY_DELAY_MS)
                        } else {
                            completedRevision[filePath] = activeRevision ?: revision
                        }
                    }
                } catch (exception: CancellationException) {
                    throw exception
                } catch (_: Throwable) {
                    // The Roslyn component may not be registered yet while Rider is starting.
                    // A later Code Vision pass can retry without blocking editor loading.
                } finally {
                    requestsInFlight.remove(filePath)
                }
            }
        }
        return contributionsByFile[filePath].orEmpty().values.flatten()
    }

    companion object {
        private const val RETRY_COUNT = 4
        private const val RETRY_DELAY_MS = 500L
    }

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

package dev.helight.helix.mixin

import com.intellij.codeInsight.completion.CompletionContributor
import com.intellij.codeInsight.completion.CompletionParameters
import com.intellij.codeInsight.completion.CompletionProvider
import com.intellij.codeInsight.completion.CompletionResultSet
import com.intellij.codeInsight.completion.CompletionType
import com.intellij.codeInsight.completion.PrioritizedLookupElement
import com.intellij.codeInsight.lookup.LookupElement
import com.intellij.codeInsight.lookup.LookupElementBuilder
import com.intellij.patterns.PlatformPatterns
import com.intellij.util.ProcessingContext
import com.intellij.psi.PsiFile
import com.intellij.psi.util.PsiTreeUtil
import dev.helight.helix.protocol.MixinCompletionSite
import dev.helight.helix.protocol.MixinFileSnapshot
import dev.helight.helix.protocol.MixinLanguageDefinition
import dev.helight.helix.protocol.MixinSourceRange

/** Completion uses synchronous local syntax contexts and enriches them with backend metadata. */
class HelixMixinCompletionContributor : CompletionContributor() {
    init {
        extend(CompletionType.BASIC,
            PlatformPatterns.psiElement().withLanguage(HelixMixinLanguage), Provider())
    }

    private class Provider : CompletionProvider<CompletionParameters>() {
        override fun addCompletions(parameters: CompletionParameters, context: ProcessingContext,
                                    result: CompletionResultSet) {
            val file = parameters.originalFile
            val source = file.text
            val offset = parameters.offset.coerceIn(0, source.length)
            val path = file.virtualFile.getUserData(HelixMixinSnapshotService.ORIGINAL_PATH)
                ?: file.virtualFile.path
            val service = HelixMixinSnapshotService.getInstance(file.project)
            val snapshot = service.snapshotForText(source, path)
            val site = snapshot?.let { findSite(it, offset) } ?: localSite(source, offset) ?: return
            val replacement = site.replacementRange
            if (replacement.startOffset !in 0..offset || replacement.endOffset !in offset..source.length) return
            val output = result.withPrefixMatcher(source.substring(replacement.startOffset, offset))

            when (site.kind) {
                "Directive" -> definitions(service, "Directive").forEach { definition ->
                    val tail = "<>".repeat(definition.minimumArguments) +
                        if (definition.operandType != "None") " " else ""
                    output.addElement(item(definition.name, "@${definition.name}", tail,
                        definition.documentation, site, offset, 100.0))
                }
                "Root" -> definitions(service, "Root").forEach { definition ->
                    output.addElement(item(definition.name, definition.name, "", definition.documentation,
                        site, offset, 90.0))
                }
                "Function", "Predicate" -> definitions(service, site.kind)
                    .filter { receiverMatches(site.receiverType, it.receiverType) }
                    .forEach { definition ->
                        val tail = "<>".repeat(definition.minimumArguments)
                        output.addElement(item(definition.name, definition.name, tail,
                            definition.receiverType, site, offset, 80.0))
                    }
                "Label" -> {
                    psiDeclarations(file, setOf("SCOPE", "LABEL"), site, offset, output, 125.0)
                    snapshot?.let { declarations(listOf(it), "Label", offset, site, output, 120.0) }
                }
                "DeclaredFunction" -> {
                    psiDeclarations(file, setOf("FUNC"), site, offset, output, 115.0)
                    declarations(service.snapshotsInDirectory(path), "Function",
                        offset, site, output, 110.0, snapshot?.filePath ?: path)
                }
                "Local" -> {
                    psiDeclarations(file, setOf("LOCAL"), site, offset, output, 125.0)
                    snapshot?.let { declarations(listOf(it), "Local", offset, site, output, 120.0) }
                }
                "Variable" -> {
                    psiDeclarations(file, setOf("VAR"), site, offset, output, 125.0)
                    snapshot?.let { declarations(listOf(it), "Variable", offset, site, output, 120.0) }
                }
                "TargetVariable" -> {
                    psiDeclarations(file, setOf("TAR"), site, offset, output, 125.0)
                    snapshot?.let { declarations(listOf(it), "TargetVariable", offset, site, output, 120.0) }
                }
                "Carry" -> {
                    psiDeclarations(file, setOf("CARRY"), site, offset, output, 125.0)
                    snapshot?.let { declarations(listOf(it), "Carry", offset, site, output, 120.0) }
                }
                "OutputTarget" -> definitions(service, "OutputTarget").forEach { definition ->
                    output.addElement(item(definition.name, definition.name, "", definition.documentation,
                        site, offset, 80.0))
                }
                "CSharpType" -> site.items.forEach { candidate ->
                    val namespace = candidate.insertText.substringBeforeLast('.', "")
                    output.addElement(semanticItem(candidate.name, candidate.insertText, namespace,
                        site, offset, 130.0))
                }
            }
        }

        private fun definitions(service: HelixMixinSnapshotService, kind: String): Sequence<MixinLanguageDefinition> =
            service.definitions.asSequence().filter { it.kind == kind }.sortedBy { it.name }

        private fun declarations(snapshots: List<MixinFileSnapshot>, kind: String, offset: Int,
                                 site: MixinCompletionSite, result: CompletionResultSet,
                                 priority: Double, currentFile: String? = null) {
            val seen = HashSet<String>()
            snapshots.sortedBy { if (it.filePath == currentFile) 0 else 1 }.forEach { snapshot ->
                snapshot.declarations.asSequence().filter {
                    it.kind == kind && (kind == "Function" ||
                        offset in it.scope.startOffset..it.scope.endOffset)
                }.forEach { declaration ->
                    if (!seen.add(declaration.name)) return@forEach
                    val own = currentFile == null || snapshot.filePath == currentFile
                    result.addElement(item(declaration.name, declaration.name, "", kind, site, offset,
                        priority - if (own) 0.0 else 10.0))
                }
            }
        }

        private fun psiDeclarations(file: PsiFile, commands: Set<String>, site: MixinCompletionSite,
                                    offset: Int, result: CompletionResultSet, priority: Double) {
            val seen = HashSet<String>()
            PsiTreeUtil.findChildrenOfType(file, HelixMixinDeclarationElement::class.java).forEach { declaration ->
                val directive = generateSequence(declaration.parent) { it.parent }
                    .firstOrNull { it.node.elementType == HelixMixinElementTypes.DIRECTIVE }
                    ?: return@forEach
                val command = directive.children.firstOrNull {
                    it.node.elementType == HelixMixinElementTypes.DIRECTIVE_NAME
                }?.text?.trimStart('@')?.uppercase() ?: return@forEach
                val name = declaration.text
                if (command in commands && name.isNotBlank() && seen.add(name))
                    result.addElement(item(name, name, "", command, site, offset, priority))
            }
        }

        private fun item(name: String, presentable: String, tail: String, type: String,
                         site: MixinCompletionSite, caretOffset: Int, priority: Double): LookupElement {
            val builder = LookupElementBuilder.create(name)
                .withPresentableText(presentable)
                .withTailText(tail, true)
                .withTypeText(type, true)
                .withInsertHandler { insertion, _ ->
                    val suffixLength = (site.replacementRange.endOffset - caretOffset).coerceAtLeast(0)
                    if (suffixLength > 0) {
                        val end = (insertion.tailOffset + suffixLength).coerceAtMost(insertion.document.textLength)
                        insertion.document.deleteString(insertion.tailOffset, end)
                    }
                    if (tail.isNotEmpty()) insertion.document.insertString(insertion.tailOffset, tail)
                    if (tail.startsWith("<>")) insertion.editor.caretModel.moveToOffset(insertion.tailOffset + 1)
                }
            return PrioritizedLookupElement.withPriority(builder, priority)
        }

        private fun semanticItem(name: String, insertText: String, type: String,
                                 site: MixinCompletionSite, caretOffset: Int,
                                 priority: Double): LookupElement {
            val builder = LookupElementBuilder.create(insertText)
                .withLookupString(name)
                .withPresentableText(name)
                .withTailText(if (type.isEmpty()) "" else "  $type", true)
                .withTypeText("C# type", true)
                .withInsertHandler { insertion, _ ->
                    val suffixLength = (site.replacementRange.endOffset - caretOffset).coerceAtLeast(0)
                    if (suffixLength > 0) {
                        val end = (insertion.tailOffset + suffixLength)
                            .coerceAtMost(insertion.document.textLength)
                        insertion.document.deleteString(insertion.tailOffset, end)
                    }
                }
            return PrioritizedLookupElement.withPriority(builder, priority)
        }

        private fun receiverMatches(actual: String, expected: String): Boolean =
            actual == "Any" || expected == "Any" || expected == "None" || actual == expected

        private fun findSite(snapshot: MixinFileSnapshot, offset: Int): MixinCompletionSite? =
            snapshot.completionSites.asSequence().filter {
                if (it.activationRange.startOffset == it.activationRange.endOffset)
                    offset == it.activationRange.startOffset
                else offset in it.activationRange.startOffset..it.activationRange.endOffset
            }.minWithOrNull(compareBy<MixinCompletionSite> {
                it.activationRange.endOffset - it.activationRange.startOffset
            }.thenByDescending { it.replacementRange.endOffset - it.replacementRange.startOffset })

        private fun localSite(source: String, offset: Int): MixinCompletionSite? =
            HelixMixinLocalCompletionParser.at(source, offset)?.let { context ->
                MixinCompletionSite(context.kind,
                    MixinSourceRange(context.replacementStart, context.replacementEnd),
                    MixinSourceRange(context.replacementStart, context.replacementEnd),
                    context.receiverType, emptyArray())
            }
    }
}

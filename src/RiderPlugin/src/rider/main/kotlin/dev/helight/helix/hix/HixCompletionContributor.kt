package dev.helight.helix.hix

import com.intellij.codeInsight.completion.CompletionContributor
import com.intellij.codeInsight.completion.CompletionParameters
import com.intellij.codeInsight.completion.CompletionProvider
import com.intellij.codeInsight.completion.CompletionResultSet
import com.intellij.codeInsight.completion.CompletionType
import com.intellij.codeInsight.lookup.LookupElementBuilder
import com.intellij.patterns.PlatformPatterns
import com.intellij.util.ProcessingContext
import com.intellij.openapi.components.service
import dev.helight.helix.hix.generated.HixLexer

/** Syntax completion is local and uses the same generated token stream as PSI. */
class HixCompletionContributor : CompletionContributor() {
    init {
        extend(CompletionType.BASIC, PlatformPatterns.psiElement().withLanguage(HixLanguage),
            object : CompletionProvider<CompletionParameters>() {
                override fun addCompletions(parameters: CompletionParameters, context: ProcessingContext,
                                            result: CompletionResultSet) {
                    val source = parameters.originalFile.text
                    val offset = parameters.offset.coerceIn(0, source.length)
                    val parsed = HixAntlrSyntax.parse(source)
                    val token = parsed.tokens.firstOrNull { it.start < offset && offset <= it.end }
                    if (token?.type in setOf(HixLexer.ARGUMENT_TEXT, HixLexer.CONTENT_TEXT, HixLexer.COMMENT,
                            HixLexer.SLASH_COMMENT, HixLexer.ESCAPE_LITERAL, HixLexer.ESCAPE_HEX)) return
                    val service = parameters.position.project.service<HixSnapshotService>()
                    service.ensureLanguageCatalog()
                    val path = parameters.originalFile.virtualFile?.path
                    val snapshot = service.snapshotForText(source, path)
                    val site = HixLookup.site(parsed, offset)
                    val receiver = HixLookup.receiverType(parsed, site.receiverEnd, snapshot)
                    val names = linkedSetOf<String>()
                    if (!site.chained && !site.member) {
                        for (index in 1..HixLexer.VOCABULARY.maxTokenType) {
                            if (HixLexer.VOCABULARY.getSymbolicName(index).orEmpty().startsWith("KEYWORD_"))
                                HixLexer.VOCABULARY.getLiteralName(index)?.trim('\'')?.let(names::add)
                        }
                        names += service.definitions.filter { it.kind == "Root" }.map { it.name }
                        HixAntlrSyntax.declarationNames(parsed.tree).forEach { names += it.text }
                    }
                    if (!site.member) {
                        service.definitions.asSequence().filter { it.kind == "Function" }
                            .filter { !site.chained || HixLookup.acceptsReceiver(it, receiver) }
                            .distinctBy { HixLookup.signature(it) }.forEach { definition ->
                                result.addElement(LookupElementBuilder.create(definition, definition.name)
                                    .withTailText("(" + definition.argumentTypes.joinToString(", ") { it.lowercase() } + ")", true)
                                    .withTypeText(definition.resultType.lowercase()))
                            }
                    }
                    if (path != null && !site.member) {
                        service.snapshotsInDirectory(path).flatMap { it.declarations.asList() }
                            .filter { !site.chained || it.kind == "Function" }
                            .forEach { names += it.name }
                    }
                    snapshot?.completionSites?.filter {
                        offset in it.activationRange.startOffset..it.activationRange.endOffset
                    }?.flatMap { it.items.asList() }?.distinctBy { it.insertText }?.forEach { item ->
                        names.remove(item.insertText)
                        result.addElement(LookupElementBuilder.create(item, item.insertText)
                            .withPresentableText(item.name).withTypeText(item.kind))
                    }
                    names.forEach { result.addElement(LookupElementBuilder.create(it)) }
                }
            })
    }
}

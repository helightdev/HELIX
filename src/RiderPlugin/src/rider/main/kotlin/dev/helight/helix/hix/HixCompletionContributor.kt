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
                    val service = parameters.position.project.service<HixSnapshotService>()
                    service.ensureLanguageCatalog()
                    val path = parameters.originalFile.virtualFile?.path
                    val metadataArgument = HixLookup.metadataArgumentAt(parsed, (offset - 1).coerceAtLeast(0))
                    val patternArgument = metadataArgument?.let { argument ->
                        service.definitions.firstOrNull { it.kind == "PatternMetadata" && it.name == argument.name }
                            ?.let { definition ->
                                val type = definition.argumentTypes.getOrNull(argument.index)
                                    ?: if (definition.variadic) definition.argumentTypes.lastOrNull() else null
                                type == "Pattern"
                            }
                    } == true
                    if (!patternArgument && token?.type in setOf(HixLexer.ARGUMENT_TEXT, HixLexer.CONTENT_TEXT,
                            HixLexer.COMMENT, HixLexer.SLASH_COMMENT, HixLexer.ESCAPE_LITERAL,
                            HixLexer.ESCAPE_HEX)) return

                    fun addPatternCompletions() {
                        service.definitions.filter { it.kind == "Kind" }.forEach { definition ->
                            result.addElement(LookupElementBuilder.create(definition, definition.name)
                                .withTypeText("kind"))
                        }
                        val names = HixAntlrSyntax.rules(parsed.tree)
                            .filterIsInstance<dev.helight.helix.hix.generated.HixParser.TypeDeclarationContext>()
                            .map { it.IDENTIFIER().text }.toMutableSet()
                        if (path != null) names += service.snapshotsInDirectory(path).flatMap { it.declarations.asList() }
                            .filter { it.kind == "Pattern" }.map { it.name }
                        names.forEach { result.addElement(LookupElementBuilder.create(it).withTypeText("pattern")) }
                    }
                    if (patternArgument) {
                        addPatternCompletions()
                        return
                    }
                    when (HixLookup.semanticRoleAt(parsed, (offset - 1).coerceAtLeast(0))) {
                        HixLookup.SemanticRole.FileMetadata -> {
                            service.definitions.filter { it.kind == "FileMetadata" }.forEach { definition ->
                                val arguments = definition.argumentTypes.joinToString("") { "<${it.lowercase()}>" }
                                result.addElement(LookupElementBuilder.create(definition, definition.name)
                                    .withTailText(arguments, true).withTypeText("file metadata"))
                            }
                            return
                        }
                        HixLookup.SemanticRole.PatternMetadata -> {
                            val target = HixLookup.patternMetadataTargetAt(parsed, (offset - 1).coerceAtLeast(0))
                            service.definitions.filter { definition ->
                                definition.kind == "PatternMetadata" &&
                                    (definition.operandType == "Pattern" || definition.operandType == target)
                            }.forEach { definition ->
                                val arguments = definition.argumentTypes.mapIndexed { index, type ->
                                    val rendered = "<${type.lowercase()}>"
                                    if (definition.variadic && index == definition.argumentTypes.lastIndex)
                                        "$rendered..." else rendered
                                }.joinToString("")
                                result.addElement(LookupElementBuilder.create(definition, definition.name)
                                    .withTailText(arguments, true).withTypeText("pattern metadata"))
                            }
                            return
                        }
                        HixLookup.SemanticRole.Pattern -> {
                            addPatternCompletions()
                            return
                        }
                        HixLookup.SemanticRole.Metadata -> return
                        null -> Unit
                    }
                    val snapshot = service.snapshotForText(source, path)
                    val site = HixLookup.site(parsed, offset)
                    val receiver = HixLookup.receiverType(parsed, site.receiverEnd, snapshot)
                    val valueContext = HixLookup.isValueContextAt(parsed, offset)
                    val statementContext = HixLookup.isStatementBlockAt(parsed, offset) && !valueContext
                    val names = linkedSetOf<String>()
                    if (!site.chained && !site.member) {
                        if (!valueContext) {
                            for (index in 1..HixLexer.VOCABULARY.maxTokenType) {
                                if (HixLexer.VOCABULARY.getSymbolicName(index).orEmpty().startsWith("KEYWORD_"))
                                    HixLexer.VOCABULARY.getLiteralName(index)?.trim('\'')?.let { keyword ->
                                        if (!statementContext || keyword in statementKeywords) names += keyword
                                    }
                            }
                        }
                        if (valueContext) {
                            names += service.definitions.filter { it.kind == "Root" }.map { it.name }
                            HixAntlrSyntax.rules(parsed.tree)
                                .filterIsInstance<dev.helight.helix.hix.generated.HixParser.VariableIdentifierContext>()
                                .forEach { names += it.IDENTIFIER().text }
                        } else if (!statementContext) {
                            HixAntlrSyntax.declarationNames(parsed.tree).forEach { names += it.text }
                        }
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
                            .filter { declaration -> when {
                                site.chained || statementContext -> declaration.kind == "Function"
                                valueContext -> declaration.kind in setOf("Function", "Pattern")
                                else -> true
                            } }
                            .forEach { names += it.name }
                    }
                    snapshot?.completionSites?.filter {
                        offset in it.activationRange.startOffset..it.activationRange.endOffset
                    }?.flatMap { site ->
                        if (site.kind == "CSharpType") {
                            val start = site.replacementRange.startOffset.coerceIn(0, source.length)
                            val prefix = source.substring(start, offset.coerceAtLeast(start))
                            service.lazyCompletions(site.kind, prefix).asList()
                        } else site.items.asList()
                    }?.distinctBy { it.insertText }?.forEach { item ->
                        names.remove(item.insertText)
                        result.addElement(LookupElementBuilder.create(item, item.insertText)
                            .withPresentableText(item.name).withTypeText(item.kind))
                    }
                    names.forEach { result.addElement(LookupElementBuilder.create(it)) }
                }
            })
    }

    private companion object {
        val statementKeywords = setOf("return", "goto", "break", "continue", "when", "local", "var", "target", "carry")
    }
}

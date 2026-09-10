package dev.helight.helix.hix

import com.intellij.codeInsight.completion.CompletionContributor
import com.intellij.codeInsight.completion.CompletionParameters
import com.intellij.codeInsight.completion.CompletionProvider
import com.intellij.codeInsight.completion.CompletionResultSet
import com.intellij.codeInsight.completion.CompletionType
import com.intellij.codeInsight.lookup.LookupElementBuilder
import com.intellij.openapi.components.service
import com.intellij.patterns.PlatformPatterns
import com.intellij.util.ProcessingContext
import dev.helight.helix.hix.generated.HixLexer
import dev.helight.helix.hix.generated.HixParser

/**
 * These are ordinary symbol completions, so BASIC is the correct IntelliJ completion type.
 * Expected-type filtering is expressed through analyzer query arguments; SMART and CLASS_NAME
 * would introduce separate invocation modes with JVM-specific semantics.
 */
abstract class HixBasicCompletionContributor(provider: CompletionProvider<CompletionParameters>) :
    CompletionContributor() {
    init {
        extend(CompletionType.BASIC, PlatformPatterns.psiElement().withLanguage(HixLanguage), provider)
    }
}

class HixMetadataCompletionContributor : HixBasicCompletionContributor(MetadataProvider)
class HixPatternCompletionContributor : HixBasicCompletionContributor(PatternProvider)
class HixExpressionCompletionContributor : HixBasicCompletionContributor(ExpressionProvider)

private data class HixCompletionContext(
    val source: String,
    val offset: Int,
    val parsed: HelixAntlrParse,
    val token: HelixAntlrToken?,
    val service: HixSnapshotService,
    val path: String?
) {
    fun definitions(kind: String, receiverType: String = "", operandType: String = "",
                    prefix: String = "") =
        service.queryDefinitions(kind, receiverType, operandType, prefix, source).asList()

    fun metadataDefinition(argument: HixLookup.MetadataArgument) =
        (definitions("FileMetadata", prefix = argument.name) +
            definitions("PatternMetadata", prefix = argument.name)).firstOrNull { it.name == argument.name }

    companion object {
        fun from(parameters: CompletionParameters): HixCompletionContext {
            val source = parameters.originalFile.text
            val offset = parameters.offset.coerceIn(0, source.length)
            val parsed = HixAntlrSyntax.parse(source)
            return HixCompletionContext(source, offset, parsed,
                parsed.tokens.firstOrNull { it.start < offset && offset <= it.end },
                parameters.position.project.service(), parameters.originalFile.virtualFile?.path)
        }
    }
}

private object MetadataProvider : CompletionProvider<CompletionParameters>() {
    override fun addCompletions(parameters: CompletionParameters, processing: ProcessingContext,
                                result: CompletionResultSet) {
        val context = HixCompletionContext.from(parameters)
        val position = (context.offset - 1).coerceAtLeast(0)
        val argument = HixLookup.metadataArgumentAt(context.parsed, position)
        if (argument != null) {
            val definition = context.metadataDefinition(argument) ?: return
            val kind = definition.argumentTypes.getOrNull(argument.index)
                ?: if (definition.variadic) definition.argumentTypes.lastOrNull() else null
            if (kind == "Backend") {
                val prefix = context.token?.let {
                    context.source.substring(it.start.coerceAtLeast(0),
                        context.offset.coerceIn(it.start, it.end))
                }.orEmpty()
                val matched = result.withPrefixMatcher(prefix)
                listOf("Standalone", "Unity").filter { it.startsWith(prefix, true) }.forEach {
                    matched.addElement(LookupElementBuilder.create(it).withTypeText("backend"))
                }
            }
            return
        }
        when (HixLookup.semanticRoleAt(context.parsed, position)) {
            HixLookup.SemanticRole.FileMetadata -> addMetadata(context, result, "FileMetadata", "file metadata")
            HixLookup.SemanticRole.PatternMetadata -> addMetadata(context, result, "PatternMetadata",
                "pattern metadata", HixLookup.patternMetadataTargetAt(context.parsed, position).orEmpty())
            else -> Unit
        }
    }

    private fun addMetadata(context: HixCompletionContext, result: CompletionResultSet, kind: String,
                            typeText: String, operandType: String = "") {
        val prefix = CompletionText.metadataPrefix(context.source, context.offset)
        val matched = result.withPrefixMatcher(prefix)
        context.definitions(kind, operandType = operandType, prefix = prefix).forEach { definition ->
            val arguments = definition.argumentTypes.mapIndexed { index, type ->
                val rendered = "<${type.lowercase()}>"
                if (definition.variadic && index == definition.argumentTypes.lastIndex) "$rendered..." else rendered
            }.joinToString("")
            matched.addElement(LookupElementBuilder.create(definition, definition.name)
                .withTailText(arguments, true).withTypeText(typeText))
        }
    }
}

private object PatternProvider : CompletionProvider<CompletionParameters>() {
    override fun addCompletions(parameters: CompletionParameters, processing: ProcessingContext,
                                result: CompletionResultSet) {
        val context = HixCompletionContext.from(parameters)
        val position = (context.offset - 1).coerceAtLeast(0)
        val argument = HixLookup.metadataArgumentAt(context.parsed, position)
        val patternArgument = argument?.let { metadata ->
            val definition = context.metadataDefinition(metadata) ?: return@let false
            (definition.argumentTypes.getOrNull(metadata.index)
                ?: if (definition.variadic) definition.argumentTypes.lastOrNull() else null) == "Pattern"
        } == true
        if (!patternArgument && HixLookup.semanticRoleAt(context.parsed, position) != HixLookup.SemanticRole.Pattern)
            return

        context.definitions("Kind").forEach { definition ->
            result.addElement(LookupElementBuilder.create(definition, definition.name).withTypeText("kind"))
        }
        val names = HixAntlrSyntax.rules(context.parsed.tree).filterIsInstance<HixParser.TypeDeclarationContext>()
            .map { it.IDENTIFIER().text }.toMutableSet()
        if (context.path != null) names += context.service.snapshotsInDirectory(context.path)
            .flatMap { it.declarations.asList() }.filter { it.kind == "Pattern" }.map { it.name }
        names.forEach { result.addElement(LookupElementBuilder.create(it).withTypeText("pattern")) }

        if (!patternArgument) {
            val start = context.token?.start?.coerceIn(0, context.offset) ?: context.offset
            context.service.lazyCompletions("CSharpType",
                context.source.substring(start, context.offset), context.source).forEach { item ->
                result.addElement(LookupElementBuilder.create(item, item.insertText)
                    .withPresentableText(item.name).withTypeText(item.kind))
            }
        }
    }
}

private object ExpressionProvider : CompletionProvider<CompletionParameters>() {
    override fun addCompletions(parameters: CompletionParameters, processing: ProcessingContext,
                                result: CompletionResultSet) {
        val context = HixCompletionContext.from(parameters)
        val position = (context.offset - 1).coerceAtLeast(0)
        if (HixLookup.metadataArgumentAt(context.parsed, position) != null ||
            HixLookup.semanticRoleAt(context.parsed, position) != null) return
        val enumValues = HixLookup.enumValuesAt(context.parsed, context.offset)
        enumValues.forEach { literal ->
            result.addElement(LookupElementBuilder.create(literal)
                .withPresentableText(literal.removeSurrounding("<", ">"))
                .withTypeText("enum"))
        }
        if (enumValues.isNotEmpty()) return
        if (context.token?.type in setOf(HixLexer.ARGUMENT_TEXT, HixLexer.CONTENT_TEXT, HixLexer.COMMENT,
                HixLexer.SLASH_COMMENT, HixLexer.ESCAPE_LITERAL, HixLexer.ESCAPE_HEX)) return

        parameters.originalFile.virtualFile?.let { context.service.observe(it, context.source) }
        val snapshot = context.service.snapshotForText(context.source, context.path)
        val site = HixLookup.site(context.parsed, context.offset)
        val receiver = HixLookup.receiverType(context.parsed, site.receiverEnd, snapshot)
        val valueContext = HixLookup.isValueContextAt(context.parsed, context.offset)
        val statementContext = HixLookup.isStatementBlockAt(context.parsed, context.offset) && !valueContext
        val names = linkedSetOf<String>()

        if (!site.chained && !site.member) {
            if (!valueContext) for (index in 1..HixLexer.VOCABULARY.maxTokenType) {
                if (HixLexer.VOCABULARY.getSymbolicName(index).orEmpty().startsWith("KEYWORD_"))
                    HixLexer.VOCABULARY.getLiteralName(index)?.trim('\'')?.let { keyword ->
                        if (!statementContext || keyword in statementKeywords) names += keyword
                    }
            }
            if (valueContext || statementContext) names += context.definitions("Root").map { it.name }
            if (valueContext) HixAntlrSyntax.rules(context.parsed.tree)
                .filterIsInstance<HixParser.VariableIdentifierContext>()
                .forEach { names += it.IDENTIFIER().text }
            else if (!statementContext) HixAntlrSyntax.declarationNames(context.parsed.tree)
                .forEach { names += it.text }
        }

        if (!site.member) context.definitions("Function", if (site.chained) receiver else "").asSequence()
            .filter { !site.chained || HixLookup.acceptsReceiver(it, receiver) }
            .distinctBy(HixLookup::signature).forEach { definition ->
                result.addElement(LookupElementBuilder.create(definition, definition.name)
                    .withTailText("(" + definition.argumentTypes.joinToString(", ") { it.lowercase() } + ")", true)
                    .withTypeText(definition.resultType.lowercase()))
            }

        if (context.path != null && !site.member) context.service.snapshotsInDirectory(context.path)
            .flatMap { it.declarations.asList() }.filter { declaration -> when {
                site.chained || statementContext -> declaration.kind == "Function"
                valueContext -> declaration.kind in setOf("Function", "Pattern")
                else -> true
            } }.forEach { names += it.name }
        names.forEach { result.addElement(LookupElementBuilder.create(it)) }
    }

    private val statementKeywords = setOf("return", "goto", "break", "continue", "when", "local", "var",
        "target", "carry")
}

private object CompletionText {
    fun metadataPrefix(source: String, offset: Int): String {
        val end = offset.coerceIn(0, source.length)
        val lineStart = source.lastIndexOfAny(charArrayOf('\n', '\r'), (end - 1).coerceAtLeast(0)) + 1
        val percent = source.lastIndexOf('%', end - 1)
        if (percent < lineStart) return ""
        return source.substring(percent + 1, end).takeWhile { it == '_' || it.isLetterOrDigit() }
    }
}

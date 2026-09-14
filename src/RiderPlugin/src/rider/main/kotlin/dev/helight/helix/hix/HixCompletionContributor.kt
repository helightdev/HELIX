package dev.helight.helix.hix

import com.intellij.codeInsight.completion.CompletionContributor
import com.intellij.codeInsight.completion.CompletionParameters
import com.intellij.codeInsight.completion.CompletionProvider
import com.intellij.codeInsight.completion.CompletionResultSet
import com.intellij.codeInsight.completion.CompletionType
import com.intellij.codeInsight.lookup.LookupElementBuilder
import com.intellij.codeInsight.completion.PrioritizedLookupElement
import com.intellij.openapi.components.service
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.patterns.PlatformPatterns
import com.intellij.util.ProcessingContext
import dev.helight.helix.hix.generated.HixLexer
import dev.helight.helix.hix.generated.HixParser

private fun LookupElementBuilder.ranked(priority: Double) = PrioritizedLookupElement.withPriority(this, priority)

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
    val file: VirtualFile?
) {
    val path get() = file?.path
    fun definitions(kind: String, receiverType: String = "", operandType: String = "",
                    prefix: String = "") =
        service.definitionsFor(source, file).filter { definition -> definition.kind == kind &&
            (operandType.isEmpty() || definition.operandType == "Pattern" || definition.operandType.equals(operandType, true)) &&
            (prefix.isEmpty() || definition.name.startsWith(prefix, true)) }

    fun metadataDefinition(argument: HixLookup.MetadataArgument) =
        (definitions("FileMetadata", prefix = argument.name) +
            definitions("PatternMetadata", prefix = argument.name) +
            definitions("TypeMetadata", prefix = argument.name) +
            definitions("DeclarationMetadata", prefix = argument.name)).firstOrNull { it.name == argument.name }

    companion object {
        fun from(parameters: CompletionParameters): HixCompletionContext {
            val source = parameters.originalFile.text
            val offset = parameters.offset.coerceIn(0, source.length)
            val parsed = HixAntlrSyntax.parse(source)
            return HixCompletionContext(source, offset, parsed,
                parsed.tokens.firstOrNull { it.start < offset && offset <= it.end },
                parameters.position.project.service(), parameters.originalFile.virtualFile)
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
            if (definition.name == "graph" && argument.index == 0) {
                val prefix = context.token?.let {
                    context.source.substring(it.start.coerceAtLeast(0), context.offset.coerceIn(it.start, it.end))
                }.orEmpty()
                val matched = result.withPrefixMatcher(prefix)
                listOf("value", "flow").filter { it.startsWith(prefix, true) }.forEach {
                    matched.addElement(LookupElementBuilder.create(it).withIcon(HixCompletionIcons.forKind("Keyword")).withTypeText("graph").ranked(1000.0))
                }
                return
            }
            val kind = definition.argumentTypes.getOrNull(argument.index)
                ?: if (definition.variadic) definition.argumentTypes.lastOrNull() else null
            if (kind == "Backend") {
                val prefix = context.token?.let {
                    context.source.substring(it.start.coerceAtLeast(0),
                        context.offset.coerceIn(it.start, it.end))
                }.orEmpty()
                val matched = result.withPrefixMatcher(prefix)
                listOf("Standalone", "Unity").filter { it.startsWith(prefix, true) }.forEach {
                    matched.addElement(LookupElementBuilder.create(it).withIcon(HixCompletionIcons.forKind("Dependency")).withTypeText("backend").ranked(1000.0))
                }
            }
            return
        }
        when (HixLookup.semanticRoleAt(context.parsed, position)) {
            HixLookup.SemanticRole.FileMetadata -> addMetadata(context, result, "FileMetadata", "file metadata")
            HixLookup.SemanticRole.PatternMetadata -> addMetadata(context, result, "PatternMetadata",
                "pattern metadata", HixLookup.patternMetadataTargetAt(context.parsed, position).orEmpty())
            HixLookup.SemanticRole.TypeMetadata -> addMetadata(context, result, "TypeMetadata", "type metadata", "Type")
            HixLookup.SemanticRole.DeclarationMetadata -> addMetadata(context, result, "DeclarationMetadata", "documentation")
            else -> Unit
        }
    }

    private fun addMetadata(context: HixCompletionContext, result: CompletionResultSet, kind: String,
                            typeText: String, operandType: String = "") {
        val prefix = CompletionText.metadataPrefix(context.source, context.offset)
        val matched = result.withPrefixMatcher(prefix)
        context.definitions(kind, operandType = operandType, prefix = prefix).distinctBy { it.name }.forEach { definition ->
            val arguments = definition.argumentTypes.mapIndexed { index, type ->
                val rendered = "<${type.lowercase()}>"
                if (definition.variadic && index == definition.argumentTypes.lastIndex) "$rendered..." else rendered
            }.joinToString("")
            matched.addElement(LookupElementBuilder.create(definition, definition.name)
                .withIcon(HixCompletionIcons.forKind(kind)).withTailText(arguments, true).withTypeText(typeText).ranked(900.0))
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
                ?: if (definition.variadic) definition.argumentTypes.lastOrNull() else null)?.equals("pattern", ignoreCase = true) == true
        } == true
        if (!patternArgument && HixLookup.semanticRoleAt(context.parsed, position) != HixLookup.SemanticRole.Pattern)
            return

        context.definitions("PrimitivePattern").forEach { definition ->
            result.addElement(LookupElementBuilder.create(definition, definition.name).withIcon(HixCompletionIcons.forKind("PrimitivePattern")).withTypeText("pattern").ranked(800.0))
        }
        val names = HixAntlrSyntax.rules(context.parsed.tree).filterIsInstance<HixParser.TypeDeclarationContext>()
            .map { it.IDENTIFIER().text }.toMutableSet()
        if (context.file != null) names += context.service.snapshotsInContext(context.file)
            .flatMap { it.declarations.asList() }.filter { it.kind == "Pattern" }.map { it.name }
        names.forEach { result.addElement(LookupElementBuilder.create(it).withIcon(HixCompletionIcons.forKind("Pattern")).withTypeText("pattern").ranked(900.0)) }

        if (!patternArgument) {
            val start = context.token?.start?.coerceIn(0, context.offset) ?: context.offset
            context.service.lazyCompletions("CSharpType",
                context.source.substring(start, context.offset), context.source, context.file).forEach { item ->
                result.addElement(LookupElementBuilder.create(item, item.insertText)
                    .withIcon(HixCompletionIcons.forKind(item.kind)).withPresentableText(item.name).withTypeText(item.kind).ranked(500.0))
            }
        }
    }
}

private object ExpressionProvider : CompletionProvider<CompletionParameters>() {
    override fun addCompletions(parameters: CompletionParameters, processing: ProcessingContext, result: CompletionResultSet) {
        val context = HixCompletionContext.from(parameters)
        val position = (context.offset - 1).coerceAtLeast(0)
        if (HixLookup.metadataArgumentAt(context.parsed, position) != null ||
            HixLookup.semanticRoleAt(context.parsed, position) != null) return
        HixLookup.enumValuesAt(context.parsed, context.offset).takeIf { it.isNotEmpty() }?.let { values ->
            values.forEach { result.addElement(LookupElementBuilder.create(it).withIcon(HixCompletionIcons.forKind("Enum")).ranked(1300.0)) }
            return
        }
        if (context.token?.type in setOf(HixLexer.ARGUMENT_TEXT, HixLexer.CONTENT_TEXT, HixLexer.COMMENT,
                HixLexer.SLASH_COMMENT, HixLexer.STRING, HixLexer.ESCAPE_LITERAL, HixLexer.ESCAPE_HEX)) return
        val parsed = HixAntlrSyntax.parseIncomplete(context.source)
        val snapshot = context.service.snapshotForText(context.source, context.path)
        val site = HixLookup.site(parsed, context.offset)
        val value = HixLookup.isValueContextAt(parsed, context.offset)
        val statement = HixLookup.isStatementBlockAt(parsed, context.offset) && !value
        val siblings = context.file?.let(context.service::sourcesInContext).orEmpty().filter { it != context.source }
        val definitions = context.service.definitionsFor(context.source, context.file).toList()
        val call = HixEditorCalls.at(HixEditorCalls.hix(context.source, snapshot), context.offset)
        val signatures = call?.let { HixEditorCalls.signatures(context.source, definitions, siblings, it) }.orEmpty()
        val expected = HixCompletionScope.expectedType(call, signatures, context.offset)
        val shapes = (listOf(context.source) + siblings).flatMap { HixLookup.patterns(HixAntlrSyntax.parse(it)) }
        val bounds = shapes.associate { it.name to if (it.tuple) "tuple" else "table" }
        fun add(name: String, kind: String, type: String, priority: Double, item: Any = name, tail: String = "", asCall: Boolean = false) {
            val compatibility = if (expected.startsWith("delegate") || expected == "function") {
                if (kind == "Function") HixCompletionCompatibility.Match.Compatible else HixCompletionCompatibility.match(type, expected, bounds)
            } else HixCompletionCompatibility.match(type, expected, bounds)
            if (value && !name.endsWith(" = ") && compatibility == HixCompletionCompatibility.Match.Impossible) return
            val rank = if (value && expected != "any" && !name.endsWith(" = ")) minOf(priority, compatibility.priority) else priority
            val builder = LookupElementBuilder.create(item, name).withIcon(HixCompletionIcons.forKind(kind, shapes.any { it.trait && (name == it.name || name == it.name + ".new") }))
                .withTypeText(type.takeUnless { it == "any" }, true).withTailText(tail, true)
            result.addElement((if (asCall && !expected.startsWith("delegate") && expected != "function") builder.callInsertion() else builder).ranked(rank))
        }
        if (site.member) {
            val fact = snapshot?.typeFacts?.filter { it.kind == "Expression" && it.range.endOffset == site.receiverEnd }
                ?.minByOrNull { it.range.startOffset }
            fact?.memberNames?.forEachIndexed { index, name ->
                if (!Regex("[A-Za-z_0-9]+").matches(name)) return@forEachIndexed
                add(name, "Field", fact.memberTypes.getOrNull(index) ?: "any", 1200.0)
            }
            return
        }
        if (!site.chained && !value && !statement) {
            val inMixin = HixAntlrSyntax.rules(parsed.tree).filterIsInstance<HixParser.MixinDeclarationContext>().any {
                context.offset >= it.start.startIndex && context.offset <= it.stop.stopIndex + 1
            }
            (if (inMixin) mixinKeywords else declarationKeywords).forEach { add(it, "Keyword", "any", 800.0) }
            return
        }
        if (!site.chained) {
            call?.let { HixCompletionScope.remainingParameters(it, signatures, context.offset).forEach { parameter ->
                add(HixLookup.fieldName(parameter.name) + " = ", "Parameter", "any", 1300.0, tail = parameter.type)
            } }
            if (statement) statementKeywords.forEach { add(it, "Keyword", "any", 850.0) }
            if (value || statement) {
                val tokenStart = context.token?.start ?: context.offset
                val smart = context.source.getOrNull(tokenStart - 1) == '$'
                HixCompletionScope.variables(parsed, context.offset, snapshot).forEach {
                    add((if (smart) "" else "$" ) + it.name, if (it.parameter) "Parameter" else "Variable", it.type, 1150.0)
                }
                if (smart) return
                definitions.filter { it.kind == "Root" }.distinctBy { it.name }.forEach { add(it.name, "Root", it.resultType, 600.0, it) }
                if (value) {
                    definitions.filter { it.kind in setOf("PrimitivePattern", "Type") }.distinctBy { it.name }
                        .forEach { add(it.name, it.kind, "pattern", if (expected == "pattern") 1100.0 else 150.0, it) }
                    shapes.forEach { add(it.name, "Pattern", "pattern", if (expected == "pattern") 1100.0 else 150.0) }
                }
            }
        }
        val receiver = HixLookup.receiverType(parsed, site.receiverEnd, snapshot)
        val local = HixEditorCalls.local(context.source, context.offset) + siblings.flatMap { HixEditorCalls.local(it) }
        val functions = (local + definitions.filter { it.kind == "Function" }.map(HixEditorCalls::from)).distinctBy { it.display }
        functions.forEach { signature ->
            val compatibility = if (site.chained) HixCompletionCompatibility.match(receiver,
                signature.parameters.firstOrNull()?.type ?: return@forEach, bounds + signature.bounds) else HixCompletionCompatibility.Match.Compatible
            if (compatibility == HixCompletionCompatibility.Match.Impossible) return@forEach
            val constructor = shapes.any { signature.name == it.name || signature.name == it.name + ".new" }
            add(signature.name, if (constructor) "Constructor" else "Function", signature.result,
                if (site.chained) compatibility.priority else if (constructor) 950.0 else 750.0,
                signature, signature.parameters.joinToString(", ", "(", ")") { it.display }, asCall = !site.chained || signature.parameters.size > 1)
        }
    }
    private val statementKeywords = setOf("return", "goto", "break", "continue", "when", "local", "carry")
    private val declarationKeywords = setOf("func", "type", "trait", "mixin", "pure", "noinline", "derivation", "meta", "compiler", "bytecode", "runtime")
    private val mixinKeywords = setOf("expression", "func", "pure", "noinline", "prelude", "strict")
}

private fun LookupElementBuilder.callInsertion(): LookupElementBuilder = withInsertHandler { context, _ ->
    val offset = context.tailOffset
    if (context.document.charsSequence.getOrNull(offset) != '(') context.document.insertString(offset, "()")
    if (context.completionChar == '(') context.setAddCompletionChar(false)
    context.editor.caretModel.moveToOffset(offset + 1)
    com.intellij.codeInsight.AutoPopupController.getInstance(context.project).autoPopupParameterInfo(context.editor, null)
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

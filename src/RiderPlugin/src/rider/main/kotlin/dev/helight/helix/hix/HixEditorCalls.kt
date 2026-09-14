package dev.helight.helix.hix

import com.intellij.openapi.util.Key
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile
import dev.helight.helix.hal.HalAntlrSyntax
import dev.helight.helix.hal.HalLanguage
import dev.helight.helix.hal.generated.HalParser
import dev.helight.helix.hix.generated.HixParser
import dev.helight.helix.protocol.MixinFileSnapshot
import dev.helight.helix.protocol.MixinLanguageDefinition
import org.antlr.v4.runtime.ParserRuleContext
import org.antlr.v4.runtime.tree.TerminalNode

/** UI projections of compiler signatures. This does not bind or execute Hix calls. */
data class HixEditorParameter(val name: String, val type: String, val optional: Boolean = false,
                              val default: String = "", val variadic: Boolean = false) {
    val display: String get() = (if (variadic) "..." else "") +
        (if (name.isEmpty()) type else "$name: $type") +
        when { default.isNotEmpty() && default != "missing" -> " = $default"; optional -> " (optional)"; else -> "" }
}
data class HixEditorSignature(val name: String, val parameters: List<HixEditorParameter>, val result: String,
                              val documentation: String = "", val bounds: Map<String, String> = emptyMap(),
                              val typeParameters: Set<String> = bounds.keys, val declaredTypes: List<String> = parameters.map { it.type }) {
    val display: String get() = name + parameters.joinToString(", ", "(", ") -> $result") { it.display }
    fun instantiates(template: HixEditorSignature): Boolean = template.typeParameters.isNotEmpty() &&
        typeParameters == template.typeParameters && declaredTypes == template.declaredTypes &&
        bounds.filterKeys { it in typeParameters } == template.bounds.filterKeys { it in template.typeParameters } &&
        parameters.map { it.name } == template.parameters.map { it.name }

    fun parameter(call: HixEditorCall, index: Int): Int {
        val argument = call.arguments.getOrNull(index)
        if (argument?.name != null) return parameters.indexOfFirst {
            it.name == argument.name || it.name == "$" + argument.name || "$" + it.name == argument.name
        }
        val position = index + if (call.chained) 1 else 0
        return if (position < parameters.size) position else if (parameters.lastOrNull()?.variadic == true)
            parameters.lastIndex else -1
    }
    fun accepts(call: HixEditorCall): Boolean {
        if (call.chained && (parameters.isEmpty() ||
                HixCompletionCompatibility.match(call.receiverType, parameters[0].type, bounds) == HixCompletionCompatibility.Match.Impossible)) return false
        val assigned = if (call.chained) hashSetOf(0) else hashSetOf<Int>()
        var named = false
        return call.arguments.withIndex().all { (index, argument) ->
            val target = parameter(call, index)
            val validOrder = argument.name != null || !named
            named = named || argument.name != null
            target >= 0 && validOrder && (parameters[target].variadic || assigned.add(target)) &&
                HixCompletionCompatibility.match(argument.type, parameters[target].type, bounds) != HixCompletionCompatibility.Match.Impossible
        }
    }
}
data class HixEditorArgument(val start: Int, val end: Int, val name: String? = null,
                             val type: String = "any", val text: String = "")
data class HixEditorCall(val name: String, val start: Int, val end: Int, val anchor: Int,
                         val arguments: List<HixEditorArgument>, val chained: Boolean = false,
                         val receiverType: String = "any", val kind: String = "Function",
                         val resultType: String = "any", val source: String = "", val resolved: HixEditorSignature? = null) {
    fun currentArgument(offset: Int): Int {
        arguments.withIndex().firstOrNull { offset <= it.value.end }?.let { return it.index }
        val last = arguments.lastOrNull() ?: return 0
        val trailing = source.substring(last.end.coerceIn(0, source.length), offset.coerceIn(last.end.coerceIn(0, source.length), source.length))
        return if (trailing.contains(',')) arguments.size else arguments.lastIndex
    }
}

internal object HixEditorCalls {
    val CURRENT_CALL = Key.create<HixEditorCall>("hix.parameter.info.call")

    private fun end(rule: ParserRuleContext) = ((rule.stop?.stopIndex ?: rule.start.startIndex - 1) + 1).coerceAtLeast(rule.start.startIndex)
    private fun key(identifier: TerminalNode?, string: TerminalNode?) = identifier?.text ?: string?.text?.removeSurrounding("\"")
    private fun rules(rule: ParserRuleContext): Sequence<ParserRuleContext> = sequence {
        yield(rule)
        rule.children.orEmpty().filterIsInstance<ParserRuleContext>().forEach { yieldAll(rules(it)) }
    }
    private fun type(start: Int, end: Int, text: String, snapshot: MixinFileSnapshot?): String =
        snapshot?.typeFacts?.firstOrNull { it.kind == "Expression" && it.range.startOffset == start && it.range.endOffset == end }?.type
            ?: when {
                text.startsWith('<') || text.startsWith('"') -> "string"
                text.toDoubleOrNull() != null -> "number"
                text == "true" || text == "false" -> "bool"
                text == "null" -> "null"
                text.startsWith("@{") -> "table"
                text.startsWith("@[") || text == "[]" -> "tuple"
                text.startsWith("func") -> "function"
                else -> "any"
            }

    fun hix(source: String, snapshot: MixinFileSnapshot? = null): List<HixEditorCall> {
        // Keep the original recovery tree while an argument or closing delimiter is being typed.
        val parsed = HixAntlrSyntax.parseIncomplete(source)
        fun argument(value: ParserRuleContext, name: String? = null): HixEditorArgument {
            val start = value.start.startIndex.coerceAtLeast(0); val stop = end(value).coerceIn(start, source.length)
            val text = source.substring(start, stop)
            return HixEditorArgument(start, stop, name, type(start, stop, text, snapshot), text)
        }
        return rules(parsed.tree).mapNotNull { owner ->
            val list: HixParser.CallValueListContext?
            val name: String
            var chained = false
            var receiver = "any"
            var kind = "Function"
            var tail: HixParser.TailValueContext? = null
            when (owner) {
                is HixParser.InvocationStatementContext -> { list = owner.callValueList(); name = owner.invocationIdentifier()?.text ?: return@mapNotNull null; tail = owner.tailValue() }
                is HixParser.ValueStatementContext -> { list = owner.callValueList(); name = owner.functionIdentifier()?.text ?: return@mapNotNull null }
                is HixParser.TransformationPartContext -> {
                    list = owner.callValueList(); name = owner.functionIdentifier()?.text ?: return@mapNotNull null; chained = true
                    receiver = HixLookup.receiverType(parsed, owner.start.startIndex, snapshot)
                }
                is HixParser.ToplevelDerivationStatementContext -> {
                    list = owner.callValueList(); name = owner.functionIdentifier()?.text ?: return@mapNotNull null; chained = true
                }
                is HixParser.MetadataContext -> {
                    name = owner.IDENTIFIER()?.text ?: return@mapNotNull null
                    val role = HixLookup.semanticRoleAt(parsed, owner.start.startIndex)
                    kind = when (role) {
                        HixLookup.SemanticRole.FileMetadata -> "FileMetadata"
                        HixLookup.SemanticRole.PatternMetadata -> "PatternMetadata"
                        HixLookup.SemanticRole.TypeMetadata -> "TypeMetadata"
                        else -> "DeclarationMetadata"
                    }
                    val values = owner.valueList()?.value().orEmpty().map { argument(it) } +
                        owner.argumentValue().map { argument(it) } + owner.valueList()?.argumentValue().orEmpty().map { argument(it) }
                    val anchor = owner.valueList()?.start?.startIndex ?: values.firstOrNull()?.start ?: return@mapNotNull null
                    return@mapNotNull HixEditorCall(name, owner.start.startIndex, end(owner), anchor,
                        values.sortedBy { it.start }, kind = kind, source = source)
                }
                else -> return@mapNotNull null
            }
            val arguments = list?.callArgument().orEmpty().mapNotNull { item -> item.value()?.let {
                argument(it, key(item.ROOT_IDENTIFIER(), item.STRING()))
            } } + list?.argumentValue().orEmpty().map { argument(it) } + listOfNotNull(tail?.let { argument(it) })
            val anchor = list?.start?.startIndex ?: arguments.firstOrNull()?.start ?: return@mapNotNull null
            val nameStart = when (owner) {
                is HixParser.ValueStatementContext -> owner.functionIdentifier()?.start?.startIndex
                is HixParser.InvocationStatementContext -> owner.invocationIdentifier()?.start?.startIndex
                is HixParser.TransformationPartContext -> owner.functionIdentifier()?.start?.startIndex
                is HixParser.ToplevelDerivationStatementContext -> owner.functionIdentifier()?.start?.startIndex
                else -> null
            }
            val fact = snapshot?.typeFacts?.firstOrNull { it.kind == "Call" && it.range.startOffset == nameStart }
            var callEnd = end(owner)
            if (list?.BEGIN_PARAMETERS() != null && (list.END_PARAMETERS()?.symbol?.startIndex ?: -1) < 0) {
                while (callEnd < source.length && source[callEnd].isWhitespace()) callEnd++
            }
            HixEditorCall(name, owner.start.startIndex, callEnd, anchor, arguments.sortedBy { it.start }, chained, receiver, kind,
                fact?.type ?: "any", source, fact?.callSignature?.let(::from))
        }.filter { it.anchor >= 0 && it.end >= it.anchor }.toList()
    }

    fun hal(source: String): List<HixEditorCall> = rules(HalAntlrSyntax.parse(source).tree)
        .filterIsInstance<HalParser.CallContext>().mapNotNull { call ->
            val name = call.IDENTIFIER()?.text ?: return@mapNotNull null
            val args = call.argumentList()?.argument().orEmpty().mapNotNull { item -> item.value()?.let { value ->
                val start = value.start.startIndex; val stop = end(value).coerceIn(start.coerceAtLeast(0), source.length)
                if (start < 0) null else HixEditorArgument(start, stop, item.IDENTIFIER()?.text,
                    type(start, stop, value.text, null), source.substring(start, stop))
            } }
            HixEditorCall(name, call.start.startIndex, end(call), call.LPAREN()?.symbol?.startIndex ?: return@mapNotNull null, args, source = source)
        }.toList()

    fun at(calls: List<HixEditorCall>, offset: Int): HixEditorCall? = calls.filter {
        offset >= it.anchor && offset <= it.end &&
            !(offset == it.end && it.source.getOrNull(it.end - 1) == ')')
    }.minByOrNull { it.end - it.anchor }

    fun owner(file: PsiFile, call: HixEditorCall): PsiElement? {
        val leaf = file.findElementAt(call.anchor) ?: return null
        return generateSequence(leaf) { it.parent }.takeWhile { it !is PsiFile }.firstOrNull {
            it.textRange.startOffset == call.anchor && it.textRange.endOffset >= call.end
        } ?: leaf
    }

    fun from(definition: MixinLanguageDefinition) = HixEditorSignature(definition.name,
        definition.argumentTypes.mapIndexed { index, type -> HixEditorParameter(
            definition.argumentNames.getOrNull(index).orEmpty(), type,
            definition.optionalArguments.getOrNull(index) == true, definition.argumentDefaults.getOrNull(index).orEmpty(),
            definition.variadic && index == definition.argumentTypes.lastIndex) }, definition.resultType, definition.documentation,
        definition.typeParameters.mapIndexed { index, name -> name to (definition.typeParameterBounds.getOrNull(index) ?: "any") }.toMap(),
        declaredTypes = definition.declaredArgumentTypes.toList().takeIf { it.isNotEmpty() } ?: definition.argumentTypes.toList())

    fun local(source: String, offset: Int? = null): List<HixEditorSignature> {
        val parsed = HixAntlrSyntax.parseIncomplete(source)
        fun parameter(field: HixParser.PatternFieldContext, index: Int) = HixEditorParameter(
            key(field.ROOT_IDENTIFIER(), field.STRING()).orEmpty(), field.patternUnion()?.text ?: "any",
            field.value() != null || field.metadataList()?.metadata()?.any { it.IDENTIFIER()?.text == "optional" } == true,
            field.value()?.text.orEmpty(), field.metadataList()?.metadata()?.any { it.IDENTIFIER()?.text == "variadic" } == true)
        val mixin = offset?.let { position -> rules(parsed.tree).filterIsInstance<HixParser.MixinDeclarationContext>()
            .firstOrNull { position >= it.start.startIndex && position <= end(it) } }
        val functions = rules(parsed.tree).filterIsInstance<HixParser.FuncDeclarationContext>().mapNotNull { function ->
            val scope = generateSequence(function.parent) { it.parent }.filterIsInstance<HixParser.MixinDeclarationContext>().firstOrNull()
            if (scope != null && scope !== mixin) return@mapNotNull null
            val name = function.functionDeclarationIdentifier()?.text ?: return@mapNotNull null
            val signature = function.directFunctionSignature()
            val params = when {
                signature == null -> listOf(HixEditorParameter("", "any", variadic = true))
                signature.collectiveInputPattern() != null -> listOf(HixEditorParameter("", signature.collectiveInputPattern().patternExpression()?.text ?: "any"))
                else -> signature.patternParameterList()?.patternField()?.mapIndexed { index, field -> parameter(field, index) }.orEmpty()
            }
            val metadata = (function.parent as? HixParser.TopLevelDeclarationContext)?.metadataList()?.metadata().orEmpty()
            val bounds = metadata.filter { it.IDENTIFIER()?.text in setOf("generic", "computedGeneric") }.flatMap { entry ->
                val args = entry.argumentValue().map { it.text.removeSurrounding("<", ">") } + entry.valueList()?.value().orEmpty().map { it.text.removeSurrounding("\"") }
                val bound = args.getOrNull(if (entry.IDENTIFIER().text == "computedGeneric") 2 else 1) ?: "any"
                args.firstOrNull().orEmpty().split(',').map { it.trim() to bound }
            }.toMap()
            val bodyValue = function.functionBody()?.value()
            val result = signature?.patternExpression()?.text ?: if (signature?.outputSignatureFields() != null) "{}" else
                bodyValue?.let { type(it.start.startIndex, end(it), it.text, null) } ?: "any"
            HixEditorSignature(name.removeSurrounding("<", ">"), params, result, bounds = bounds)
        }.toList()
        val constructors = HixLookup.constructors(parsed).flatMap { shape ->
            val signature = HixEditorSignature(shape.name, shape.fields.map { field ->
                HixEditorParameter(field.name, field.type, field.optional || field.defaulted, field.default) }, if (shape.trait) "{}" else shape.name)
            listOf(signature, signature.copy(name = shape.name + ".new")) + if (shape.trait)
                listOf(HixEditorSignature(shape.name, listOf(HixEditorParameter("value", "table")), "{}")) else emptyList()
        }
        return functions + constructors
    }

    private data class FileContext(val source: String, val calls: List<HixEditorCall>,
                                   val definitions: List<MixinLanguageDefinition>, val siblings: List<String>) {
        val signatures = java.util.concurrent.ConcurrentHashMap<Int, List<HixEditorSignature>>()
    }
    private val CONTEXT = Key.create<com.intellij.psi.util.CachedValue<FileContext>>("hix.editor.call.context")
    private fun context(file: PsiFile): FileContext = com.intellij.psi.util.CachedValuesManager.getCachedValue(file, CONTEXT) {
        val source = file.text
        val service = if (file.language == HalLanguage) null else HixSnapshotService.getInstance(file.project)
        val context = FileContext(source,
            if (service == null) hal(source) else hix(source, service.snapshotForText(source, file.virtualFile?.path)),
            service?.definitionsFor(source, file.virtualFile)?.toList().orEmpty(),
            file.virtualFile?.let { service?.sourcesInContext(it) }.orEmpty())
        com.intellij.psi.util.CachedValueProvider.Result.create(context, file,
            com.intellij.psi.util.PsiModificationTracker.MODIFICATION_COUNT,
            service?.modificationTracker ?: com.intellij.openapi.util.ModificationTracker.NEVER_CHANGED)
    }
    fun signatures(file: PsiFile, call: HixEditorCall): List<HixEditorSignature> {
        if (file.language == HalLanguage) return if (call.name == "ref") listOf(
            HixEditorSignature("ref", listOf(HixEditorParameter("section", "string")), "any", "Resolves a local HAL section.")) else emptyList()
        val context = context(file)
        return context.signatures.computeIfAbsent(call.anchor) {
            signatures(context.source, context.definitions, context.siblings, call)
        }
    }

    fun signatures(source: String, definitions: List<MixinLanguageDefinition>, siblings: List<String>, call: HixEditorCall): List<HixEditorSignature> {
        val imported = HixAntlrSyntax.parseIncomplete(source).tree.fileMetadataSection()?.metadataList()?.metadata().orEmpty()
            .filter { it.IDENTIFIER()?.text == "using" }.map { metadata ->
                val args = metadata.argumentValue().map { it.text.removeSurrounding("<", ">") }
                args.firstOrNull().orEmpty() to args.getOrNull(1)
            }
        val names = imported.mapNotNull { (prefix, alias) ->
            if (alias == null) "$prefix.${call.name}" else if (call.name == alias || call.name.startsWith("$alias."))
                prefix + call.name.removePrefix(alias) else null
        } + call.name
        val catalog = definitions.filter { it.kind == call.kind && it.name in names }.map(::from)
        val local = if (call.kind == "Function") (local(source, call.start) + siblings.filter { it != source }.flatMap { local(it) }).filter { it.name in names } else emptyList()
        val shapes = (listOf(source) + siblings.filter { it != source }).flatMap { HixLookup.patterns(HixAntlrSyntax.parse(it)) }
            .associate { it.name to if (it.tuple) "tuple" else "table" }
        val prototypes = (local + catalog).filter { template -> call.resolved?.instantiates(template) != true }
        return (listOfNotNull(call.resolved) + prototypes).map { it.copy(bounds = shapes + it.bounds) }
            .distinctBy { it.display }.sortedByDescending { it.accepts(call) }
    }

    fun calls(file: PsiFile): List<HixEditorCall> = context(file).calls
}

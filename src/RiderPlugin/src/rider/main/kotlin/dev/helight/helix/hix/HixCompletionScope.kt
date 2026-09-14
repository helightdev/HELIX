package dev.helight.helix.hix

import dev.helight.helix.hix.generated.HixParser
import dev.helight.helix.protocol.MixinFileSnapshot
import org.antlr.v4.runtime.ParserRuleContext

internal object HixCompletionScope {
    data class Variable(val name: String, val type: String, val parameter: Boolean)
    private fun contains(rule: ParserRuleContext, offset: Int) = rule.start.startIndex <= offset &&
        (rule.stop?.stopIndex ?: rule.start.startIndex) + 1 >= offset

    fun variables(parsed: HelixAntlrParse, offset: Int, snapshot: MixinFileSnapshot?): List<Variable> {
        val nodes = HixAntlrSyntax.rules(parsed.tree).toList()
        val function = nodes.filterIsInstance<HixParser.FuncDeclarationContext>().filter { contains(it, offset) }
            .minByOrNull { (it.stop?.stopIndex ?: 0) - it.start.startIndex }
        val mixin = nodes.filterIsInstance<HixParser.MixinDeclarationContext>().firstOrNull { contains(it, offset) }
        val parameters = (function?.directFunctionSignature()?.patternParameterList()?.patternField()
            ?: mixin?.patternParameterList()?.patternField()).orEmpty().mapNotNull { field ->
            field.ROOT_IDENTIFIER()?.text?.let { Variable(it, field.patternUnion()?.text ?: "any", true) }
        }
        val locals = nodes.filterIsInstance<HixParser.LocalDeclarationStatementContext>().filter { declaration ->
            declaration.start.startIndex < offset && (declaration.assignedValue()?.stop?.stopIndex ?: declaration.stop.stopIndex) < offset &&
                generateSequence(declaration.parent) { it.parent }.filterIsInstance<ParserRuleContext>()
                    .filter { it is HixParser.StatementBlockContext || it is HixParser.LambdaValueContext || it is HixParser.InlineBlockValueContext }
                    .all { contains(it, offset) }
        }.mapNotNull { declaration ->
            val identifier = declaration.variableIdentifier()?.IDENTIFIER() ?: return@mapNotNull null
            val type = declaration.patternExpression()?.text ?: snapshot?.typeFacts?.firstOrNull {
                it.kind == "Type" && it.range.startOffset == identifier.symbol.startIndex
            }?.type ?: "any"
            Variable(identifier.text, type, false)
        }
        return (parameters + locals).associateBy { it.name }.values.toList()
    }

    fun expectedType(call: HixEditorCall?, signatures: List<HixEditorSignature>, offset: Int): String {
        if (call == null || signatures.isEmpty()) return "any"
        val index = call.currentArgument(offset)
        return signatures.filter { it.accepts(call) }.mapNotNull {
            it.parameters.getOrNull(it.parameter(call, index))?.type
        }.distinct().singleOrNull() ?: "any"
    }
    fun remainingParameters(call: HixEditorCall, signatures: List<HixEditorSignature>, offset: Int): List<HixEditorParameter> {
        val index = call.currentArgument(offset)
        if (call.arguments.getOrNull(index)?.let { it.name != null || it.text.trimStart().startsWith('$') } == true) return emptyList()
        return signatures.filter { it.accepts(call) }.flatMap { signature ->
            val assigned = call.arguments.take(index).indices.map { signature.parameter(call, it) }.toSet() +
                if (call.chained) setOf(0) else emptySet()
            signature.parameters.filterIndexed { position, parameter -> position !in assigned &&
                parameter.name.isNotEmpty() && parameter.name.toIntOrNull() == null && !parameter.variadic }
        }.distinctBy { it.name }
    }
}

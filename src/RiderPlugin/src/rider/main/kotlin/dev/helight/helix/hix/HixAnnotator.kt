package dev.helight.helix.hix

import com.intellij.lang.annotation.AnnotationHolder
import com.intellij.lang.annotation.Annotator
import com.intellij.lang.annotation.HighlightSeverity
import com.intellij.openapi.util.TextRange
import com.intellij.openapi.components.service
import com.intellij.openapi.editor.colors.EditorColorsManager
import java.awt.Font
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile
import dev.helight.helix.hix.generated.HixParser
import dev.helight.helix.protocol.MixinLanguageDefinition
import org.antlr.v4.runtime.Token
import org.antlr.v4.runtime.ParserRuleContext
import org.antlr.v4.runtime.tree.TerminalNode
import org.antlr.v4.runtime.tree.ParseTree

class HixAnnotator : Annotator {
    override fun annotate(element: PsiElement, holder: AnnotationHolder) {
        if (element !is PsiFile || element.language != HixLanguage) return
        val localParse = HixAntlrSyntax.parse(element.text)
        localParse.diagnostics.distinct().forEach { diagnostic ->
            holder.newAnnotation(HighlightSeverity.ERROR, diagnostic.message)
                .range(TextRange(diagnostic.start, diagnostic.end)).create()
        }
        val service = element.project.service<HixSnapshotService>()
        val definitions = service.definitionsFor(element.text)
        addLocalSemanticHighlighting(localParse, element.textLength, holder, definitions)
        element.virtualFile?.let { service.observe(it, element.text) }
        val path = element.virtualFile?.path
        val snapshot = element.getUserData(HixSnapshotService.SEMANTIC_SNAPSHOT)
            ?: service.snapshotForText(element.text, path)
        snapshot?.diagnostics?.distinct()?.forEach { diagnostic ->
            val severity = when (diagnostic.severity.lowercase()) {
                "warning" -> HighlightSeverity.WARNING
                "information", "info" -> HighlightSeverity.INFORMATION
                else -> HighlightSeverity.ERROR
            }
            val start = diagnostic.range.startOffset.coerceIn(0, element.textLength)
            val end = diagnostic.range.endOffset.coerceIn(start, element.textLength)
            holder.newAnnotation(severity, diagnostic.message).range(TextRange(start, end)).create()
        }
        snapshot?.typeFacts?.filter { it.kind == "DynamicCall" }?.forEach { fact ->
            val start = fact.range.startOffset.coerceIn(0, element.textLength)
            val end = fact.range.endOffset.coerceIn(start, element.textLength)
            val attributes = EditorColorsManager.getInstance().globalScheme
                .getAttributes(HixColors.KEYWORD).clone().apply { fontType = Font.ITALIC }
            holder.newSilentAnnotation(HighlightSeverity.INFORMATION).range(TextRange(start, end))
                .enforcedTextAttributes(attributes).create()
        }
    }

    private fun addLocalSemanticHighlighting(parse: HelixAntlrParse, textLength: Int,
                                              holder: AnnotationHolder,
                                              definitions: Array<MixinLanguageDefinition>) {
        fun highlight(node: TerminalNode?, color: com.intellij.openapi.editor.colors.TextAttributesKey) {
            val token = node?.symbol ?: return
            if (token.type == Token.EOF || token.startIndex < 0) return
            val start = token.startIndex.coerceIn(0, textLength)
            val end = (token.stopIndex + 1).coerceIn(start, textLength)
            holder.newSilentAnnotation(HighlightSeverity.INFORMATION).range(TextRange(start, end))
                .textAttributes(color).create()
        }
        fun terminals(node: ParseTree): Sequence<TerminalNode> = sequence {
            if (node is TerminalNode) yield(node)
            else for (index in 0 until node.childCount) yieldAll(terminals(node.getChild(index)))
        }

        val rules = HixAntlrSyntax.rules(parse.tree).toList()
        fun scope(rule: ParserRuleContext): ParserRuleContext? {
            var parent = rule.parent
            while (parent is ParserRuleContext) {
                if (parent is HixParser.FuncDeclarationContext ||
                    parent is HixParser.ExpressionDeclarationContext ||
                    parent is HixParser.LambdaValueContext) return parent
                parent = parent.parent
            }
            return null
        }
        fun hasAncestor(rule: ParserRuleContext, type: Class<out ParserRuleContext>): Boolean {
            var parent = rule.parent
            while (parent is ParserRuleContext) {
                if (type.isInstance(parent)) return true
                parent = parent.parent
            }
            return false
        }
        val parameterNames = rules.filterIsInstance<HixParser.PatternFieldContext>()
            .filter { hasAncestor(it, HixParser.PatternParameterListContext::class.java) }
            .mapNotNull { rule -> rule.ROOT_IDENTIFIER()?.text?.let { name -> scope(rule)?.let { it to name } } }
            .groupBy({ it.first }, { it.second })
        val localNames = rules.filterIsInstance<HixParser.VariableIdentifierContext>()
            .filter { it.parent is HixParser.LocalDeclarationStatementContext }
            .mapNotNull { rule -> scope(rule)?.let { it to rule.text } }
            .groupBy({ it.first }, { it.second })

        fun expectedArgumentType(argument: HixParser.ArgumentValueContext): String? {
            val metadata = generateSequence(argument.parent) { it.parent }
                .filterIsInstance<HixParser.MetadataContext>().firstOrNull()
            if (metadata != null) {
                val index = metadata.valueList()?.argumentValue()?.indexOf(argument) ?: -1
                if (index < 0) return null
                return definitions.asSequence().filter {
                    it.name == metadata.IDENTIFIER()?.text && it.kind in setOf("FileMetadata", "PatternMetadata", "TypeMetadata")
                }.mapNotNull { definition -> definition.argumentTypes.getOrNull(index)
                    ?: if (definition.variadic) definition.argumentTypes.lastOrNull() else null
                }.firstOrNull { it.isTypeParameter() }
            }

            val valueList = generateSequence(argument.parent) { it.parent }
                .filterIsInstance<HixParser.ValueListContext>().firstOrNull() ?: return null
            val directArguments = valueList.argumentValue()
            val index = if (argument in directArguments) directArguments.indexOf(argument) else {
                val value = generateSequence(argument.parent) { it.parent }
                    .filterIsInstance<HixParser.ValueContext>().firstOrNull { it.parent == valueList }
                valueList.value().indexOf(value)
            }
            if (index < 0) return null
            val functionName = when (val owner = valueList.parent) {
                is HixParser.ValueStatementContext -> owner.functionIdentifier().text
                is HixParser.TransformationPartContext -> owner.functionIdentifier()?.text
                is HixParser.InvocationStatementContext -> owner.invocationIdentifier().text
                else -> null
            } ?: return null
            return definitions.asSequence().filter { it.kind == "Function" && it.name == functionName }
                .mapNotNull { definition -> definition.argumentTypes.getOrNull(index)
                    ?: if (definition.variadic) definition.argumentTypes.lastOrNull() else null
                }.firstOrNull { it.isTypeParameter() }
        }

        rules.filterIsInstance<HixParser.ArgumentValueContext>().forEach { argument ->
            if (expectedArgumentType(argument) == null) return@forEach
            val start = (argument.start.startIndex + 1).coerceIn(0, textLength)
            val end = argument.stop.stopIndex.coerceIn(start, textLength)
            if (end > start) holder.newSilentAnnotation(HighlightSeverity.INFORMATION)
                .range(TextRange(start, end)).textAttributes(HixColors.TYPE).create()
        }

        rules.forEach { rule ->
            when (rule) {
                is HixParser.MetadataContext -> {
                    val role = (rule.IDENTIFIER() ?: rule.METADATA_PREFIX())?.symbol?.startIndex?.let {
                        HixLookup.semanticRoleAt(parse, it)
                    }
                    val isPatternMetadata = role == HixLookup.SemanticRole.PatternMetadata
                    val metadataColor = if (isPatternMetadata) HixColors.PATTERN_METADATA else HixColors.METADATA
                    terminals(rule).filter { it.symbol.type in metadataStructureTokens }
                        .forEach { highlight(it, metadataColor) }
                    highlight(rule.IDENTIFIER(), metadataColor)
                }
                is HixParser.TypeDeclarationContext -> highlight(rule.IDENTIFIER(), HixColors.TYPE)
                is HixParser.PatternIdentifierContext -> {
                    highlight(rule.IDENTIFIER(), HixColors.TYPE)
                    highlight(rule.ROOT_IDENTIFIER(), HixColors.TYPE)
                }
                is HixParser.KindIdentifierContext -> {
                    highlight(rule.IDENTIFIER(), HixColors.TYPE)
                    highlight(rule.ROOT_IDENTIFIER(), HixColors.TYPE)
                }
                is HixParser.PatternFieldContext -> highlight(rule.ROOT_IDENTIFIER(),
                    if (hasAncestor(rule, HixParser.PatternParameterListContext::class.java))
                        HixColors.PARAMETER else HixColors.FIELD)
                is HixParser.TableKeyedEntryContext -> highlight(rule.ROOT_IDENTIFIER(), HixColors.FIELD)
                is HixParser.FuncDeclarationContext ->
                    highlight(rule.functionDeclarationIdentifier().IDENTIFIER(), HixColors.FUNCTION_IDENTIFIER)
                is HixParser.MixinIdentifierContext -> {
                    highlight(rule.IDENTIFIER(), HixColors.MIXIN)
                    highlight(rule.NAMESPACE_IDENTIFIER(), HixColors.MIXIN)
                }
                is HixParser.VariableIdentifierContext -> {
                    highlight(rule.IDENTIFIER(), HixColors.LOCAL)
                }
                is HixParser.DerivationRootContext -> {
                    val root = rule.ROOT_IDENTIFIER()
                    val currentScope = scope(rule)
                    when (root?.text) {
                        in parameterNames[currentScope].orEmpty() -> highlight(root, HixColors.PARAMETER)
                        in localNames[currentScope].orEmpty() -> highlight(root, HixColors.LOCAL)
                    }
                }
                is HixParser.InvocationStatementContext ->
                    highlight(rule.invocationIdentifier().IDENTIFIER(), HixColors.FUNCTION)
                is HixParser.FunctionIdentifierContext -> {
                    highlight(rule.FUNCTION_IDENTIFIER(), HixColors.FUNCTION)
                    highlight(rule.ROOT_IDENTIFIER(), HixColors.FUNCTION)
                }
            }
        }
    }

    private companion object {
        fun String.isTypeParameter() = lowercase() in setOf("pattern", "type", "kind")

        val metadataStructureTokens = setOf(
            dev.helight.helix.hix.generated.HixLexer.METADATA_PREFIX,
            dev.helight.helix.hix.generated.HixLexer.BEGIN_METADATA_VALUE,
            dev.helight.helix.hix.generated.HixLexer.BEGIN_ARGUMENT,
            dev.helight.helix.hix.generated.HixLexer.ARGUMENT_END,
            dev.helight.helix.hix.generated.HixLexer.BEGIN_PARAMETERS,
            dev.helight.helix.hix.generated.HixLexer.END_PARAMETERS,
            dev.helight.helix.hix.generated.HixLexer.EMPTY_PARAMETERS,
            dev.helight.helix.hix.generated.HixLexer.VALUE_DELIMITER,
            dev.helight.helix.hix.generated.HixLexer.VALUE_END_INLINE
        )
    }
}

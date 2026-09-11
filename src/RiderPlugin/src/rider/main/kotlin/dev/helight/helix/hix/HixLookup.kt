package dev.helight.helix.hix

import dev.helight.helix.hix.generated.HixLexer
import dev.helight.helix.protocol.MixinFileSnapshot
import dev.helight.helix.protocol.MixinLanguageDefinition
import dev.helight.helix.hix.generated.HixParser
import org.antlr.v4.runtime.tree.TerminalNode

/** Completion uses canonical token identities and compiler facts, without a second type resolver. */
internal object HixLookup {
    enum class SemanticRole { Pattern, PatternMetadata, TypeMetadata, DeclarationMetadata, FileMetadata, Metadata }
    data class MetadataArgument(val name: String, val index: Int)
    data class Site(val chained: Boolean, val member: Boolean, val receiverEnd: Int)

    fun semanticRoleAt(parsed: HelixAntlrParse, offset: Int): SemanticRole? {
        val sourceOffset = offset.coerceIn(0, parsed.source.length)
        val delimiter = parsed.source.indexOf("---")
        if (delimiter >= 0 && sourceOffset < delimiter) {
            val lineStart = parsed.source.lastIndexOfAny(charArrayOf('\n', '\r'),
                (sourceOffset - 1).coerceAtLeast(0)).let { it + 1 }
            val prefix = parsed.source.substring(lineStart, (sourceOffset + 1).coerceAtMost(parsed.source.length))
                .trimStart()
            if (prefix.startsWith('%')) return SemanticRole.FileMetadata
        }
        fun contains(node: TerminalNode?): Boolean {
            val token = node?.symbol ?: return false
            return offset >= token.startIndex && offset <= token.stopIndex + 1
        }
        val role = HixAntlrSyntax.rules(parsed.tree).mapNotNull { rule ->
            when (rule) {
                is HixParser.MetadataContext -> if (contains(rule.IDENTIFIER()) || contains(rule.METADATA_PREFIX())) {
                    var parent = rule.parent
                    while (parent is org.antlr.v4.runtime.ParserRuleContext && parent !is HixParser.FileMetadataSectionContext &&
                        parent !is HixParser.PatternExpressionContext && parent !is HixParser.PatternFieldContext)
                        parent = parent.parent
                    when (parent) {
                        is HixParser.FileMetadataSectionContext -> SemanticRole.FileMetadata
                        is HixParser.PatternExpressionContext, is HixParser.PatternFieldContext -> SemanticRole.PatternMetadata
                        else -> {
                            val declaration = rule.parent?.parent as? HixParser.TopLevelDeclarationContext
                            if (declaration?.typeDeclaration() != null) SemanticRole.TypeMetadata
                            else if (declaration?.funcDeclaration() != null || declaration?.mixinDeclaration() != null)
                                SemanticRole.DeclarationMetadata
                            else
                            if (delimiter >= 0 && offset < delimiter) SemanticRole.FileMetadata else SemanticRole.Metadata
                        }
                    }
                } else null
                is HixParser.PatternIdentifierContext ->
                    if (contains(rule.IDENTIFIER()) || contains(rule.ROOT_IDENTIFIER())) SemanticRole.Pattern else null
                is HixParser.KindIdentifierContext ->
                    if (contains(rule.IDENTIFIER()) || contains(rule.ROOT_IDENTIFIER())) SemanticRole.Pattern else null
                else -> null
            }
        }.firstOrNull()
        if (role != null) return role
        val token = parsed.tokens.firstOrNull { offset >= it.start && offset <= it.end }
        if (token?.type == HixLexer.METADATA_PREFIX) {
            if (delimiter >= 0 && token.start < delimiter) return SemanticRole.FileMetadata
            if (delimiter < 0 && parsed.tree.topLevelDeclaration().isEmpty()) return SemanticRole.FileMetadata
            return SemanticRole.Metadata
        }
        return null
    }

    fun metadataArgumentAt(parsed: HelixAntlrParse, offset: Int): MetadataArgument? =
        HixAntlrSyntax.rules(parsed.tree).filterIsInstance<HixParser.MetadataContext>().mapNotNull { metadata ->
            val name = metadata.IDENTIFIER()?.text ?: return@mapNotNull null
            metadata.valueList()?.argumentValue()?.withIndex()?.firstOrNull { (_, argument) ->
                offset >= argument.start.startIndex && offset <= argument.stop.stopIndex + 1
            }?.let { MetadataArgument(name, it.index) }
        }.firstOrNull()

    fun patternMetadataTargetAt(parsed: HelixAntlrParse, offset: Int): String? {
        val metadata = HixAntlrSyntax.rules(parsed.tree).filterIsInstance<HixParser.MetadataContext>()
            .firstOrNull { rule -> offset >= rule.start.startIndex && offset <= rule.stop.stopIndex + 1 }
            ?: return null
        var parent = metadata.parent
        while (parent is org.antlr.v4.runtime.ParserRuleContext) {
            if (parent is HixParser.PatternFieldContext) return "Field"
            if (parent is HixParser.PatternExpressionContext) return "Pattern"
            parent = parent.parent
        }
        return null
    }

    fun isValueContextAt(parsed: HelixAntlrParse, offset: Int): Boolean {
        val token = parsed.tokens.firstOrNull { offset >= it.start && offset <= it.end }
        if (token?.type in setOf(HixLexer.ROOT_IDENTIFIER, HixLexer.VALUE_SMART_ROOT,
                HixLexer.FUNCTION_IDENTIFIER, HixLexer.MEMBER_IDENTIFIER)) return true
        return HixAntlrSyntax.rules(parsed.tree).any { rule ->
            offset >= rule.start.startIndex && offset <= rule.stop.stopIndex + 1 && when (rule) {
                is HixParser.ValueContext, is HixParser.NonArgumentValueContext,
                is HixParser.PrimaryValueContext, is HixParser.DerivationContext,
                is HixParser.ValueListContext, is HixParser.AssignedValueContext,
                is HixParser.InlineTransformationContext -> true
                else -> false
            }
        }
    }

    fun isStatementBlockAt(parsed: HelixAntlrParse, offset: Int): Boolean =
        HixAntlrSyntax.rules(parsed.tree).any { rule ->
            rule is HixParser.StatementBlockContext && offset >= rule.start.startIndex &&
                offset <= rule.stop.stopIndex + 1
        }

    fun enumValuesAt(parsed: HelixAntlrParse, offset: Int): List<String> {
        fun contains(rule: org.antlr.v4.runtime.ParserRuleContext) =
            offset >= rule.start.startIndex && offset <= rule.stop.stopIndex + 1
        fun values(metadata: Iterable<HixParser.MetadataContext>?): List<String> {
            val enumeration = metadata?.firstOrNull { it.IDENTIFIER()?.text == "enum" }
                ?: return emptyList()
            val list = enumeration.valueList() ?: return emptyList()
            return if (list.argumentValue().isNotEmpty()) list.argumentValue().map { it.text }
            else list.value().map { it.text }
        }

        val local = HixAntlrSyntax.rules(parsed.tree).filterIsInstance<HixParser.LocalDeclarationStatementContext>()
            .filter(::contains).minByOrNull { it.stop.stopIndex - it.start.startIndex } ?: return emptyList()
        values(local.patternExpression()?.inlineMetadataList()?.metadata()?.asIterable()).takeIf { it.isNotEmpty() }?.let { return it }

        val entry = HixAntlrSyntax.rules(local).filterIsInstance<HixParser.TableKeyedEntryContext>()
            .filter(::contains).minByOrNull { it.stop.stopIndex - it.start.startIndex } ?: return emptyList()
        val fieldName = entry.ROOT_IDENTIFIER()?.text ?: return emptyList()
        val typeName = local.patternExpression()?.patternUnion()?.patternTerm()?.firstOrNull()
            ?.patternPrimary()?.patternIdentifier()?.text ?: return emptyList()
        val declaration = HixAntlrSyntax.rules(parsed.tree).filterIsInstance<HixParser.TypeDeclarationContext>()
            .firstOrNull { it.IDENTIFIER()?.text == typeName } ?: return emptyList()
        val field = declaration.patternExpression()?.patternUnion()?.patternTerm()?.firstOrNull()
            ?.patternPrimary()?.tablePattern()?.patternField()
            ?.firstOrNull { it.ROOT_IDENTIFIER()?.text == fieldName }
        return values(field?.metadataList()?.metadata()?.asIterable())
    }
    fun site(parsed: HelixAntlrParse, offset: Int): Site {
        val tokens = parsed.tokens.filter { it.start < offset && it.type !in setOf(0,
            HixLexer.OUTER_WHITESPACE, HixLexer.VALUE_WHITESPACE, HixLexer.METADATA_WHITESPACE,
            HixLexer.COMMENT, HixLexer.SLASH_COMMENT) }
        var previous = tokens.lastOrNull()
        if (previous?.type in setOf(HixLexer.FUNCTION_IDENTIFIER, HixLexer.MEMBER_IDENTIFIER) &&
            previous!!.end >= offset) previous = tokens.getOrNull(tokens.lastIndex - 1)
        return Site(previous?.type in setOf(HixLexer.VALUE_FUNCTION, HixLexer.VALUE_PREDICATE),
            previous?.type == HixLexer.VALUE_MEMBER, previous?.start ?: offset)
    }

    fun receiverType(parsed: HelixAntlrParse, end: Int, snapshot: MixinFileSnapshot?): String {
        val fact = snapshot?.typeFacts?.filter {
            it.range.endOffset <= end && it.range.endOffset >= 0 &&
                parsed.source.substring(it.range.endOffset, end).isBlank() && it.kind != "Coercion"
        }?.maxWithOrNull(compareBy({ it.range.endOffset }, { -it.range.startOffset }))
        if (fact != null && fact.type != "any") return fact.type
        val token = parsed.tokens.lastOrNull { it.end <= end && parsed.source.substring(it.end, end).isBlank() }
        return when (token?.type) {
            HixLexer.NUMBER -> "number"
            HixLexer.BOOLEAN -> "bool"
            HixLexer.NULL -> "null"
            HixLexer.ARGUMENT_END -> "string"
            else -> "any"
        }
    }

    fun acceptsReceiver(definition: MixinLanguageDefinition, receiver: String): Boolean {
        if (definition.kind != "Function" || definition.argumentTypes.isEmpty()) return false
        val expected = definition.argumentTypes[0].lowercase()
        val actual = receiver.lowercase()
        if (actual == "any" || expected == "any" || actual == expected) return true
        if (actual.startsWith("@{")) return expected == "table"
        if (actual.startsWith("@[")) return expected == "tuple"
        // Named patterns and unions remain unknown here; their authoritative resolution belongs to the compiler.
        return actual !in setOf("string", "number", "bool", "null", "symbol", "tuple", "table", "kind", "function", "error", "pattern")
    }

    fun signature(definition: MixinLanguageDefinition): String =
        definition.name + "(" + definition.argumentTypes.mapIndexed { index, type ->
            (if (definition.variadic && index == definition.argumentTypes.lastIndex) "..." else "") + type.lowercase()
        }.joinToString(", ") + ") -> " + definition.resultType.lowercase()
}

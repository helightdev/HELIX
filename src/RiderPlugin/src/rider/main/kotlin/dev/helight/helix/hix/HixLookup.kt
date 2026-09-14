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
    data class ConstructorField(val name: String, val type: String, val optional: Boolean, val defaulted: Boolean, val default: String = "")
    data class Constructor(val name: String, val fields: List<ConstructorField>, val tuple: Boolean, val trait: Boolean = false, val traitName: String = name)
    data class ConstructorCall(val name: String, val assigned: Set<String>)

    fun fieldName(name: String): String = if (Regex("[A-Za-z_][\\w.]*").matches(name)) name
        else "\"" + name.replace("\\", "\\\\").replace("\"", "\\\"") + "\""

    private fun key(identifier: TerminalNode?, quoted: TerminalNode?): String? =
        identifier?.text ?: quoted?.text?.removeSurrounding("\"")?.replace("\\\"", "\"")?.replace("\\\\", "\\")

    /** Local shape projection for completion; diagnostics and executable binding remain compiler-owned. */
    fun patterns(parsed: HelixAntlrParse): List<Constructor> {
        val declarations = HixAntlrSyntax.rules(parsed.tree).filterIsInstance<HixParser.TypeDeclarationContext>()
            .associateBy { it.IDENTIFIER()?.text ?: "" }
        fun resolve(name: String, active: Set<String>): Constructor? {
            if (name in active) return null
            val declaration = declarations[name] ?: return null
            val expression = declaration.patternExpression() ?: return null
            val primary = expression.patternUnion()?.patternTerm()?.singleOrNull()?.patternPrimary() ?: return null
            val tuple = primary.tuplePattern() != null
            val sourceFields = primary.tablePattern()?.patternField() ?: primary.tuplePattern()?.patternField()
            var shape = if (sourceFields != null) Constructor(name, sourceFields.mapIndexed { index, field ->
                ConstructorField(key(field.ROOT_IDENTIFIER(), field.STRING()) ?: if (tuple) index.toString()
                    else field.patternUnion()?.text ?: index.toString(),
                    field.metadataList()?.metadata()?.firstOrNull { it.IDENTIFIER()?.text == "many" }?.let {
                        if (it.argumentValue().isEmpty()) "%many<${field.patternUnion()?.text ?: "any"}>" else it.text
                    } ?: field.patternUnion()?.text ?: "any",
                    field.metadataList()?.metadata()?.any { it.IDENTIFIER()?.text == "optional" } == true,
                    field.value() != null || field.metadataList()?.metadata()?.any { it.IDENTIFIER()?.text == "default" } == true,
                    field.value()?.text.orEmpty())
            }, tuple) else resolve(primary.patternIdentifier()?.text ?: return null, active + name)?.copy(name = name)
                ?: return null
            val directives = (expression.inlineMetadataList()?.metadata() ?: emptyList()) +
                ((declaration.parent as? HixParser.TopLevelDeclarationContext)?.metadataList()?.metadata() ?: emptyList())
            for (directive in directives) {
                val kind = directive.IDENTIFIER()?.text
                if (kind !in setOf("inherit", "trait")) continue
                val target = directive.argumentValue().firstOrNull()?.text?.removeSurrounding("<", ">")
                    ?: directive.valueList()?.value()?.firstOrNull()?.text?.removeSurrounding("\"") ?: continue
                val inherited = resolve(target, active + name) ?: continue
                if (kind == "trait") {
                    if (!shape.tuple && inherited.trait) shape = shape.copy(fields = shape.fields +
                        ConstructorField("$" + inherited.traitName, target, false, false))
                } else if (shape.tuple == inherited.tuple && !inherited.trait) {
                    val fields = if (shape.tuple) shape.fields.mapIndexed { index, field ->
                        val parent = inherited.fields.getOrNull(index)
                        if (parent == null) field else field.copy(optional = field.optional && parent.optional,
                            defaulted = field.defaulted && parent.defaulted)
                    } else {
                        val merged = inherited.fields.associateBy { it.name }.toMutableMap()
                        shape.fields.forEach { field ->
                            val parent = merged[field.name]
                            merged[field.name] = if (parent == null) field else field.copy(
                                optional = (field.optional || field.defaulted) && (parent.optional || parent.defaulted),
                                defaulted = (field.defaulted || parent.defaulted) &&
                                    (field.optional || field.defaulted) && (parent.optional || parent.defaulted),
                                default = if (field.defaulted) field.default else parent.default)
                        }
                        merged.values.toList()
                    }
                    shape = shape.copy(fields = fields)
                }
            }
            return if (declaration.KEYWORD_TRAIT() != null) shape.copy(trait = true, traitName = name) else shape
        }
        return declarations.keys.mapNotNull { resolve(it, emptySet()) }
    }

    fun constructors(parsed: HelixAntlrParse): List<Constructor> = patterns(parsed).map { shape ->
        shape.copy(fields = shape.fields.map { field ->
            if (field.name.startsWith("$") && shape.fields.none { it.name == field.name.drop(1) })
                field.copy(name = field.name.drop(1)) else field
        })
    }

    fun constructorCallAt(parsed: HelixAntlrParse, offset: Int): ConstructorCall? {
        val list = HixAntlrSyntax.rules(parsed.tree).filterIsInstance<HixParser.CallValueListContext>()
            .filter { offset >= it.start.startIndex && offset <= it.stop.stopIndex + 1 }
            .minByOrNull { it.stop.stopIndex - it.start.startIndex } ?: return null
        val name = when (val owner = list.parent) {
            is HixParser.ValueStatementContext -> owner.functionIdentifier().text
            is HixParser.TransformationPartContext -> owner.functionIdentifier()?.text
            is HixParser.InvocationStatementContext -> owner.invocationIdentifier().text
            else -> null
        } ?: return null
        return ConstructorCall(name, list.callArgument().mapNotNull { key(it.ROOT_IDENTIFIER(), it.STRING()) }.toSet())
    }

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
            (metadata.argumentValue() + (metadata.valueList()?.argumentValue() ?: emptyList())).withIndex().firstOrNull { (_, argument) ->
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
            val list = enumeration.valueList()
            val arguments = enumeration.argumentValue() + (list?.argumentValue() ?: emptyList())
            return if (arguments.isNotEmpty()) arguments.map { it.text }
            else list?.value()?.map { it.text } ?: emptyList()
        }

        val local = HixAntlrSyntax.rules(parsed.tree).filterIsInstance<HixParser.LocalDeclarationStatementContext>()
            .filter(::contains).minByOrNull { it.stop.stopIndex - it.start.startIndex } ?: return emptyList()
        values(local.patternExpression()?.inlineMetadataList()?.metadata()?.asIterable()).takeIf { it.isNotEmpty() }?.let { return it }

        val entry = HixAntlrSyntax.rules(local).filterIsInstance<HixParser.TableKeyedEntryContext>()
            .filter(::contains).minByOrNull { it.stop.stopIndex - it.start.startIndex } ?: return emptyList()
        val fieldName = key(entry.ROOT_IDENTIFIER(), entry.STRING()) ?: return emptyList()
        val typeName = local.patternExpression()?.patternUnion()?.patternTerm()?.firstOrNull()
            ?.patternPrimary()?.patternIdentifier()?.text ?: return emptyList()
        val declaration = HixAntlrSyntax.rules(parsed.tree).filterIsInstance<HixParser.TypeDeclarationContext>()
            .firstOrNull { it.IDENTIFIER()?.text == typeName } ?: return emptyList()
        val field = declaration.patternExpression()?.patternUnion()?.patternTerm()?.firstOrNull()
            ?.patternPrimary()?.tablePattern()?.patternField()
            ?.firstOrNull { key(it.ROOT_IDENTIFIER(), it.STRING()) == fieldName }
        return values(field?.metadataList()?.metadata()?.asIterable())
    }
    fun site(parsed: HelixAntlrParse, offset: Int): Site {
        val tokens = parsed.tokens.filter { it.start < offset && it.type !in setOf(0,
            HixLexer.OUTER_WHITESPACE, HixLexer.VALUE_WHITESPACE, HixLexer.METADATA_WHITESPACE,
            HixLexer.COMMENT, HixLexer.SLASH_COMMENT) }
        var previous = tokens.lastOrNull()
        if (previous?.type in setOf(HixLexer.FUNCTION_IDENTIFIER, HixLexer.LABEL_IDENTIFIER, HixLexer.MEMBER_IDENTIFIER) &&
            previous!!.end >= offset) previous = tokens.getOrNull(tokens.lastIndex - 1)
        return Site(previous?.type in setOf(HixLexer.VALUE_FUNCTION, HixLexer.VALUE_PREDICATE, HixLexer.LABEL_PREFIX),
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
        return actual !in setOf("string", "number", "bool", "null", "opaque", "tuple", "table", "function", "error", "pattern")
    }

    fun signature(definition: MixinLanguageDefinition): String =
        if (definition.kind == "Type" || definition.kind == "PrimitivePattern") definition.name else definition.name + "(" + definition.argumentTypes.mapIndexed { index, type ->
            (if (definition.variadic && index == definition.argumentTypes.lastIndex) "..." else "") + type
        }.joinToString(", ") + ") -> " + definition.resultType
}

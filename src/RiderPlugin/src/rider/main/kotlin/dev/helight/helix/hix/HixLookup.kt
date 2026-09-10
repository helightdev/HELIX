package dev.helight.helix.hix

import dev.helight.helix.hix.generated.HixLexer
import dev.helight.helix.protocol.MixinFileSnapshot
import dev.helight.helix.protocol.MixinLanguageDefinition

/** Completion uses canonical token identities and compiler facts, without a second type resolver. */
internal object HixLookup {
    data class Site(val chained: Boolean, val member: Boolean, val receiverEnd: Int)
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

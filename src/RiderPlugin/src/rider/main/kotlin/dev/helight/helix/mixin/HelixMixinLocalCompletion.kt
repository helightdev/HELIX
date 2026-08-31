package dev.helight.helix.mixin

internal data class HelixLocalCompletion(
    val kind: String,
    val replacementStart: Int,
    val replacementEnd: Int,
    val receiverType: String = "Any"
)

internal object HelixMixinLocalCompletionParser {
    fun at(source: CharSequence, rawOffset: Int): HelixLocalCompletion? {
        val offset = rawOffset.coerceIn(0, source.length)
        val lineStart = source.lastIndexOf('\n', (offset - 1).coerceAtLeast(0)).let { if (it < 0) 0 else it + 1 }
        var marker = lineStart
        while (marker < source.length && marker < offset && (source[marker] == ' ' || source[marker] == '\t')) marker++
        if (marker < source.length && source[marker] == '@' && marker < offset) {
            val wordEnd = nameEnd(source, marker + 1)
            if (offset <= wordEnd && source.subSequence(marker + 1, offset).all(::isName))
                return HelixLocalCompletion("Directive", marker + 1, wordEnd)
        }

        val wordStart = nameStart(source, offset)
        val operator = when {
            wordStart >= 3 && source.subSequence(wordStart - 3, wordStart).toString() == ":!?" -> wordStart - 3
            wordStart >= 2 && source.subSequence(wordStart - 2, wordStart).toString() == ":?" -> wordStart - 2
            wordStart >= 1 && source[wordStart - 1] == ':' -> wordStart - 1
            else -> -1
        }
        if (operator >= 0) {
            val predicate = source[operator + 1] == '?' ||
                operator + 2 < source.length && source[operator + 1] == '!' && source[operator + 2] == '?'
            return HelixLocalCompletion(if (predicate) "Predicate" else "Function",
                wordStart, nameEnd(source, wordStart))
        }

        if (wordStart > 0 && source[wordStart - 1] == '@' && wordStart - 1 != marker)
            return HelixLocalCompletion("Root", wordStart, nameEnd(source, wordStart))

        if (wordStart > 0 && source[wordStart - 1] == '#') {
            var referenceStart = wordStart - 2
            while (referenceStart >= lineStart && isName(source[referenceStart])) referenceStart--
            while (referenceStart >= lineStart && source[referenceStart] != '@') referenceStart--
            if (referenceStart >= lineStart) {
                val rootStart = referenceStart + if (referenceStart + 1 < source.length && source[referenceStart + 1] == '(') 2 else 1
                val rootEnd = nameEnd(source, rootStart)
                val kind = when (source.subSequence(rootStart, rootEnd).toString()) {
                    "local" -> "Local"; "var" -> "Variable"; "tar" -> "TargetVariable"; "carry" -> "Carry"; else -> null
                }
                if (kind != null) return HelixLocalCompletion(kind, wordStart, nameEnd(source, wordStart))
            }
        }

        val directiveEnd = if (marker < source.length && source[marker] == '@') nameEnd(source, marker + 1) else -1
        if (directiveEnd > marker + 1 && offset > directiveEnd) {
            var position = directiveEnd; var index = 0
            while (position < source.length && position <= offset && source[position] == '<') {
                val close = findAngleEnd(source, position)
                val contentEnd = if (close < 0) source.length else close - 1
                if (offset in (position + 1)..contentEnd) {
                    val command = source.subSequence(marker + 1, directiveEnd).toString().uppercase()
                    val kind = when {
                        index == 0 && command in setOf("GOTO", "MATCH") -> "Label"
                        index == 0 && command == "INLINE" -> "DeclaredFunction"
                        command == "CALL" && index in 0..1 -> "DeclaredFunction"
                        command in setOf("CODE", "MIXIN") && index == 0 -> "OutputTarget"
                        else -> null
                    }
                    if (kind != null) {
                        val replacementStart = nameStart(source, offset).coerceAtLeast(position + 1)
                        return HelixLocalCompletion(kind, replacementStart, nameEnd(source, replacementStart))
                    }
                }
                if (close < 0) break
                position = close; index++
            }
        }
        return null
    }

    private fun findAngleEnd(source: CharSequence, open: Int): Int {
        var depth = 0
        for (position in open until source.length) when (source[position]) {
            '<' -> depth++; '>' -> if (--depth == 0) return position + 1
        }
        return -1
    }
    private fun nameStart(source: CharSequence, offset: Int): Int { var value = offset
        while (value > 0 && isName(source[value - 1])) value--; return value }
    private fun nameEnd(source: CharSequence, offset: Int): Int { var value = offset
        while (value < source.length && isName(source[value])) value++; return value }
    private fun isName(value: Char): Boolean = value == '_' || value.isLetterOrDigit()
}

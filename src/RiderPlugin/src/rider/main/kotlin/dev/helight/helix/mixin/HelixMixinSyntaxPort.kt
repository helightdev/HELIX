package dev.helight.helix.mixin

/**
 * Direct Kotlin port of the source-only parts of MixinExpressionLexer,
 * MixinExpressionParser, MixinExpressionSyntax, MixinEditorLexer and
 * MixinEditorSyntaxParser. Keep the control flow and ranges aligned with the C# files.
 */
internal data class HelixSourceRange(val start: Int = 0, val end: Int = 0) {
    val length: Int get() = end - start
    val isEmpty: Boolean get() = length == 0
}

internal enum class HelixExpressionTokenKind {
    Literal, At, OpenParenthesis, CloseParenthesis, NullRoot, Identifier, Member, Path,
    Property, Predicate, Negation, ArgumentStart, ArgumentLiteral,
    ArgumentExpressionStart, ArgumentExpressionEnd, ArgumentEnd, Invalid
}

internal data class HelixExpressionToken(
    val kind: HelixExpressionTokenKind, val text: String, val start: Int, val end: Int
)

internal data class HelixLogicalSourceSegment(
    val logicalStart: Int, val sourceStart: Int, val length: Int
)

internal class HelixLogicalSourceLine(
    val physicalLine: Int, val physicalRange: HelixSourceRange
) {
    private val segments = ArrayList<HelixLogicalSourceSegment>()
    private val continuationRanges = ArrayList<HelixSourceRange>()
    var text: String = ""
        private set
    var continuedFromLine: Int = -1
    val continuations: List<HelixSourceRange> get() = continuationRanges
    val sourceEnd: Int get() {
        if (continuationRanges.isEmpty()) return physicalRange.end
        val markerEnd = continuationRanges.last().end
        if (segments.isEmpty()) return markerEnd
        val segment = segments.last()
        return maxOf(markerEnd, segment.sourceStart + segment.length)
    }

    fun setInitial(value: String, sourceStart: Int) {
        text = value
        segments.clear()
        if (text.isNotEmpty()) segments += HelixLogicalSourceSegment(0, sourceStart, text.length)
    }

    fun appendContinuation(separator: String, value: String, sourceStart: Int, range: HelixSourceRange) {
        text += separator
        val logicalStart = text.length
        text += value
        if (value.isNotEmpty()) segments += HelixLogicalSourceSegment(logicalStart, sourceStart, value.length)
        continuationRanges += range
    }

    fun mapRange(logicalRange: HelixSourceRange): HelixSourceRange {
        if (segments.isEmpty()) return physicalRange
        val start = mapPosition(logicalRange.start, false)
        val end = if (logicalRange.isEmpty) start else mapPosition(logicalRange.end, true)
        return HelixSourceRange(start, maxOf(start, end))
    }

    private fun mapPosition(position: Int, end: Boolean): Int {
        for (segment in segments) {
            val segmentEnd = segment.logicalStart + segment.length
            if (position < segment.logicalStart) return segment.sourceStart
            if (position < segmentEnd || end && position == segmentEnd)
                return segment.sourceStart + minOf(segment.length, maxOf(0, position - segment.logicalStart))
        }
        val last = segments.last()
        return last.sourceStart + last.length
    }
}

internal object HelixExpressionLexer {
    fun lexExpression(rawSource: String?): List<HelixExpressionToken> {
        val source = rawSource.orEmpty()
        val tokens = ArrayList<HelixExpressionToken>()
        var position = 0
        var literalStart = 0
        while (position < source.length) {
            if (source[position] != '@') { position++; continue }
            if (position + 1 < source.length && source[position + 1] == '@') {
                addLiteral(tokens, source, literalStart, position)
                tokens += HelixExpressionToken(HelixExpressionTokenKind.Literal, "@", position, position + 2)
                position += 2
                literalStart = position
                continue
            }
            addLiteral(tokens, source, literalStart, position)
            val parsed = lexReference(source, position, tokens)
            if (parsed < 0) {
                tokens += HelixExpressionToken(HelixExpressionTokenKind.Invalid, "", position, position)
                return tokens
            }
            position = parsed
            literalStart = position
        }
        addLiteral(tokens, source, literalStart, source.length)
        return tokens
    }

    private fun lexReference(source: String, initial: Int, tokens: MutableList<HelixExpressionToken>): Int {
        var position = initial
        tokens += HelixExpressionToken(HelixExpressionTokenKind.At, "@", position, ++position)
        val parenthesized = position < source.length && source[position] == '('
        if (parenthesized) {
            tokens += HelixExpressionToken(HelixExpressionTokenKind.OpenParenthesis, "(", position, position + 1)
            position++
        }
        val nullRoot = position < source.length && source[position] == ':'
        if (nullRoot) {
            tokens += HelixExpressionToken(HelixExpressionTokenKind.NullRoot, "", position, position + 1)
            position++
        } else {
            position = lexIdentifier(source, position, HelixExpressionTokenKind.Identifier, tokens)
            if (position < 0) return -1
        }
        if (!nullRoot && position < source.length && source[position] == '#') {
            position++
            position = lexIdentifier(source, position, HelixExpressionTokenKind.Member, tokens)
            if (position < 0) return -1
        }
        if (nullRoot && position < source.length &&
            (isName(source[position]) || source[position] == '!' || source[position] == '?')) {
            position = lexProperty(source, position, tokens)
            if (position < 0) return -1
        }
        while (position < source.length && (source[position] == ':' || source[position] == '#')) {
            val path = source[position++] == '#'
            position = if (path) lexIdentifier(source, position, HelixExpressionTokenKind.Path, tokens)
            else lexProperty(source, position, tokens)
            if (position < 0) return -1
        }
        if (!parenthesized) return position
        if (position >= source.length || source[position] != ')') return -1
        tokens += HelixExpressionToken(HelixExpressionTokenKind.CloseParenthesis, ")", position, position + 1)
        return position + 1
    }

    private fun lexProperty(source: String, initial: Int, tokens: MutableList<HelixExpressionToken>): Int {
        var position = initial
        if (position < source.length && source[position] == '!') {
            tokens += HelixExpressionToken(HelixExpressionTokenKind.Negation, "!", position, position + 1)
            position++
        }
        if (position < source.length && source[position] == '?') {
            tokens += HelixExpressionToken(HelixExpressionTokenKind.Predicate, "?", position, position + 1)
            position++
        }
        position = lexIdentifier(source, position, HelixExpressionTokenKind.Property, tokens)
        if (position < 0) return -1
        while (position < source.length && source[position] == '<') {
            val argumentStart = ++position
            var depth = 1
            while (position < source.length && depth != 0) {
                if (source[position] == '<') depth++ else if (source[position] == '>') depth--
                position++
            }
            if (depth != 0) return -1
            val argumentEnd = position - 1
            tokens += HelixExpressionToken(HelixExpressionTokenKind.ArgumentStart, "<", argumentStart - 1, argumentStart)
            if (argumentEnd - argumentStart >= 2 && source[argumentStart] == '(' && source[argumentEnd - 1] == ')') {
                tokens += HelixExpressionToken(HelixExpressionTokenKind.ArgumentExpressionStart, "(", argumentStart, argumentStart + 1)
                lexExpression(source.substring(argumentStart + 1, argumentEnd - 1)).forEach {
                    tokens += it.copy(start = it.start + argumentStart + 1, end = it.end + argumentStart + 1)
                }
                tokens += HelixExpressionToken(HelixExpressionTokenKind.ArgumentExpressionEnd, ")", argumentEnd - 1, argumentEnd)
            } else {
                tokens += HelixExpressionToken(HelixExpressionTokenKind.ArgumentLiteral,
                    source.substring(argumentStart, argumentEnd), argumentStart, argumentEnd)
            }
            tokens += HelixExpressionToken(HelixExpressionTokenKind.ArgumentEnd, ">", argumentEnd, position)
        }
        return position
    }

    private fun lexIdentifier(source: String, initial: Int, kind: HelixExpressionTokenKind,
                              tokens: MutableList<HelixExpressionToken>): Int {
        var position = initial
        while (position < source.length && isName(source[position])) position++
        if (position == initial) return -1
        tokens += HelixExpressionToken(kind, source.substring(initial, position), initial, position)
        return position
    }

    private fun addLiteral(tokens: MutableList<HelixExpressionToken>, source: String, start: Int, end: Int) {
        if (end > start) tokens += HelixExpressionToken(HelixExpressionTokenKind.Literal,
            source.substring(start, end), start, end)
    }

    fun getLogicalSourceLines(rawSource: String?): List<HelixLogicalSourceLine> {
        val source = rawSource.orEmpty()
        val physical = getPhysicalSourceLines(source)
        val logical = physical.mapIndexed { index, range -> HelixLogicalSourceLine(index, range) }
        var continuedLine = -1
        for (index in physical.indices) {
            val range = physical[index]
            var contentEnd = range.end
            while (contentEnd > range.start && (source[contentEnd - 1] == '\r' || source[contentEnd - 1] == '\n')) contentEnd--
            val text = source.substring(range.start, contentEnd)
            val marker = skipWhitespace(text, 0)
            if (marker + 1 < text.length && text[marker] == '@') {
                val kind = text[marker + 1]
                if (kind == '#') {
                    logical[index].setInitial("", range.start)
                    continuedLine = -1
                    continue
                }
                if (kind == '\\' || kind == '+') {
                    if (continuedLine >= 0) {
                        val suffixStart = marker + 2
                        logical[continuedLine].appendContinuation(if (kind == '\\') "\n" else "",
                            text.substring(suffixStart), range.start + suffixStart,
                            HelixSourceRange(range.start + marker, range.start + marker + 2))
                        logical[index].setInitial("", range.start)
                        logical[index].continuedFromLine = continuedLine
                    } else logical[index].setInitial(text, range.start)
                    continue
                }
            }
            logical[index].setInitial(text, range.start)
            continuedLine = if (marker + 1 < text.length && text[marker] == '@' &&
                (text[marker + 1].isLetter() || text[marker + 1] == '_')) index else -1
        }
        return logical
    }

    private fun getPhysicalSourceLines(source: String): List<HelixSourceRange> {
        val ranges = ArrayList<HelixSourceRange>()
        var start = 0
        var position = 0
        while (position < source.length) {
            if (source[position] != '\r' && source[position] != '\n') { position++; continue }
            if (source[position] == '\r' && position + 1 < source.length && source[position + 1] == '\n') position++
            ranges += HelixSourceRange(start, position + 1)
            start = ++position
        }
        ranges += HelixSourceRange(start, source.length)
        return ranges
    }

    private fun skipWhitespace(text: String, initial: Int): Int {
        var position = initial
        while (position < text.length && text[position].isWhitespace()) position++
        return position
    }

    private fun isName(value: Char) = value == '_' || value.isLetterOrDigit()
}

internal enum class HelixExpressionRoot {
    Target, This, Attribute, Argument, Variable, TargetVariable, Local, True, False, Null, Table, Parameter, Carry
}

internal data class HelixPropertyArgumentSyntax(
    val literal: String?, val valueExpression: List<HelixExpressionValue>?,
    val booleanExpression: List<HelixExpressionReference?>?, val sourceRange: HelixSourceRange
)
internal data class HelixExpressionProperty(
    val name: String, val parsedArguments: List<HelixPropertyArgumentSyntax>, val negated: Boolean,
    val sourceRange: HelixSourceRange, val nameRange: HelixSourceRange
)
internal data class HelixExpressionReference(
    val root: HelixExpressionRoot, val member: String?, val properties: List<HelixExpressionProperty>,
    val parenthesized: Boolean, val sourceRange: HelixSourceRange,
    val rootRange: HelixSourceRange, val memberRange: HelixSourceRange
)
internal data class HelixExpressionValue(
    val literal: String?, val reference: HelixExpressionReference?, val sourceRange: HelixSourceRange
)

internal object HelixExpressionParser {
    fun parseExpressionValues(text: String): List<HelixExpressionValue> =
        parseValueExpression(HelixExpressionLexer.lexExpression(text), text)

    private fun parseValueExpression(tokens: List<HelixExpressionToken>, source: String?): List<HelixExpressionValue> {
        val result = ArrayList<HelixExpressionValue>()
        var position = 0
        while (position < tokens.size) {
            val token = tokens[position]
            if (token.kind == HelixExpressionTokenKind.Literal) {
                result += HelixExpressionValue(token.text, null, HelixSourceRange(token.start, token.end))
                position++
                continue
            }
            val parsed = parseReference(tokens, position)
            if (parsed == null) {
                val referenceStart = position++
                while (position < tokens.size && tokens[position].kind != HelixExpressionTokenKind.At &&
                    tokens[position].kind != HelixExpressionTokenKind.Literal) position++
                val end = if (position < tokens.size) tokens[position].start else
                    source?.length ?: tokens[referenceStart].end
                val literal = if (source == null) tokens[referenceStart].text else
                    source.substring(tokens[referenceStart].start.coerceAtMost(source.length), end.coerceAtMost(source.length))
                result += HelixExpressionValue(literal, null, HelixSourceRange(tokens[referenceStart].start, end))
                continue
            }
            result += HelixExpressionValue(null, parsed.first, parsed.first.sourceRange)
            position = parsed.second
        }
        if (result.isEmpty()) result += HelixExpressionValue("", null, HelixSourceRange())
        return result
    }

    private fun parseReference(tokens: List<HelixExpressionToken>, initial: Int): Pair<HelixExpressionReference, Int>? {
        var position = initial
        val at = tokens.getOrNull(position)?.takeIf { it.kind == HelixExpressionTokenKind.At } ?: return null
        position++
        val parenthesized = tokens.getOrNull(position)?.kind == HelixExpressionTokenKind.OpenParenthesis
        if (parenthesized) position++
        val rootToken = tokens.getOrNull(position) ?: return null
        val root = if (rootToken.kind == HelixExpressionTokenKind.NullRoot) HelixExpressionRoot.Null
        else if (rootToken.kind == HelixExpressionTokenKind.Identifier) parseRoot(rootToken.text) ?: return null
        else return null
        position++
        val memberToken = tokens.getOrNull(position)?.takeIf { it.kind == HelixExpressionTokenKind.Member }
        if (memberToken != null) position++
        val properties = ArrayList<HelixExpressionProperty>()
        while (position < tokens.size) {
            val path = tokens.getOrNull(position)?.takeIf { it.kind == HelixExpressionTokenKind.Path }
            if (path != null) {
                position++
                properties += HelixExpressionProperty("path",
                    listOf(HelixPropertyArgumentSyntax(path.text, null, null, HelixSourceRange(path.start, path.end))), false,
                    HelixSourceRange(path.start - 1, path.end), HelixSourceRange(path.start, path.end))
                continue
            }
            val propertyStartPosition = position
            val negated = tokens.getOrNull(position)?.kind == HelixExpressionTokenKind.Negation
            if (negated) position++
            val predicate = tokens.getOrNull(position)?.kind == HelixExpressionTokenKind.Predicate
            if (predicate) position++
            val property = tokens.getOrNull(position)?.takeIf { it.kind == HelixExpressionTokenKind.Property }
            if (property == null) break
            position++
            val arguments = ArrayList<HelixPropertyArgumentSyntax>()
            while (tokens.getOrNull(position)?.kind == HelixExpressionTokenKind.ArgumentStart) {
                position++
                val parsed = parseArgument(tokens, position, property.text) ?: return null
                arguments += parsed.first
                position = parsed.second
            }
            val propertyStart = tokens.getOrNull(propertyStartPosition)?.start?.minus(1) ?: property.start - 1
            val propertyEnd = tokens.getOrNull(position - 1)?.end ?: property.end
            properties += HelixExpressionProperty(if (predicate && property.text == "type") "typeSymbol" else property.text,
                arguments, negated, HelixSourceRange(propertyStart, propertyEnd), HelixSourceRange(property.start, property.end))
        }
        if (tokens.getOrNull(position)?.kind == HelixExpressionTokenKind.CloseParenthesis) position++
        if (tokens.getOrNull(position)?.kind == HelixExpressionTokenKind.Invalid) return null
        val referenceEnd = tokens.getOrNull(position - 1)?.end ?: at.end
        return HelixExpressionReference(root, memberToken?.text, properties, parenthesized,
            HelixSourceRange(at.start, referenceEnd), HelixSourceRange(rootToken.start, rootToken.end),
            memberToken?.let { HelixSourceRange(it.start, it.end) } ?: HelixSourceRange()) to position
    }

    private fun parseArgument(tokens: List<HelixExpressionToken>, initial: Int,
                              property: String): Pair<HelixPropertyArgumentSyntax, Int>? {
        var position = initial
        val literal = tokens.getOrNull(position)?.takeIf { it.kind == HelixExpressionTokenKind.ArgumentLiteral }
        if (literal != null) {
            position++
            val end = tokens.getOrNull(position)?.takeIf { it.kind == HelixExpressionTokenKind.ArgumentEnd } ?: return null
            return HelixPropertyArgumentSyntax(literal.text, null, null,
                HelixSourceRange(literal.start - 1, end.end)) to (position + 1)
        }
        val expressionStart = tokens.getOrNull(position)
            ?.takeIf { it.kind == HelixExpressionTokenKind.ArgumentExpressionStart } ?: return null
        position++
        val start = position
        var depth = 1
        while (position < tokens.size && depth != 0) {
            if (tokens[position].kind == HelixExpressionTokenKind.ArgumentExpressionStart) depth++
            else if (tokens[position].kind == HelixExpressionTokenKind.ArgumentExpressionEnd) depth--
            if (depth != 0) position++
        }
        if (depth != 0) return null
        val expressionTokens = tokens.subList(start, position)
        val valueExpression = if (property == "and" || property == "or") null
            else parseValueExpression(expressionTokens, null)
        val booleanExpression = if (property == "and" || property == "or")
            parseBooleanExpression(expressionTokens) else null
        position++
        val expressionEnd = tokens.getOrNull(position)?.takeIf { it.kind == HelixExpressionTokenKind.ArgumentEnd } ?: return null
        return HelixPropertyArgumentSyntax(null, valueExpression, booleanExpression,
            HelixSourceRange(expressionStart.start - 1, expressionEnd.end)) to (position + 1)
    }

    private fun parseBooleanExpression(tokens: List<HelixExpressionToken>): List<HelixExpressionReference?> {
        val result = ArrayList<HelixExpressionReference?>(); var position = 0
        while (position < tokens.size) {
            if (tokens[position].kind == HelixExpressionTokenKind.Literal && tokens[position].text.isBlank()) {
                position++; continue
            }
            val parsed = parseReference(tokens, position)
            if (parsed == null) { result += null; break }
            result += parsed.first; position = parsed.second
        }
        return result
    }

    private fun parseRoot(keyword: String): HelixExpressionRoot? = when (keyword) {
        "target" -> HelixExpressionRoot.Target; "this" -> HelixExpressionRoot.This
        "attr" -> HelixExpressionRoot.Attribute; "arg" -> HelixExpressionRoot.Argument
        "var" -> HelixExpressionRoot.Variable; "tar" -> HelixExpressionRoot.TargetVariable
        "local" -> HelixExpressionRoot.Local; "true" -> HelixExpressionRoot.True
        "false" -> HelixExpressionRoot.False; "null" -> HelixExpressionRoot.Null
        "table" -> HelixExpressionRoot.Table; "param" -> HelixExpressionRoot.Parameter
        "carry" -> HelixExpressionRoot.Carry; else -> null
    }
}

internal enum class HelixEditorTokenKind {
    Directive, Value, Path, Function, ArgumentDelimiter, DirectiveArgumentDelimiter,
    ExpressionArgumentDelimiter, Argument, Operator, Parenthesis, EnclosedReferenceParenthesis,
    Escape, Continuation, Comment, Whitespace, TextWhitespace, NewLine, Text, Invalid
}
internal data class HelixEditorToken(val kind: HelixEditorTokenKind, val start: Int, val end: Int)

internal object HelixEditorLexer {
    fun lex(source: CharSequence): List<HelixEditorToken> {
        val result = ArrayList<HelixEditorToken>()
        var position = 0; var lineStart = true; var expectFunction = false; var expectEnclosedRoot = false
        var argumentDepth = 0; var referenceActive = false; var directiveHeader = false
        var textContentActive = false
        val argumentReferences = ArrayDeque<Boolean>(); val directiveArguments = ArrayDeque<Boolean>()
        var enclosedReferenceDepth = 0
        while (position < source.length) {
            val start = position; val current = source[position]
            if (current == '\r' || current == '\n') {
                if (current == '\r' && position + 1 < source.length && source[position + 1] == '\n') position++
                position++; result += HelixEditorToken(HelixEditorTokenKind.NewLine, start, position)
                lineStart = true; expectFunction = false; expectEnclosedRoot = false; referenceActive = false
                directiveHeader = false; textContentActive = false; enclosedReferenceDepth = 0; continue
            }
            if (current == ' ' || current == '\t') {
                while (position < source.length && (source[position] == ' ' || source[position] == '\t')) position++
                val beginsOperand = argumentDepth == 0 && directiveHeader
                val kind = if (!lineStart && (argumentDepth > 0 || textContentActive))
                    HelixEditorTokenKind.TextWhitespace else HelixEditorTokenKind.Whitespace
                result += HelixEditorToken(kind, start, position)
                if (argumentDepth == 0) {
                    referenceActive = false
                    directiveHeader = false
                    if (beginsOperand) textContentActive = true
                }
                continue
            }
            if (lineStart && startsWith(source, position, "@#")) {
                while (position < source.length && source[position] != '\r' && source[position] != '\n') position++
                result += HelixEditorToken(HelixEditorTokenKind.Comment, start, position); lineStart = false; continue
            }
            if (lineStart && (startsWith(source, position, "@\\") || startsWith(source, position, "@+"))) {
                position += 2; result += HelixEditorToken(HelixEditorTokenKind.Continuation, start, position)
                // A continuation is removed before expression parsing, so a suffix beginning
                // with ':' or '#' continues the receiver from the preceding physical line.
                // Keeping it inactive here makes the first '<' plain text and, worse, lets its
                // closing '>' consume an outer argument depth. Every following physical line is
                // then tokenized in the wrong state.
                referenceActive = true
                textContentActive = true
                lineStart = false
                continue
            }
            if (startsWith(source, position, "@@")) {
                position += 2; result += HelixEditorToken(HelixEditorTokenKind.Escape, start, position); lineStart = false; continue
            }
            if (startsWith(source, position, "@(")) {
                position++; result += HelixEditorToken(HelixEditorTokenKind.Value, start, position)
                result += HelixEditorToken(HelixEditorTokenKind.EnclosedReferenceParenthesis, position, ++position)
                enclosedReferenceDepth++; expectEnclosedRoot = true; referenceActive = true; lineStart = false; continue
            }
            if ((current == ')' && enclosedReferenceDepth > 0) ||
                (current == '<' && (referenceActive || directiveHeader)) || (current == '>' && argumentDepth > 0)) {
                position++
                val directiveArgument = if (current == '<') directiveHeader && !referenceActive else
                    current == '>' && directiveArguments.last()
                result += HelixEditorToken(if (current == ')') HelixEditorTokenKind.EnclosedReferenceParenthesis
                    else if (directiveArgument) HelixEditorTokenKind.DirectiveArgumentDelimiter
                    else HelixEditorTokenKind.ExpressionArgumentDelimiter, start, position)
                if (current == '<') { argumentReferences.addLast(referenceActive); directiveArguments.addLast(directiveArgument)
                    argumentDepth++; referenceActive = false }
                else if (current == '>' && argumentDepth > 0) { argumentDepth--; referenceActive = argumentReferences.removeLast(); directiveArguments.removeLast() }
                else { enclosedReferenceDepth--; referenceActive = false }
                lineStart = false; continue
            }
            if (referenceActive && (startsWith(source, position, ":!?") || startsWith(source, position, ":?") || current == ':')) {
                position += if (startsWith(source, position, ":!?")) 3 else if (startsWith(source, position, ":?")) 2 else 1
                result += HelixEditorToken(HelixEditorTokenKind.Operator, start, position)
                expectFunction = true; expectEnclosedRoot = false; lineStart = false; continue
            }
            if (referenceActive && current == '#') {
                position++; while (position < source.length && isName(source[position])) position++
                result += HelixEditorToken(HelixEditorTokenKind.Path, start, position); lineStart = false; continue
            }
            if (current == '@' && position + 1 < source.length && isName(source[position + 1])) {
                val directive = lineStart; position += 2
                while (position < source.length && isName(source[position])) position++
                result += HelixEditorToken(if (directive) HelixEditorTokenKind.Directive else HelixEditorTokenKind.Value, start, position)
                directiveHeader = directive; referenceActive = !directive; lineStart = false; continue
            }
            if (expectFunction && isName(current)) {
                position++; while (position < source.length && isName(source[position])) position++
                result += HelixEditorToken(HelixEditorTokenKind.Function, start, position)
                expectFunction = false; lineStart = false; continue
            }
            if (expectEnclosedRoot && isName(current)) {
                position++; while (position < source.length && isName(source[position])) position++
                result += HelixEditorToken(HelixEditorTokenKind.Value, start, position)
                expectEnclosedRoot = false; lineStart = false; continue
            }
            while (position < source.length && source[position] !in "@#:<>)\r\n \t") position++
            if (position == start) position++
            result += HelixEditorToken(if (argumentDepth > 0) HelixEditorTokenKind.Argument else HelixEditorTokenKind.Text, start, position)
            referenceActive = false; lineStart = false
        }
        return result
    }
    private fun startsWith(source: CharSequence, position: Int, value: String) =
        position + value.length <= source.length && value.indices.all { source[position + it] == value[it] }
    private fun isName(value: Char) = value == '_' || value.isLetterOrDigit()
}

internal data class HelixLocalToken(val kind: String, val start: Int, val end: Int)
internal object HelixMixinLocalLexer {
    fun lex(source: CharSequence): List<HelixLocalToken> = HelixEditorLexer.lex(source).map {
        HelixLocalToken(it.kind.name, it.start, it.end)
    }
    fun lexExpression(source: CharSequence): List<HelixLocalToken> = lex(source)
}

internal data class HelixLocalNode(
    val kind: String, val start: Int, val end: Int,
    val children: MutableList<HelixLocalNode> = ArrayList()
)

internal object HelixMixinLocalParser {
    fun parse(source: CharSequence): HelixLocalNode = HelixEditorSyntaxParser.parse(source.toString())
}

internal data class HelixFrontendParse(
    val source: String, val tokens: List<HelixLocalToken>, val syntax: HelixLocalNode
)

/** Shares the one immutable local language pass used by IntelliJ's lexer and parser. */
internal object HelixMixinFrontendParseCache {
    @Volatile private var latest: HelixFrontendParse? = null

    fun parse(source: CharSequence): HelixFrontendParse {
        val text = source.toString()
        latest?.takeIf { it.source == text }?.let { return it }
        return synchronized(this) {
            latest?.takeIf { it.source == text } ?: HelixFrontendParse(
                text, HelixMixinLocalLexer.lex(text), HelixMixinLocalParser.parse(text)
            ).also { latest = it }
        }
    }
}

internal object HelixEditorSyntaxParser {
    fun parse(source: String): HelixLocalNode {
        val logicalLines = HelixExpressionLexer.getLogicalSourceLines(source)
        val children = ArrayList<HelixLocalNode>()
        for (line in logicalLines) {
            if (line.continuedFromLine >= 0) continue
            children += if (line.continuations.isEmpty()) projectSourceLine(source, line.physicalRange)
            else projectLogicalLine(line)
        }
        return HelixLocalNode("Document", 0, source.length, children)
    }

    private fun projectSourceLine(source: String, range: HelixSourceRange): HelixLocalNode {
        var contentEnd = range.end
        while (contentEnd > range.start && (source[contentEnd - 1] == '\r' || source[contentEnd - 1] == '\n')) contentEnd--
        var marker = range.start
        while (marker < contentEnd && (source[marker] == ' ' || source[marker] == '\t')) marker++
        if (marker == contentEnd) return HelixLocalNode("Operand", range.start, range.end)
        if (startsWith(source, marker, "@#")) return HelixLocalNode("Comment", range.start, range.end)
        if (startsWith(source, marker, "@\\") || startsWith(source, marker, "@+")) return HelixLocalNode(
            "Continuation", range.start, range.end, projectContinuationExpression(source, marker + 2, contentEnd).toMutableList())
        if (source[marker] != '@') return HelixLocalNode("Error", range.start, range.end)
        var nameEnd = marker + 1
        while (nameEnd < contentEnd && (source[nameEnd].isLetter() || source[nameEnd] == '_')) nameEnd++
        if (nameEnd == marker + 1) return HelixLocalNode("Error", range.start, range.end)
        val children = arrayListOf(HelixLocalNode("DirectiveName", marker, nameEnd))
        val command = source.substring(marker + 1, nameEnd).uppercase()
        var position = nameEnd; var argumentIndex = 0
        while (position < contentEnd && source[position] == '<') {
            val close = findDelimitedEnd(source, position, contentEnd)
            if (close < 0) { children += HelixLocalNode("Error", position, contentEnd); position = contentEnd; break }
            val innerStart = position + 1; val innerEnd = close - 1
            val argumentChildren = if (innerEnd - innerStart >= 2 && source[innerStart] == '(' && source[innerEnd - 1] == ')')
                projectExpression(source, innerStart + 1, innerEnd - 1).toMutableList() else ArrayList()
            children += HelixLocalNode(directiveArgumentKind(command, argumentIndex++), position, close, argumentChildren)
            position = close
        }
        while (position < contentEnd && (source[position] == ' ' || source[position] == '\t')) position++
        if (position < contentEnd) children += HelixLocalNode("Operand", position, contentEnd,
            projectExpression(source, position, contentEnd).toMutableList())
        return HelixLocalNode("Directive", range.start, range.end, children)
    }

    private fun projectLogicalLine(line: HelixLogicalSourceLine): HelixLocalNode {
        val logical = projectSourceLine(line.text, HelixSourceRange(0, line.text.length))
        val entries = ArrayList<IntervalEntry>(); var order = 0
        fun flatten(node: HelixLocalNode, depth: Int) {
            entries += IntervalEntry(mapNode(node, line, false), depth, order++)
            node.children.forEach { flatten(it, depth + 1) }
        }
        logical.children.forEach { flatten(it, 1) }
        line.continuations.forEach { entries += IntervalEntry(HelixLocalNode("Continuation", it.start, it.end), 1, order++) }
        return rebuild(logical.kind, HelixSourceRange(line.physicalRange.start, line.sourceEnd), entries)
    }

    private fun mapNode(node: HelixLocalNode, line: HelixLogicalSourceLine, children: Boolean = true): HelixLocalNode {
        val range = line.mapRange(HelixSourceRange(node.start, node.end))
        return HelixLocalNode(node.kind, range.start, range.end,
            if (children) node.children.map { mapNode(it, line) }.toMutableList() else ArrayList())
    }

    private data class IntervalEntry(val node: HelixLocalNode, val depth: Int, val order: Int)
    private fun rebuild(kind: String, range: HelixSourceRange, entries: List<IntervalEntry>): HelixLocalNode {
        val root = HelixLocalNode(kind, range.start, range.end); val stack = ArrayDeque<HelixLocalNode>(); stack.addLast(root)
        entries.sortedWith(compareBy<IntervalEntry> { it.node.start }.thenByDescending { it.node.end }
            .thenBy { it.depth }.thenBy { it.order }).forEach { entry ->
            val child = HelixLocalNode(entry.node.kind, entry.node.start, entry.node.end)
            while (stack.size > 1 && !contains(stack.last(), child)) stack.removeLast()
            if (contains(stack.last(), child)) { stack.last().children += child; stack.addLast(child) }
        }
        return root
    }

    private fun projectExpression(source: String, start: Int, end: Int): List<HelixLocalNode> {
        if (end <= start) return emptyList()
        val text = source.substring(start, end); val result = ArrayList<HelixLocalNode>()
        HelixExpressionParser.parseExpressionValues(text).mapNotNullTo(result) { it.reference?.let { ref -> projectReference(ref, start) } }
        HelixExpressionLexer.lexExpression(text).filter { it.kind == HelixExpressionTokenKind.Literal && it.text == "@" && it.end - it.start == 2 }
            .forEach { result += HelixLocalNode("Escape", start + it.start, start + it.end) }
        HelixExpressionLexer.lexExpression(text).filter { it.kind == HelixExpressionTokenKind.Invalid }.forEach {
            result += HelixLocalNode("Error", start + it.start, start + minOf(text.length, maxOf(it.end, it.start + 1)))
        }
        return result.sortedBy { it.start }
    }

    private fun projectContinuationExpression(source: String, start: Int, end: Int): List<HelixLocalNode> {
        val result = projectExpression(source, start, end).toMutableList(); var suffixStart = start
        while (suffixStart < end && (source[suffixStart] == ' ' || source[suffixStart] == '\t')) suffixStart++
        if (suffixStart >= end || (source[suffixStart] != ':' && source[suffixStart] != '#')) return result
        val syntheticRoot = if (source[suffixStart] == '#') "@this#_" else "@null"
        val value = HelixExpressionParser.parseExpressionValues(syntheticRoot + source.substring(suffixStart, end))
            .firstOrNull { it.reference != null }?.reference ?: return result
        if (value.sourceRange.start != 0) return result
        result += projectReference(value, suffixStart - syntheticRoot.length).children
            .filter { it.kind == "Path" || it.kind == "FunctionCall" }
        return result.sortedBy { it.start }
    }

    private fun projectReference(reference: HelixExpressionReference, offset: Int): HelixLocalNode {
        val children = ArrayList<HelixLocalNode>()
        children += HelixLocalNode("Root", offset + reference.rootRange.start, offset + reference.rootRange.end)
        if (reference.member != null) children += HelixLocalNode("Member",
            offset + reference.memberRange.start, offset + reference.memberRange.end)
        for (property in reference.properties) {
            val propertyChildren = ArrayList<HelixLocalNode>()
            if (property.name != "path") for (argument in property.parsedArguments) {
                val kind = if (argument.literal == null) "ExpressionArgument" else "LiteralArgument"
                propertyChildren += HelixLocalNode(kind, offset + argument.sourceRange.start, offset + argument.sourceRange.end,
                    if (argument.literal == null) argument.valueExpression.orEmpty().mapNotNull {
                        it.reference?.let { ref -> projectReference(ref, offset) }
                    }.toMutableList() else ArrayList())
            }
            children += HelixLocalNode(if (property.name == "path") "Path" else "FunctionCall",
                offset + property.sourceRange.start, offset + property.sourceRange.end, propertyChildren)
        }
        return HelixLocalNode(if (reference.parenthesized) "ParenthesizedReference" else "Reference",
            offset + reference.sourceRange.start, offset + reference.sourceRange.end, children)
    }

    private fun directiveArgumentKind(command: String, index: Int): String {
        val declares = index == 0 && command in setOf("FUNC", "SCOPE", "LABEL", "LOCAL", "VAR", "TAR", "CARRY", "ANNOTATION", "DERIVATION", "DEFINE_TARGET")
        val references = (index == 0 && command in setOf("GOTO", "MATCH", "INLINE", "ANNOTATION", "DERIVATION")) || command == "CALL" && index in 0..1
        return when { declares && references -> "DeclarationReferenceDirectiveArgument"; declares -> "DeclarationDirectiveArgument"
            references -> "ReferenceDirectiveArgument"; else -> "DirectiveArgument" }
    }
    private fun findDelimitedEnd(source: String, start: Int, limit: Int): Int { var depth = 0
        for (position in start until limit) { if (source[position] == '<') depth++ else if (source[position] == '>' && --depth == 0) return position + 1 }; return -1 }
    private fun startsWith(source: String, position: Int, value: String) = source.startsWith(value, position)
    private fun contains(outer: HelixLocalNode, inner: HelixLocalNode) = inner.start >= outer.start && inner.end <= outer.end
}

package dev.helight.helix.expression

import com.intellij.lexer.LexerBase
import com.intellij.psi.tree.IElementType

class HelixExpressionLexer : LexerBase() {
    private var buffer: CharSequence = ""
    private var end = 0
    private var start = 0
    private var tokenEnd = 0
    private var type: IElementType? = null
    private var lineStart = true
    private var expectFunction = false
    private var enclosed = false

    override fun start(buffer: CharSequence, startOffset: Int, endOffset: Int, initialState: Int) {
        this.buffer = buffer
        end = endOffset
        start = startOffset
        tokenEnd = startOffset
        lineStart = initialState and LINE_STARTED == 0
        expectFunction = initialState and EXPECT_FUNCTION != 0
        enclosed = initialState and ENCLOSED != 0
        locateToken()
    }

    override fun getState(): Int = (if (!lineStart) LINE_STARTED else 0) or
        (if (expectFunction) EXPECT_FUNCTION else 0) or (if (enclosed) ENCLOSED else 0)
    override fun getTokenType(): IElementType? = type
    override fun getTokenStart(): Int = start
    override fun getTokenEnd(): Int = tokenEnd
    override fun getBufferSequence(): CharSequence = buffer
    override fun getBufferEnd(): Int = end

    override fun advance() {
        start = tokenEnd
        locateToken()
    }

    private fun locateToken() {
        if (start >= end) {
            type = null
            return
        }
        val c = buffer[start]
        if (c == '\r' || c == '\n') {
            tokenEnd = if (c == '\r' && start + 1 < end && buffer[start + 1] == '\n') start + 2 else start + 1
            type = HelixExpressionTypes.NEW_LINE
            lineStart = true
            expectFunction = false
            enclosed = false
            return
        }
        if (c == ' ' || c == '\t') {
            tokenEnd = scanWhile(start + 1) { it == ' ' || it == '\t' }
            type = HelixExpressionTypes.WHITE_SPACE
            return
        }
        if (expectFunction && isName(c)) {
            tokenEnd = scanWhile(start + 1, ::isName)
            type = HelixExpressionTypes.FUNCTION
            expectFunction = false
            lineStart = false
            return
        }
        if (startsWith("@@")) return token(2, HelixExpressionTypes.ESCAPED_AT)
        if (startsWith("@(")) {
            enclosed = true
            return token(2, HelixExpressionTypes.ENCLOSED_START)
        }
        if (startsWith(":!?")) {
            expectFunction = true
            return token(3, HelixExpressionTypes.NEGATED_PREDICATE)
        }
        if (startsWith(":?")) {
            expectFunction = true
            return token(2, HelixExpressionTypes.PREDICATE)
        }
        if (c == ':') {
            expectFunction = true
            return token(1, HelixExpressionTypes.INVOKE)
        }
        if (c == '<') {
            var i = start + 1
            while (i < end && buffer[i] != '>' && buffer[i] != '\r' && buffer[i] != '\n') i++
            if (i < end && buffer[i] == '>') i++
            tokenEnd = i
            type = HelixExpressionTypes.ARGUMENT
            lineStart = false
            return
        }
        if (c == '#') {
            tokenEnd = scanWhile(start + 1, ::isName)
            type = HelixExpressionTypes.PATH
            lineStart = false
            return
        }
        if (c == '@' && start + 1 < end && isName(buffer[start + 1])) {
            tokenEnd = scanWhile(start + 2, ::isName)
            type = if (lineStart) HelixExpressionTypes.DIRECTIVE else HelixExpressionTypes.VALUE
            lineStart = false
            return
        }
        if (c == ')' && enclosed) {
            enclosed = false
            return token(1, HelixExpressionTypes.ENCLOSED_END)
        }
        if (c == ')') return token(1, HelixExpressionTypes.TEXT)

        tokenEnd = scanWhile(start + 1) {
            it !in charArrayOf('@', '#', ':', '<', ')', '\r', '\n', ' ', '\t')
        }
        if (tokenEnd == start) tokenEnd++
        type = HelixExpressionTypes.TEXT
        lineStart = false
    }

    private fun token(length: Int, tokenType: IElementType) {
        tokenEnd = (start + length).coerceAtMost(end)
        type = tokenType
        lineStart = false
    }

    private fun startsWith(value: String): Boolean =
        start + value.length <= end && buffer.subSequence(start, start + value.length).toString() == value

    private fun scanWhile(from: Int, predicate: (Char) -> Boolean): Int {
        var i = from
        while (i < end && predicate(buffer[i])) i++
        return i
    }

    private fun isName(c: Char): Boolean = c == '_' || c.isLetterOrDigit()

    private companion object {
        const val LINE_STARTED = 1
        const val EXPECT_FUNCTION = 2
        const val ENCLOSED = 4
    }
}

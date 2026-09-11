package dev.helight.helix.hal

import dev.helight.helix.hal.generated.HalLexer
import dev.helight.helix.hal.generated.HalParser
import org.antlr.v4.runtime.ANTLRInputStream
import org.antlr.v4.runtime.BaseErrorListener
import org.antlr.v4.runtime.CommonTokenStream
import org.antlr.v4.runtime.RecognitionException
import org.antlr.v4.runtime.Recognizer
import org.antlr.v4.runtime.Token

internal data class HalAntlrToken(val type: Int, val start: Int, val end: Int)
internal data class HalAntlrDiagnostic(val start: Int, val end: Int, val message: String)
internal data class HalAntlrParse(val source: String, val tokens: List<HalAntlrToken>,
                                  val tree: HalParser.DocumentContext, val diagnostics: List<HalAntlrDiagnostic>)

internal object HalAntlrSyntax {
    private val latest = ThreadLocal<HalAntlrParse>()

    @Suppress("DEPRECATION")
    fun parse(source: CharSequence): HalAntlrParse {
        val text = source.toString()
        latest.get()?.takeIf { it.source == text }?.let { return it }
        val diagnostics = arrayListOf<HalAntlrDiagnostic>()
        val listener = object : BaseErrorListener() {
            override fun syntaxError(recognizer: Recognizer<*, *>?, offendingSymbol: Any?, line: Int,
                                     charPositionInLine: Int, msg: String, e: RecognitionException?) {
                val token = offendingSymbol as? Token
                val start = (token?.startIndex ?: (recognizer as? HalLexer)?.charIndex ?: 0).coerceIn(0, text.length)
                val end = ((token?.stopIndex ?: start) + 1).coerceIn(start, text.length)
                diagnostics += HalAntlrDiagnostic(start, maxOf(start + 1, end).coerceAtMost(text.length), msg)
            }
        }
        val lexer = HalLexer(ANTLRInputStream(text)).apply { removeErrorListeners(); addErrorListener(listener) }
        val stream = CommonTokenStream(lexer)
        val parser = HalParser(stream).apply { removeErrorListeners(); addErrorListener(listener) }
        val tree = parser.document()
        stream.fill()
        val tokens = arrayListOf<HalAntlrToken>()
        var position = 0
        fun gap(end: Int) {
            if (end <= position) return
            tokens += HalAntlrToken(0, position, end)
        }
        stream.tokens.filter { it.type != Token.EOF }.forEach {
            gap(it.startIndex); tokens += HalAntlrToken(it.type, it.startIndex, it.stopIndex + 1); position = it.stopIndex + 1
        }
        gap(text.length)
        return HalAntlrParse(text, tokens, tree, diagnostics).also(latest::set)
    }

    fun lex(source: CharSequence) = parse(source).tokens
}

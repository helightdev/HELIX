package dev.helight.helix.mixin

import dev.helight.helix.mixin.generated.MixinLexer
import dev.helight.helix.mixin.generated.MixinParser
import org.antlr.v4.runtime.ANTLRInputStream
import org.antlr.v4.runtime.BaseErrorListener
import org.antlr.v4.runtime.CommonTokenStream
import org.antlr.v4.runtime.ParserRuleContext
import org.antlr.v4.runtime.RecognitionException
import org.antlr.v4.runtime.Recognizer
import org.antlr.v4.runtime.Token
import org.antlr.v4.runtime.tree.TerminalNode

internal data class HelixAntlrToken(val type: Int, val start: Int, val end: Int)
internal data class HelixAntlrDiagnostic(val start: Int, val end: Int, val message: String)
internal data class HelixAntlrParse(
    val source: String,
    val tokens: List<HelixAntlrToken>,
    val tree: MixinParser.CompilationUnitContext,
    val diagnostics: List<HelixAntlrDiagnostic>
)

/** Both recognition and lexer modes come exclusively from the generated Java grammar. */
internal object HelixMixinAntlrSyntax {
    private val latest = ThreadLocal<HelixAntlrParse>()

    @Suppress("DEPRECATION") // IntelliJ offsets are UTF-16, not Unicode code-point indices.
    fun parse(source: CharSequence): HelixAntlrParse {
        val text = source.toString()
        latest.get()?.takeIf { it.source == text }?.let { return it }
        val diagnostics = ArrayList<HelixAntlrDiagnostic>()
        val listener = object : BaseErrorListener() {
            override fun syntaxError(recognizer: Recognizer<*, *>?, offendingSymbol: Any?, line: Int,
                                     charPositionInLine: Int, msg: String, e: RecognitionException?) {
                val token = offendingSymbol as? Token
                val start = (token?.startIndex ?: (recognizer as? MixinLexer)?.charIndex ?: 0).coerceIn(0, text.length)
                val end = ((token?.stopIndex ?: start) + 1).coerceIn(start, text.length)
                diagnostics += HelixAntlrDiagnostic(start, end, msg)
            }
        }
        val lexer = MixinLexer(ANTLRInputStream(text)).apply {
            removeErrorListeners()
            addErrorListener(listener)
        }
        val stream = CommonTokenStream(lexer)
        val parser = MixinParser(stream).apply {
            removeErrorListeners()
            addErrorListener(listener)
        }
        val tree = parser.compilationUnit()
        stream.fill()
        val tokens = ArrayList<HelixAntlrToken>()
        var position = 0
        fun gap(end: Int) {
            if (end <= position) return
            var start = position
            while (start < end) {
                val whitespace = text[start].isWhitespace()
                var stop = start + 1
                while (stop < end && text[stop].isWhitespace() == whitespace) stop++
                tokens += HelixAntlrToken(if (whitespace) 0 else MixinLexer.ERROR_TOKEN, start, stop)
                start = stop
            }
        }
        for (token in stream.tokens) {
            if (token.type == Token.EOF) continue
            gap(token.startIndex)
            tokens += HelixAntlrToken(token.type, token.startIndex, token.stopIndex + 1)
            if (token.type == MixinLexer.ERROR_TOKEN)
                diagnostics += HelixAntlrDiagnostic(token.startIndex, token.stopIndex + 1, "Invalid token")
            position = token.stopIndex + 1
        }
        gap(text.length)
        return HelixAntlrParse(text, tokens, tree, diagnostics).also(latest::set)
    }

    fun rules(context: ParserRuleContext): Sequence<ParserRuleContext> = sequence {
        yield(context)
        context.children.orEmpty().filterIsInstance<ParserRuleContext>().forEach { yieldAll(rules(it)) }
    }

    fun declarationNames(context: ParserRuleContext): Sequence<TerminalNode> = rules(context).mapNotNull {
        when (it) {
            is MixinParser.FuncDeclarationContext -> it.IDENTIFIER()
            is MixinParser.VariableIdentifierContext -> it.IDENTIFIER()
            is MixinParser.LabelIdentifierContext -> it.LABEL_IDENTIFIER()
            else -> null
        }
    }
}

package dev.helight.helix.hix

import dev.helight.helix.hix.generated.HixLexer
import dev.helight.helix.hix.generated.HixParser
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
    val tree: HixParser.CompilationUnitContext,
    val diagnostics: List<HelixAntlrDiagnostic>
)

/** Both recognition and lexer modes come exclusively from the generated Java grammar. */
internal object HixAntlrSyntax {
    private val latest = ThreadLocal<HelixAntlrParse>()

    private data class Pass(val tree: HixParser.CompilationUnitContext,
                            val tokens: List<HelixAntlrToken>,
                            val diagnostics: List<HelixAntlrDiagnostic>)

    @Suppress("DEPRECATION") // IntelliJ offsets are UTF-16, not Unicode code-point indices.
    fun parse(source: CharSequence): HelixAntlrParse {
        val text = source.toString()
        latest.get()?.takeIf { it.source == text }?.let { return it }
        val original = parsePass(text)
        val recovered = text.toCharArray()
        var pass = original
        var attempts = 0
        while (attempts++ < MAX_RECOVERY_PASSES) {
            val offsets = pass.diagnostics.mapNotNull { diagnostic ->
                diagnostic.start.takeIf { it < text.length }
            }.toMutableSet()
            unmatchedMultilineMode(pass.tokens, text)?.let(offsets::add)
            if (offsets.isEmpty() || offsets.none { blankLine(recovered, it) }) break
            pass = parsePass(String(recovered))
        }
        val diagnostics = (original.diagnostics + original.tokens.filter { it.type == HixLexer.ERROR_TOKEN }
            .map { HelixAntlrDiagnostic(it.start, it.end, "Invalid token") }).distinct()
        return HelixAntlrParse(text, pass.tokens, pass.tree, diagnostics).also(latest::set)
    }

    /** Raw document tokens for editor services; parser recovery must never rewrite this view. */
    fun lex(source: CharSequence): List<HelixAntlrToken> = parsePass(source.toString()).tokens

    @Suppress("DEPRECATION")
    private fun parsePass(text: String): Pass {
        val diagnostics = ArrayList<HelixAntlrDiagnostic>()
        val listener = object : BaseErrorListener() {
            override fun syntaxError(recognizer: Recognizer<*, *>?, offendingSymbol: Any?, line: Int,
                                     charPositionInLine: Int, msg: String, e: RecognitionException?) {
                val token = offendingSymbol as? Token
                val start = (token?.startIndex ?: (recognizer as? HixLexer)?.charIndex ?: 0).coerceIn(0, text.length)
                val end = ((token?.stopIndex ?: start) + 1).coerceIn(start, text.length)
                diagnostics += HelixAntlrDiagnostic(start, end, msg)
            }
        }
        val lexer = object : HixLexer(ANTLRInputStream(text)) {
            override fun popMode(): Int {
                if (_modeStack.isEmpty) {
                    mode(DEFAULT_MODE)
                    return DEFAULT_MODE
                }
                return super.popMode()
            }
        }.apply {
            removeErrorListeners()
            addErrorListener(listener)
        }
        val stream = CommonTokenStream(lexer)
        val parser = HixParser(stream).apply {
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
                tokens += HelixAntlrToken(if (whitespace) 0 else HixLexer.ERROR_TOKEN, start, stop)
                start = stop
            }
        }
        for (token in stream.tokens) {
            if (token.type == Token.EOF) continue
            gap(token.startIndex)
            tokens += HelixAntlrToken(token.type, token.startIndex, token.stopIndex + 1)
            position = token.stopIndex + 1
        }
        gap(text.length)
        return Pass(tree, tokens, diagnostics)
    }

    private fun unmatchedMultilineMode(tokens: List<HelixAntlrToken>, source: String): Int? {
        data class Opener(val offset: Int, val closer: Int)
        val openers = ArrayDeque<Opener>()
        tokens.forEach { token ->
            val closer = when (token.type) {
                HixLexer.BEGIN_ARGUMENT -> HixLexer.ARGUMENT_END
                HixLexer.BEGIN_PARAMETERS -> HixLexer.END_PARAMETERS
                HixLexer.BEGIN_VALUE_INLINE, HixLexer.BEGIN_METADATA_VALUE,
                HixLexer.BEGIN_TUPLE -> HixLexer.VALUE_END_INLINE
                HixLexer.BEGIN_TABLE -> HixLexer.RC
                else -> null
            }
            if (closer != null) openers.addLast(Opener(token.start, closer))
            else if (openers.lastOrNull()?.closer == token.type) openers.removeLast()
        }
        return openers.lastOrNull()?.offset?.takeIf { source.indexOf('\n', it) >= 0 }
    }

    private fun blankLine(text: CharArray, offset: Int): Boolean {
        if (text.isEmpty()) return false
        var start = offset.coerceIn(0, text.lastIndex)
        while (start > 0 && text[start - 1] != '\n' && text[start - 1] != '\r') start--
        var end = start
        while (end < text.size && text[end] != '\n' && text[end] != '\r') end++
        // At EOF, ANTLR's partial context is more useful than deleting the entire line. Line
        // quarantine exists to prevent an error from poisoning syntax that follows it.
        if (end == text.size) return false
        var changed = false
        for (index in start until end) if (!text[index].isWhitespace()) {
            text[index] = ' '
            changed = true
        }
        return changed
    }

    fun rules(context: ParserRuleContext): Sequence<ParserRuleContext> = sequence {
        yield(context)
        context.children.orEmpty().filterIsInstance<ParserRuleContext>().forEach { yieldAll(rules(it)) }
    }

    fun declarationNames(context: ParserRuleContext): Sequence<TerminalNode> = rules(context).mapNotNull {
        when (it) {
            is HixParser.FuncDeclarationContext -> it.functionDeclarationIdentifier().IDENTIFIER()
            is HixParser.TypeDeclarationContext -> it.IDENTIFIER()
            is HixParser.VariableIdentifierContext -> it.IDENTIFIER()
            is HixParser.LabelIdentifierContext -> it.LABEL_IDENTIFIER()
            else -> null
        }
    }

    private const val MAX_RECOVERY_PASSES = 32
}

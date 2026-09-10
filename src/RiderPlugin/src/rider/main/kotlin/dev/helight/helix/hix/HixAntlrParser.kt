package dev.helight.helix.hix

import com.intellij.lang.ASTNode
import com.intellij.lang.PsiBuilder
import com.intellij.lang.PsiParser
import com.intellij.lexer.LexerBase
import com.intellij.openapi.project.Project
import com.intellij.psi.tree.IElementType
import dev.helight.helix.hix.generated.HixLexer
import dev.helight.helix.hix.generated.HixParser
import org.antlr.v4.runtime.ParserRuleContext
import org.antlr.v4.runtime.tree.TerminalNode

internal class HelixAntlrTokenType(val antlrType: Int, debugName: String? = null) : IElementType(
    debugName ?: HixLexer.VOCABULARY.getSymbolicName(antlrType) ?: "HIX_WHITESPACE", HixLanguage)

internal object HelixAntlrTypes {
    val tokens = Array(HixLexer.VOCABULARY.maxTokenType + 1) { HelixAntlrTokenType(it) }
    val rules = Array(HixParser.ruleNames.size) { HixElementType(HixParser.ruleNames[it]) }
}

/**
 * A few Hix delimiters share one ANTLR closing token even though they open different constructs.
 * IntelliJ's brace infrastructure requires a one-to-one token pair, so the editor lexer gives
 * those closers distinct identities while retaining their underlying ANTLR type for highlighting.
 */
internal object HixEditorTokenTypes {
    val CONSTRUCT_PREFIX = HelixAntlrTokenType(0, "HIX_CONSTRUCT_PREFIX")
    val METADATA_VALUE_PREFIX = HelixAntlrTokenType(HixLexer.BEGIN_METADATA_VALUE,
        "HIX_METADATA_VALUE_PREFIX")
    val TABLE_END = HelixAntlrTokenType(HixLexer.RC, "HIX_TABLE_END")
    val INTERPOLATION_END = HelixAntlrTokenType(HixLexer.RC, "HIX_INTERPOLATION_END")
    val TUPLE_END = HelixAntlrTokenType(HixLexer.VALUE_END_INLINE, "HIX_TUPLE_END")
    val INLINE_END = HelixAntlrTokenType(HixLexer.VALUE_END_INLINE, "HIX_INLINE_END")
    val METADATA_VALUE_END = HelixAntlrTokenType(HixLexer.VALUE_END_INLINE, "HIX_METADATA_VALUE_END")
}

class HixEditorLexer(@Suppress("UNUSED_PARAMETER") project: Project?) : LexerBase() {
    private data class EditorToken(val type: IElementType, val start: Int, val end: Int)

    private var buffer: CharSequence = ""
    private var end = 0
    private var tokens: List<EditorToken> = emptyList()
    private var index = 0
    override fun start(buffer: CharSequence, startOffset: Int, endOffset: Int, initialState: Int) {
        this.buffer = buffer
        end = endOffset
        // Replay lexer modes from the beginning; restart offsets can fall inside interpolated arguments.
        val delimiterStack = ArrayDeque<Pair<Int, IElementType>>()
        val editorTokens = ArrayList<EditorToken>()
        HixAntlrSyntax.lex(buffer.subSequence(0, endOffset)).forEach { token ->
            if (token.type == HixLexer.EMPTY_PARAMETERS && token.end - token.start >= 2) {
                editorTokens += EditorToken(HelixAntlrTypes.tokens[HixLexer.BEGIN_PARAMETERS],
                    token.start, token.start + 1)
                if (token.end > token.start + 2) editorTokens += EditorToken(HelixAntlrTypes.tokens[0],
                    token.start + 1, token.end - 1)
                editorTokens += EditorToken(HelixAntlrTypes.tokens[HixLexer.END_PARAMETERS],
                    token.end - 1, token.end)
                return@forEach
            }

            var tokenStart = token.start
            if (token.end - token.start == 2 && token.type in setOf(HixLexer.BEGIN_TABLE,
                    HixLexer.BEGIN_TUPLE, HixLexer.BEGIN_METADATA_VALUE,
                    HixLexer.BEGIN_VALUE_INTERPOLATE)) {
                val prefixType = if (token.type == HixLexer.BEGIN_METADATA_VALUE)
                    HixEditorTokenTypes.METADATA_VALUE_PREFIX else HixEditorTokenTypes.CONSTRUCT_PREFIX
                editorTokens += EditorToken(prefixType, token.start, token.start + 1)
                tokenStart++
            }

            val closer = when (token.type) {
                HixLexer.LC -> HixLexer.RC to HelixAntlrTypes.tokens[HixLexer.RC]
                HixLexer.BEGIN_TABLE -> HixLexer.RC to HixEditorTokenTypes.TABLE_END
                HixLexer.BEGIN_VALUE_INTERPOLATE -> HixLexer.RC to HixEditorTokenTypes.INTERPOLATION_END
                HixLexer.BEGIN_PARAMETERS -> HixLexer.END_PARAMETERS to HelixAntlrTypes.tokens[HixLexer.END_PARAMETERS]
                HixLexer.BEGIN_TUPLE -> HixLexer.VALUE_END_INLINE to HixEditorTokenTypes.TUPLE_END
                HixLexer.BEGIN_VALUE_INLINE -> HixLexer.VALUE_END_INLINE to HixEditorTokenTypes.INLINE_END
                HixLexer.BEGIN_METADATA_VALUE -> HixLexer.VALUE_END_INLINE to HixEditorTokenTypes.METADATA_VALUE_END
                else -> null
            }
            if (closer != null) delimiterStack.addLast(closer)
            val type = if (delimiterStack.lastOrNull()?.first == token.type && closer == null)
                delimiterStack.removeLast().second else HelixAntlrTypes.tokens[token.type]
            editorTokens += EditorToken(type, tokenStart, token.end)
        }
        tokens = editorTokens.asSequence().filter { it.end > startOffset && it.start < endOffset }
            .map { it.copy(start = maxOf(startOffset, it.start), end = minOf(endOffset, it.end)) }.toList()
        index = 0
    }
    override fun getState() = 0
    override fun getTokenType(): IElementType? = tokens.getOrNull(index)?.type
    override fun getTokenStart() = tokens.getOrNull(index)?.start ?: end
    override fun getTokenEnd() = tokens.getOrNull(index)?.end ?: end
    override fun getBufferSequence() = buffer
    override fun getBufferEnd() = end
    override fun advance() { if (index < tokens.size) index++ }
}

class HixPsiParser(@Suppress("UNUSED_PARAMETER") project: Project) : PsiParser {
    override fun parse(root: IElementType, builder: PsiBuilder): ASTNode {
        val marker = builder.mark()
        val parsed = HixAntlrSyntax.parse(builder.originalText)
        parsed.tree.children.orEmpty().filterIsInstance<ParserRuleContext>().forEach { node(builder, it) }
        while (!builder.eof()) builder.advanceLexer()
        marker.done(root)
        return builder.treeBuilt
    }

    private fun node(builder: PsiBuilder, context: ParserRuleContext) {
        val start = context.start?.startIndex ?: return
        val end = (context.stop?.stopIndex ?: start - 1) + 1
        if (start < 0 || end <= start) return
        while (!builder.eof() && builder.currentOffset < start) builder.advanceLexer()
        if (builder.currentOffset != start) return
        val marker = builder.mark()
        for (child in context.children.orEmpty()) {
            if (child is ParserRuleContext) node(builder, child)
            else if (child is TerminalNode && child.symbol.type == HixLexer.IDENTIFIER &&
                (context is HixParser.FuncDeclarationContext || context is HixParser.InvocationStatementContext ||
                    context is HixParser.ControlflowStatementContext)) {
                while (!builder.eof() && builder.currentOffset < child.symbol.startIndex) builder.advanceLexer()
                if (!builder.eof() && builder.currentOffset == child.symbol.startIndex) {
                    val name = builder.mark()
                    builder.advanceLexer()
                    name.done(if (context is HixParser.FuncDeclarationContext) HixElementTypes.DECLARATION
                        else HixElementTypes.REFERENCE)
                }
            }
        }
        while (!builder.eof() && builder.currentOffset < end) builder.advanceLexer()
        marker.done(when (context) {
            is HixParser.VariableIdentifierContext, is HixParser.MixinIdentifierContext,
            is HixParser.FunctionDeclarationIdentifierContext -> HixElementTypes.DECLARATION
            is HixParser.FunctionIdentifierContext -> HixElementTypes.REFERENCE
            else -> HelixAntlrTypes.rules[context.ruleIndex]
        })
    }
}

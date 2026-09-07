package dev.helight.helix.mixin

import com.intellij.lang.ASTNode
import com.intellij.lang.PsiBuilder
import com.intellij.lang.PsiParser
import com.intellij.lexer.LexerBase
import com.intellij.openapi.project.Project
import com.intellij.psi.tree.IElementType
import dev.helight.helix.mixin.generated.MixinLexer
import dev.helight.helix.mixin.generated.MixinParser
import org.antlr.v4.runtime.ParserRuleContext
import org.antlr.v4.runtime.tree.TerminalNode

internal class HelixAntlrTokenType(val antlrType: Int) : IElementType(
    MixinLexer.VOCABULARY.getSymbolicName(antlrType) ?: "MIXIN_WHITESPACE", HelixMixinLanguage)

internal object HelixAntlrTypes {
    val tokens = Array(MixinLexer.VOCABULARY.maxTokenType + 1) { HelixAntlrTokenType(it) }
    val rules = Array(MixinParser.ruleNames.size) { HelixMixinElementType(MixinParser.ruleNames[it]) }
}

class HelixMixinLexer(@Suppress("UNUSED_PARAMETER") project: Project?) : LexerBase() {
    private var buffer: CharSequence = ""
    private var end = 0
    private var tokens: List<HelixAntlrToken> = emptyList()
    private var index = 0
    override fun start(buffer: CharSequence, startOffset: Int, endOffset: Int, initialState: Int) {
        this.buffer = buffer
        end = endOffset
        // Replay lexer modes from the beginning; restart offsets can fall inside interpolated arguments.
        tokens = HelixMixinAntlrSyntax.parse(buffer.subSequence(0, endOffset)).tokens
            .asSequence().filter { it.end > startOffset && it.start < endOffset }
            .map { it.copy(start = maxOf(startOffset, it.start), end = minOf(endOffset, it.end)) }.toList()
        index = 0
    }
    override fun getState() = 0
    override fun getTokenType(): IElementType? = tokens.getOrNull(index)?.let { HelixAntlrTypes.tokens[it.type] }
    override fun getTokenStart() = tokens.getOrNull(index)?.start ?: end
    override fun getTokenEnd() = tokens.getOrNull(index)?.end ?: end
    override fun getBufferSequence() = buffer
    override fun getBufferEnd() = end
    override fun advance() { if (index < tokens.size) index++ }
}

class HelixMixinParser(@Suppress("UNUSED_PARAMETER") project: Project) : PsiParser {
    override fun parse(root: IElementType, builder: PsiBuilder): ASTNode {
        val marker = builder.mark()
        val parsed = HelixMixinAntlrSyntax.parse(builder.originalText)
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
            else if (child is TerminalNode && child.symbol.type == MixinLexer.IDENTIFIER &&
                (context is MixinParser.FuncDeclarationContext || context is MixinParser.InvocationStatementContext ||
                    context is MixinParser.ControlflowStatementContext)) {
                while (!builder.eof() && builder.currentOffset < child.symbol.startIndex) builder.advanceLexer()
                if (!builder.eof() && builder.currentOffset == child.symbol.startIndex) {
                    val name = builder.mark()
                    builder.advanceLexer()
                    name.done(if (context is MixinParser.FuncDeclarationContext) HelixMixinElementTypes.DECLARATION
                        else HelixMixinElementTypes.REFERENCE)
                }
            }
        }
        while (!builder.eof() && builder.currentOffset < end) builder.advanceLexer()
        marker.done(when (context) {
            is MixinParser.VariableIdentifierContext, is MixinParser.MixinIdentifierContext -> HelixMixinElementTypes.DECLARATION
            is MixinParser.FunctionIdentifierContext -> HelixMixinElementTypes.REFERENCE
            else -> HelixAntlrTypes.rules[context.ruleIndex]
        })
    }
}

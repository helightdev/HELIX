package dev.helight.helix.hal

import com.intellij.lang.ASTNode
import com.intellij.lang.PsiBuilder
import com.intellij.lang.PsiParser
import com.intellij.openapi.project.Project
import com.intellij.psi.tree.IElementType
import dev.helight.helix.hal.generated.HalLexer as GeneratedHalLexer
import dev.helight.helix.hal.generated.HalParser
import org.antlr.v4.runtime.ParserRuleContext

internal class HalAntlrTokenType(val antlrType: Int) : IElementType(
    GeneratedHalLexer.VOCABULARY.getSymbolicName(antlrType) ?: "HAL_WHITESPACE", HalLanguage)
internal object HalAntlrTypes {
    val tokens = Array(GeneratedHalLexer.VOCABULARY.maxTokenType + 1) { HalAntlrTokenType(it) }
    val rules = Array(HalParser.ruleNames.size) { IElementType(HalParser.ruleNames[it], HalLanguage) }
}

internal class HalPsiParser(@Suppress("UNUSED_PARAMETER") project: Project) : PsiParser {
    override fun parse(root: IElementType, builder: PsiBuilder): ASTNode {
        val file = builder.mark()
        val parsed = HalAntlrSyntax.parse(builder.originalText)
        parsed.tree.children.orEmpty().filterIsInstance<ParserRuleContext>().forEach { node(builder, it) }
        while (!builder.eof()) builder.advanceLexer()
        file.done(root)
        return builder.treeBuilt
    }

    private fun node(builder: PsiBuilder, context: ParserRuleContext) {
        val start = context.start?.startIndex ?: return
        val end = (context.stop?.stopIndex ?: start - 1) + 1
        if (start < 0 || end <= start) return
        while (!builder.eof() && builder.currentOffset < start) builder.advanceLexer()
        if (builder.currentOffset != start) return
        val marker = builder.mark()
        context.children.orEmpty().filterIsInstance<ParserRuleContext>().forEach { node(builder, it) }
        while (!builder.eof() && builder.currentOffset < end) builder.advanceLexer()
        marker.done(HalAntlrTypes.rules[context.ruleIndex])
    }
}

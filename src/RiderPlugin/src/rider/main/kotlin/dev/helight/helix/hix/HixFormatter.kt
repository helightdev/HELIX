package dev.helight.helix.hix

import com.intellij.formatting.Alignment
import com.intellij.formatting.Block
import com.intellij.formatting.ChildAttributes
import com.intellij.formatting.FormattingContext
import com.intellij.formatting.FormattingModel
import com.intellij.formatting.FormattingModelBuilder
import com.intellij.formatting.FormattingModelProvider
import com.intellij.formatting.Indent
import com.intellij.formatting.Spacing
import com.intellij.formatting.Wrap
import com.intellij.lang.ASTNode
import com.intellij.openapi.util.TextRange
import com.intellij.psi.PsiDocumentManager
import com.intellij.psi.TokenType
import com.intellij.psi.formatter.common.AbstractBlock
import com.intellij.psi.impl.source.codeStyle.PreFormatProcessor
import com.jetbrains.rd.framework.impl.RpcTimeouts
import com.jetbrains.rider.projectView.solution
import dev.helight.helix.protocol.helixExpressionModel

class HixFormattingModelBuilder : FormattingModelBuilder {
    override fun createModel(formattingContext: FormattingContext): FormattingModel =
        FormattingModelProvider.createFormattingModelForPsiFile(formattingContext.containingFile,
            HixPassthroughBlock(formattingContext.node, null, null), formattingContext.codeStyleSettings)
}

private class HixPassthroughBlock(node: ASTNode, wrap: Wrap?, alignment: Alignment?) :
    AbstractBlock(node, wrap, alignment) {
    override fun buildChildren(): List<Block> {
        val result = arrayListOf<Block>(); var child = myNode.firstChildNode
        while (child != null) {
            if (child.elementType != TokenType.WHITE_SPACE && child.textLength > 0)
                result.add(HixPassthroughBlock(child, null, null))
            child = child.treeNext
        }
        return result
    }
    override fun getIndent(): Indent? = null
    override fun getSpacing(child1: Block?, child2: Block): Spacing = Spacing.getReadOnlySpacing()
    // This method is also queried for an empty block while Rider calculates the
    // indentation for Enter/backspace. Delegating to a child in that case makes
    // FormatProcessor try to read child 0 from an empty list.
    override fun getChildAttributes(newChildIndex: Int) = ChildAttributes(Indent.getNoneIndent(), null)
    override fun isLeaf() = myNode.firstChildNode == null
}

class HixPreFormatProcessor : PreFormatProcessor {
    override fun process(element: ASTNode, range: TextRange): TextRange {
        val file = element.psi.containingFile ?: return range
        if (file.language != HixLanguage) return range
        val documents = PsiDocumentManager.getInstance(file.project)
        val document = documents.getDocument(file) ?: return range
        val formatted = try {
            file.project.solution.helixExpressionModel.formatHix.sync(file.text, RpcTimeouts(1_000L, 5_000L))
        } catch (_: Throwable) { return range }
        if (formatted != document.text) { document.replaceString(0, document.textLength, formatted); documents.commitDocument(document) }
        return TextRange(0, formatted.length)
    }
}

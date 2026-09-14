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
    override fun getChildAttributes(newChildIndex: Int): ChildAttributes {
        val children = subBlocks
        if (children.isEmpty()) return ChildAttributes(Indent.getNoneIndent(), null)

        // Prefer the preceding syntax when Enter is pressed. Besides avoiding an
        // invalid delegation into an empty list, this retains the indentation of
        // content introduced by @> / @+ and of an already-chopped argument list.
        if (newChildIndex > 0) {
            val previous = children[minOf(newChildIndex - 1, children.lastIndex)]
            if (previous is HixPassthroughBlock && previous.opensIndentedRegion())
                return ChildAttributes(Indent.getNormalIndent(), null)
            return ChildAttributes.DELEGATE_TO_PREV_CHILD
        }
        return ChildAttributes.DELEGATE_TO_NEXT_CHILD
    }

    private fun opensIndentedRegion(): Boolean {
        // Function/block headers are commonly represented as one composite PSI
        // child (for example, `func main {`), unlike table openers which are a
        // standalone leaf. Inspect the significant trailing character so both
        // shapes receive the same editor indentation.
        return myNode.text.trimEnd().lastOrNull() in setOf('{', '(', '[')
    }
    override fun isLeaf() = myNode.firstChildNode == null
}

class HixPreFormatProcessor : PreFormatProcessor {
    override fun process(element: ASTNode, range: TextRange): TextRange {
        val file = element.psi.containingFile ?: return range
        if (file.language != HixLanguage) return range
        val documents = PsiDocumentManager.getInstance(file.project)
        val document = documents.getDocument(file) ?: return range
        val formatted = try {
            file.project.getService(dev.helight.helix.hix.HixToolchainService::class.java).request("hix/format", mapOf("text" to file.text), 5_000).asString
        } catch (_: Throwable) { return range }
        if (formatted != document.text) { document.replaceString(0, document.textLength, formatted); documents.commitDocument(document) }
        return TextRange(0, formatted.length)
    }
}

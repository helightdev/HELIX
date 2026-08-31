package dev.helight.helix.mixin

import com.intellij.lang.ASTNode
import com.intellij.lang.PsiBuilder
import com.intellij.lang.PsiParser
import com.intellij.lexer.LexerBase
import com.intellij.openapi.project.Project
import com.intellij.psi.tree.IElementType
import java.util.TreeSet

/** Lossless synchronous frontend lexer; backend snapshots are not on the typing path. */
class HelixMixinLexer(private val project: Project?) : LexerBase() {
    private var buffer: CharSequence = ""
    private var endOffset = 0
    private var boundaries = intArrayOf(0)
    private var boundaryIndex = 0
    private var tokens: List<HelixLocalToken> = emptyList()
    private var expressionParentheses: Set<Int> = emptySet()
    private var tokenType: IElementType? = null

    override fun start(buffer: CharSequence, startOffset: Int, endOffset: Int, initialState: Int) {
        this.buffer = buffer
        this.endOffset = endOffset
        // Reconstruct state from the beginning so an IntelliJ incremental restart in the middle
        // of a nested argument receives the same tokens as a full lex. The lexer is local and
        // linear, so this remains cheap while avoiding a second encoded lexer-state machine.
        val parsed = HelixMixinFrontendParseCache.parse(buffer)
        tokens = parsed.tokens
        val points = TreeSet<Int>()
        points += startOffset
        points += endOffset
        tokens.forEach { add(points, it.start, it.end, startOffset, endOffset) }
        fun addSyntax(node: HelixLocalNode) {
            add(points, node.start, node.end, startOffset, endOffset)
            if (node.kind == "ExpressionArgument" && node.end - node.start >= 4) {
                // The compiler syntax owns <(expression)> as one argument. MixinEditorLexer
                // deliberately leaves these two parentheses in Argument tokens, so the PSI
                // adapter splits them using the authoritative argument range.
                add(points, node.start + 1, node.start + 2, startOffset, endOffset)
                add(points, node.end - 2, node.end - 1, startOffset, endOffset)
            }
            if (node.kind == "ParenthesizedReference" && node.end - node.start >= 3) {
                add(points, node.start + 1, node.start + 2, startOffset, endOffset)
                add(points, node.end - 1, node.end, startOffset, endOffset)
            }
            node.children.forEach(::addSyntax)
        }
        addSyntax(parsed.syntax)
        val parentheses = HashSet<Int>()
        fun collectParentheses(node: HelixLocalNode) {
            if (node.kind == "ExpressionArgument" && node.end - node.start >= 4) {
                parentheses += node.start + 1
                parentheses += node.end - 2
            }
            if (node.kind == "ParenthesizedReference" && node.end - node.start >= 3) {
                parentheses += node.start + 1
                parentheses += node.end - 1
            }
            node.children.forEach(::collectParentheses)
        }
        collectParentheses(parsed.syntax)
        expressionParentheses = parentheses
        boundaries = points.toIntArray()
        boundaryIndex = boundaries.binarySearch(startOffset).let { if (it < 0) -it - 1 else it }
        updateToken()
    }

    override fun getState() = 0
    override fun getTokenType(): IElementType? = tokenType
    override fun getTokenStart(): Int = boundaries.getOrElse(boundaryIndex) { endOffset }
    override fun getTokenEnd(): Int = boundaries.getOrElse(boundaryIndex + 1) { endOffset }
    override fun getBufferSequence(): CharSequence = buffer
    override fun getBufferEnd(): Int = endOffset

    override fun advance() {
        if (boundaryIndex < boundaries.lastIndex) boundaryIndex++
        updateToken()
    }

    private fun updateToken() {
        val start = tokenStart
        if (start >= endOffset || boundaryIndex >= boundaries.lastIndex) {
            tokenType = null
            return
        }
        val local = tokens.firstOrNull { it.start <= start && start < it.end }
        tokenType = if (start in expressionParentheses) {
            if (buffer[start] == '(') HelixMixinTokenTypes.OPEN_PARENTHESIS
            else HelixMixinTokenTypes.CLOSE_PARENTHESIS
        } else local?.let {
            HelixMixinTokenTypes.fromBackend(it.kind, buffer.subSequence(tokenStart, tokenEnd))
        } ?: HelixMixinTokenTypes.PLAIN
    }

    private fun add(points: MutableSet<Int>, start: Int, end: Int, minimum: Int, maximum: Int) {
        if (start in minimum..maximum) points += start
        if (end in minimum..maximum) points += end
    }
}

/** Builds the complete, stable IntelliJ AST synchronously from the local syntax port.
 * Backend snapshots enrich references and diagnostics but never alter PSI structure. */
class HelixMixinParser(private val project: Project) : PsiParser {
    override fun parse(root: IElementType, builder: PsiBuilder): ASTNode {
        val marker = builder.mark()
        val tree = buildTree(builder.originalText)
        tree.children.forEach { parseNode(builder, it) }
        while (!builder.eof()) builder.advanceLexer()
        marker.done(root)
        return builder.treeBuilt
    }

    private fun parseNode(builder: PsiBuilder, node: IntervalNode) {
        while (!builder.eof() && builder.currentOffset < node.start) builder.advanceLexer()
        if (builder.currentOffset > node.start || node.end <= node.start) return
        val marker = builder.mark()
        for (child in node.children) {
            while (!builder.eof() && builder.currentOffset < child.start) builder.advanceLexer()
            parseNode(builder, child)
        }
        while (!builder.eof() && builder.currentOffset < node.end) builder.advanceLexer()
        marker.done(node.type)
    }

    private fun buildTree(source: CharSequence): IntervalNode {
        val local = HelixMixinFrontendParseCache.parse(source).syntax
        val root = interval(local, -1)
        addLocalSemanticOverlays(root, local, source)
        sortChildren(root)
        return root
    }

    private fun interval(node: HelixLocalNode, priority: Int): IntervalNode =
        IntervalNode(node.start, node.end, HelixMixinElementTypes.syntax(node.kind), priority,
            node.children.map { interval(it, 0) }.toMutableList())

    private fun addLocalSemanticOverlays(root: IntervalNode, node: HelixLocalNode, source: CharSequence) {
        val declaration = node.kind == "DeclarationDirectiveArgument" ||
            node.kind == "DeclarationReferenceDirectiveArgument"
        val reference = node.kind == "ReferenceDirectiveArgument" ||
            node.kind == "DeclarationReferenceDirectiveArgument"
        val start = (node.start + 1).coerceAtMost(node.end)
        val end = (node.end - 1).coerceAtLeast(start)
        if (declaration && end > start)
            insertOverlay(root, IntervalNode(start, end, HelixMixinElementTypes.DECLARATION, 1))
        if (reference && end > start)
            insertOverlay(root, IntervalNode(start, end, HelixMixinElementTypes.REFERENCE, 2))

        if (node.kind == "Reference" || node.kind == "ParenthesizedReference") {
            val rootNode = node.children.firstOrNull { it.kind == "Root" }
            if (rootNode != null && rootNode.end > rootNode.start) {
                val rootName = source.subSequence(rootNode.start, rootNode.end).toString()
                if (rootName in setOf("local", "var", "tar", "carry")) {
                    val member = node.children.firstOrNull { it.kind == "Member" }
                    if (member != null && member.end > member.start)
                        insertOverlay(root, IntervalNode(member.start, member.end,
                            HelixMixinElementTypes.REFERENCE, 2))
                }
            }
        }
        node.children.forEach { addLocalSemanticOverlays(root, it, source) }
    }

    private fun insertOverlay(parent: IntervalNode, overlay: IntervalNode) {
        if (parent.start == overlay.start && parent.end == overlay.end && parent.type == overlay.type)
            return
        if (parent.children.any {
                it.start == overlay.start && it.end == overlay.end && it.type == overlay.type
            }) return
        val nested = parent.children.asSequence().filter { it.contains(overlay) }
            .minByOrNull { it.end - it.start }
        if (nested != null) insertOverlay(nested, overlay) else if (parent.contains(overlay)) parent.children += overlay
    }

    private fun sortChildren(node: IntervalNode) {
        node.children.sortWith(compareBy<IntervalNode> { it.start }
            .thenByDescending { it.end }.thenBy { it.priority })
        node.children.forEach(::sortChildren)
    }

    private data class IntervalNode(
        val start: Int,
        val end: Int,
        val type: IElementType,
        val priority: Int,
        val children: MutableList<IntervalNode> = ArrayList()
    ) {
        fun contains(other: IntervalNode): Boolean = other.start >= start && other.end <= end
    }
}

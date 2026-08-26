package dev.helight.helix.expression

import com.intellij.lang.ASTNode
import com.intellij.lang.PsiBuilder
import com.intellij.lang.PsiParser
import com.intellij.psi.tree.IElementType

class HelixExpressionParser : PsiParser {
    override fun parse(root: IElementType, builder: PsiBuilder): ASTNode {
        val file = builder.mark()
        while (!builder.eof()) parseStatement(builder)
        file.done(root)
        return builder.treeBuilt
    }

    private fun parseStatement(builder: PsiBuilder) {
        if (builder.tokenType == HelixExpressionTypes.NEW_LINE) {
            builder.advanceLexer()
            return
        }
        val statement = builder.mark()
        if (builder.tokenType == HelixExpressionTypes.NEWLINE_CONTINUATION ||
            builder.tokenType == HelixExpressionTypes.DIRECT_CONTINUATION) {
            builder.error("Continuation requires an immediately preceding directive")
        }
        if (builder.tokenType == HelixExpressionTypes.DIRECTIVE) {
            val directive = builder.mark()
            builder.advanceLexer()
            while (builder.tokenType == HelixExpressionTypes.ARGUMENT ||
                builder.tokenType == HelixExpressionTypes.WHITE_SPACE) builder.advanceLexer()
            directive.done(HelixExpressionTypes.DIRECTIVE_CALL)
        }
        val expression = builder.mark()
        parseExpressionLine(builder)
        while (builder.tokenType == HelixExpressionTypes.NEW_LINE) {
            builder.advanceLexer()
            if (builder.tokenType != HelixExpressionTypes.NEWLINE_CONTINUATION &&
                builder.tokenType != HelixExpressionTypes.DIRECT_CONTINUATION) break
            val continuation = builder.mark()
            builder.advanceLexer()
            parseExpressionLine(builder)
            continuation.done(HelixExpressionTypes.CONTINUATION)
        }
        expression.done(HelixExpressionTypes.EXPRESSION)
        statement.done(HelixExpressionTypes.STATEMENT)
    }

    private fun parseExpressionLine(builder: PsiBuilder) {
        while (!builder.eof() && builder.tokenType != HelixExpressionTypes.NEW_LINE) {
            if (builder.tokenType == HelixExpressionTypes.VALUE ||
                builder.tokenType == HelixExpressionTypes.ENCLOSED_START) parseReference(builder)
            else builder.advanceLexer()
        }
    }

    private fun parseReference(builder: PsiBuilder) {
        val reference = builder.mark()
        val enclosed = builder.tokenType == HelixExpressionTypes.ENCLOSED_START
        builder.advanceLexer()
        if (enclosed && builder.tokenType == HelixExpressionTypes.TEXT) builder.advanceLexer()
        while (!builder.eof() && builder.tokenType != HelixExpressionTypes.NEW_LINE) {
            when (builder.tokenType) {
                HelixExpressionTypes.PATH -> builder.advanceLexer()
                HelixExpressionTypes.INVOKE,
                HelixExpressionTypes.PREDICATE,
                HelixExpressionTypes.NEGATED_PREDICATE -> parseFunction(builder)
                HelixExpressionTypes.ARGUMENT -> builder.advanceLexer()
                HelixExpressionTypes.ENCLOSED_END -> {
                    builder.advanceLexer()
                    break
                }
                else -> break
            }
        }
        reference.done(HelixExpressionTypes.REFERENCE)
    }

    private fun parseFunction(builder: PsiBuilder) {
        val function = builder.mark()
        builder.advanceLexer()
        if (builder.tokenType == HelixExpressionTypes.FUNCTION) builder.advanceLexer()
        else builder.error("Expected function or predicate name")
        while (builder.tokenType == HelixExpressionTypes.ARGUMENT) builder.advanceLexer()
        function.done(HelixExpressionTypes.FUNCTION_CALL)
    }
}

package dev.helight.helix.mixin

import com.intellij.application.options.CodeStyle
import com.intellij.codeInsight.editorActions.BackspaceHandlerDelegate
import com.intellij.codeInsight.editorActions.TypedHandlerDelegate
import com.intellij.codeInsight.editorActions.enter.EnterHandlerDelegate
import com.intellij.codeInsight.editorActions.enter.EnterHandlerDelegateAdapter
import com.intellij.openapi.actionSystem.DataContext
import com.intellij.lang.BracePair
import com.intellij.lang.PairedBraceMatcher
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.project.Project
import com.intellij.psi.PsiFile
import com.intellij.psi.tree.IElementType

class HelixMixinBraceMatcher : PairedBraceMatcher {
    override fun getPairs(): Array<BracePair> = arrayOf(
        BracePair(HelixMixinTokenTypes.OPEN_ANGLE, HelixMixinTokenTypes.CLOSE_ANGLE, true),
        BracePair(HelixMixinTokenTypes.DIRECTIVE_OPEN_ANGLE, HelixMixinTokenTypes.DIRECTIVE_CLOSE_ANGLE, true),
        BracePair(HelixMixinTokenTypes.FUNCTION_OPEN_ANGLE, HelixMixinTokenTypes.FUNCTION_CLOSE_ANGLE, true),
        BracePair(HelixMixinTokenTypes.OPEN_PARENTHESIS, HelixMixinTokenTypes.CLOSE_PARENTHESIS, true),
        BracePair(HelixMixinTokenTypes.DIRECTIVE_OPEN_PARENTHESIS,
            HelixMixinTokenTypes.DIRECTIVE_CLOSE_PARENTHESIS, true),
        BracePair(HelixMixinTokenTypes.FUNCTION_OPEN_PARENTHESIS,
            HelixMixinTokenTypes.FUNCTION_CLOSE_PARENTHESIS, true)
    )
    override fun isPairedBracesAllowedBeforeType(leftBraceType: IElementType, contextType: IElementType?) = true
    override fun getCodeConstructStart(file: PsiFile, openingBraceOffset: Int) = openingBraceOffset
}

class HelixMixinTypedHandler : TypedHandlerDelegate() {
    override fun beforeCharTyped(c: Char, project: Project, editor: Editor, file: PsiFile,
                                 fileType: com.intellij.openapi.fileTypes.FileType): Result {
        if (file.language != HelixMixinLanguage || c !in charArrayOf('>', ')')) return Result.CONTINUE
        val offset = editor.caretModel.offset
        if (offset < editor.document.textLength && editor.document.charsSequence[offset] == c) {
            editor.caretModel.moveToOffset(offset + 1)
            return Result.STOP
        }
        return Result.CONTINUE
    }

    override fun charTyped(c: Char, project: Project, editor: Editor, file: PsiFile): Result {
        if (file.language != HelixMixinLanguage || c !in charArrayOf('<', '(')) return Result.CONTINUE
        val offset = editor.caretModel.offset
        val text = editor.document.charsSequence
        val shouldPair = when (c) {
            '<' -> if (offset < 2) false else {
                val beforeOpening = text.subSequence(0, offset - 1)
                val previous = HelixEditorLexer.lex(beforeOpening).lastOrNull {
                    it.kind != HelixEditorTokenKind.Whitespace && it.kind != HelixEditorTokenKind.NewLine
                }
                previous?.let { token ->
                    token.kind == HelixEditorTokenKind.Directive ||
                        token.kind == HelixEditorTokenKind.Function ||
                        token.kind in setOf(HelixEditorTokenKind.DirectiveArgumentDelimiter,
                            HelixEditorTokenKind.ExpressionArgumentDelimiter) &&
                        token.end > token.start && beforeOpening[token.start] == '>'
                } == true
            }
            '(' -> offset >= 2 && text[offset - 2] in charArrayOf('@', '<')
            else -> false
        }
        if (!shouldPair || offset < text.length && text[offset] == if (c == '<') '>' else ')') return Result.CONTINUE
        editor.document.insertString(offset, if (c == '<') ">" else ")")
        return Result.STOP
    }
}

class HelixMixinBackspaceHandler : BackspaceHandlerDelegate() {
    private var closing: Char? = null
    override fun beforeCharDeleted(c: Char, file: PsiFile, editor: Editor) {
        closing = if (file.language == HelixMixinLanguage && c in charArrayOf('<', '(')) {
            val offset = editor.caretModel.offset
            editor.document.charsSequence.getOrNull(offset)?.takeIf { it == if (c == '<') '>' else ')' }
        } else null
    }

    override fun charDeleted(c: Char, file: PsiFile, editor: Editor): Boolean {
        val expected = closing ?: return false
        val offset = editor.caretModel.offset
        if (offset < editor.document.textLength && editor.document.charsSequence[offset] == expected)
            editor.document.deleteString(offset, offset + 1)
        closing = null
        return false
    }
}

class HelixMixinEnterHandler : EnterHandlerDelegateAdapter() {
    override fun postProcessEnter(file: PsiFile, editor: Editor,
                                  dataContext: DataContext): EnterHandlerDelegate.Result {
        if (file.language != HelixMixinLanguage) return EnterHandlerDelegate.Result.Continue
        val document = editor.document
        val caret = editor.caretModel.offset
        if (caret <= 0 || document.textLength == 0) return EnterHandlerDelegate.Result.Continue
        val insertionOffset = (caret - 1).coerceAtLeast(0)
        fun containsStructuredArgument(node: HelixLocalNode): Boolean {
            if (node.kind in STRUCTURED_ARGUMENTS && insertionOffset > node.start && insertionOffset < node.end)
                return true
            return node.children.any(::containsStructuredArgument)
        }
        val isStructuredArgument = containsStructuredArgument(
            HelixMixinFrontendParseCache.parse(document.immutableCharSequence).syntax
        )
        if (!isStructuredArgument) return EnterHandlerDelegate.Result.Continue

        val currentLine = document.getLineNumber(caret.coerceAtMost(document.textLength))
        if (currentLine <= 0) return EnterHandlerDelegate.Result.Continue
        val currentStart = document.getLineStartOffset(currentLine)
        val currentPrefix = document.charsSequence.subSequence(currentStart, caret)
        if (currentPrefix.any { !it.isWhitespace() }) return EnterHandlerDelegate.Result.Continue
        val previousStart = document.getLineStartOffset(currentLine - 1)
        val previousEnd = document.getLineEndOffset(currentLine - 1)
        val previous = document.charsSequence.subSequence(previousStart, previousEnd).toString()
        val baseIndent = previous.takeWhile(Char::isWhitespace)
        val trimmed = previous.trimEnd()
        val indentSize = CodeStyle.getIndentOptions(file).INDENT_SIZE.coerceAtLeast(1)
        val desired = baseIndent + if (trimmed.endsWith('<') || trimmed.endsWith('('))
            " ".repeat(indentSize) else ""
        if (currentPrefix.toString() != desired) {
            document.replaceString(currentStart, caret, desired)
            editor.caretModel.moveToOffset(currentStart + desired.length)
        }
        return EnterHandlerDelegate.Result.Stop
    }

    companion object {
        private val STRUCTURED_ARGUMENTS = setOf(
            "DirectiveArgument", "DeclarationDirectiveArgument", "ReferenceDirectiveArgument",
            "DeclarationReferenceDirectiveArgument", "ExpressionArgument"
        )
    }
}

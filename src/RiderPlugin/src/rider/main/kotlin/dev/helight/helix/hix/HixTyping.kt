package dev.helight.helix.hix

import com.intellij.codeInsight.editorActions.BackspaceHandlerDelegate
import com.intellij.codeInsight.editorActions.TypedHandlerDelegate
import com.intellij.codeInsight.editorActions.enter.EnterHandlerDelegateAdapter
import com.intellij.lang.BracePair
import com.intellij.lang.PairedBraceMatcher
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.project.Project
import com.intellij.psi.PsiFile
import com.intellij.psi.tree.IElementType
import dev.helight.helix.hix.generated.HixLexer

class HixBraceMatcher : PairedBraceMatcher {
    override fun getPairs(): Array<BracePair> = arrayOf(
        pair(HixLexer.LC, HixLexer.RC, true),
        pair(HixLexer.BEGIN_PARAMETERS, HixLexer.END_PARAMETERS),
        pair(HixLexer.BEGIN_ARGUMENT, HixLexer.ARGUMENT_END),
        pair(HixLexer.BEGIN_VALUE_INLINE, HixLexer.VALUE_END_INLINE),
        pair(HixLexer.BEGIN_TUPLE, HixLexer.VALUE_END_INLINE),
        pair(HixLexer.BEGIN_TABLE, HixLexer.VALUE_END_INTERPOLATE),
        pair(HixLexer.BEGIN_VALUE_INTERPOLATE, HixLexer.VALUE_END_INTERPOLATE)
    )
    private fun pair(open: Int, close: Int, structural: Boolean = false) =
        BracePair(HelixAntlrTypes.tokens[open], HelixAntlrTypes.tokens[close], structural)
    override fun isPairedBracesAllowedBeforeType(leftBraceType: IElementType, contextType: IElementType?) = true
    override fun getCodeConstructStart(file: PsiFile, openingBraceOffset: Int) = openingBraceOffset
}

class HixTypedHandler : TypedHandlerDelegate() {
    override fun charTyped(c: Char, project: Project, editor: Editor, file: PsiFile): Result {
        if (file.language != HixLanguage || c !in "<([{") return Result.CONTINUE
        val offset = editor.caretModel.offset
        val text = editor.document.charsSequence
        val token = HixAntlrSyntax.parse(text).tokens.firstOrNull { it.start <= offset - 1 && it.end >= offset }
            ?: return Result.CONTINUE
        if (token.type !in setOf(HixLexer.BEGIN_ARGUMENT, HixLexer.BEGIN_PARAMETERS, HixLexer.BEGIN_VALUE_INLINE,
                HixLexer.BEGIN_TUPLE, HixLexer.BEGIN_TABLE, HixLexer.LC)) return Result.CONTINUE
        val close = when (c) { '<' -> '>'; '(' -> ')'; '[' -> ']'; else -> '}' }
        if (text.getOrNull(offset) != close) editor.document.insertString(offset, close.toString())
        return Result.CONTINUE
    }
}

class HixBackspaceHandler : BackspaceHandlerDelegate() {
    private var closing: Char? = null
    override fun beforeCharDeleted(c: Char, file: PsiFile, editor: Editor) {
        val expected = when (c) { '<' -> '>'; '(' -> ')'; '[' -> ']'; '{' -> '}'; else -> null }
        closing = expected?.takeIf { file.language == HixLanguage &&
            editor.document.charsSequence.getOrNull(editor.caretModel.offset) == it }
    }
    override fun charDeleted(c: Char, file: PsiFile, editor: Editor): Boolean {
        val expected = closing ?: return false
        closing = null
        val offset = editor.caretModel.offset
        if (editor.document.charsSequence.getOrNull(offset) == expected) editor.document.deleteString(offset, offset + 1)
        return false
    }
}

class HixEnterHandler : EnterHandlerDelegateAdapter()

package dev.helight.helix.mixin

import com.intellij.codeInsight.editorActions.BackspaceHandlerDelegate
import com.intellij.codeInsight.editorActions.TypedHandlerDelegate
import com.intellij.codeInsight.editorActions.enter.EnterHandlerDelegateAdapter
import com.intellij.lang.BracePair
import com.intellij.lang.PairedBraceMatcher
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.project.Project
import com.intellij.psi.PsiFile
import com.intellij.psi.tree.IElementType
import dev.helight.helix.mixin.generated.MixinLexer

class HelixMixinBraceMatcher : PairedBraceMatcher {
    override fun getPairs(): Array<BracePair> = arrayOf(
        pair(MixinLexer.LC, MixinLexer.RC, true),
        pair(MixinLexer.BEGIN_PARAMETERS, MixinLexer.END_PARAMETERS),
        pair(MixinLexer.BEGIN_ARGUMENT, MixinLexer.ARGUMENT_END),
        pair(MixinLexer.BEGIN_VALUE_INLINE, MixinLexer.VALUE_END_INLINE),
        pair(MixinLexer.BEGIN_TUPLE, MixinLexer.VALUE_END_INLINE),
        pair(MixinLexer.BEGIN_TABLE, MixinLexer.VALUE_END_INTERPOLATE),
        pair(MixinLexer.BEGIN_VALUE_INTERPOLATE, MixinLexer.VALUE_END_INTERPOLATE)
    )
    private fun pair(open: Int, close: Int, structural: Boolean = false) =
        BracePair(HelixAntlrTypes.tokens[open], HelixAntlrTypes.tokens[close], structural)
    override fun isPairedBracesAllowedBeforeType(leftBraceType: IElementType, contextType: IElementType?) = true
    override fun getCodeConstructStart(file: PsiFile, openingBraceOffset: Int) = openingBraceOffset
}

class HelixMixinTypedHandler : TypedHandlerDelegate() {
    override fun charTyped(c: Char, project: Project, editor: Editor, file: PsiFile): Result {
        if (file.language != HelixMixinLanguage || c !in "<([{") return Result.CONTINUE
        val offset = editor.caretModel.offset
        val text = editor.document.charsSequence
        val token = HelixMixinAntlrSyntax.parse(text).tokens.firstOrNull { it.start <= offset - 1 && it.end >= offset }
            ?: return Result.CONTINUE
        if (token.type !in setOf(MixinLexer.BEGIN_ARGUMENT, MixinLexer.BEGIN_PARAMETERS, MixinLexer.BEGIN_VALUE_INLINE,
                MixinLexer.BEGIN_TUPLE, MixinLexer.BEGIN_TABLE, MixinLexer.LC)) return Result.CONTINUE
        val close = when (c) { '<' -> '>'; '(' -> ')'; '[' -> ']'; else -> '}' }
        if (text.getOrNull(offset) != close) editor.document.insertString(offset, close.toString())
        return Result.CONTINUE
    }
}

class HelixMixinBackspaceHandler : BackspaceHandlerDelegate() {
    private var closing: Char? = null
    override fun beforeCharDeleted(c: Char, file: PsiFile, editor: Editor) {
        val expected = when (c) { '<' -> '>'; '(' -> ')'; '[' -> ']'; '{' -> '}'; else -> null }
        closing = expected?.takeIf { file.language == HelixMixinLanguage &&
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

class HelixMixinEnterHandler : EnterHandlerDelegateAdapter()

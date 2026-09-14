package dev.helight.helix.hix

import com.intellij.codeInsight.editorActions.BackspaceHandlerDelegate
import com.intellij.codeInsight.editorActions.enter.EnterBetweenBracesNoCommitDelegate
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.util.Key
import com.intellij.psi.PsiFile
import com.intellij.psi.tree.IElementType
import dev.helight.helix.hal.HalAntlrTypes
import dev.helight.helix.hal.HalLanguage
import dev.helight.helix.hal.generated.HalLexer
import dev.helight.helix.hix.generated.HixLexer

/** Lets the platform insert both newlines and retain the caret on the inner line. */
class HixBracketEnterDelegate : EnterBetweenBracesNoCommitDelegate() {
    override fun isBracePair(left: Char, right: Char): Boolean = paired(left, right)
    override fun isCommentType(type: IElementType): Boolean = type in setOf(
        HelixAntlrTypes.tokens[HixLexer.COMMENT], HelixAntlrTypes.tokens[HixLexer.SLASH_COMMENT],
        HalAntlrTypes.tokens[HalLexer.LINE_COMMENT])

    companion object {
        internal fun paired(left: Char, right: Char) = when (left) {
            '[' -> right == ']'
            '{' -> right == '}'
            '(' -> right == ')'
            else -> false
        }
    }
}

/** Preserve the inner indent when joining an otherwise empty closing-bracket line. */
class HixBracketBackspaceDelegate : BackspaceHandlerDelegate() {
    override fun beforeCharDeleted(c: Char, file: PsiFile, editor: Editor) {
        editor.putUserData(joinKey, null)
        if (file.language != HixLanguage && file.language != HalLanguage) return
        if (editor.selectionModel.hasSelection()) return
        join(editor.document.text, editor.caretModel.offset)?.let { editor.putUserData(joinKey, it) }
    }
    override fun charDeleted(c: Char, file: PsiFile, editor: Editor): Boolean {
        val plan = editor.getUserData(joinKey) ?: return false
        editor.putUserData(joinKey, null)
        editor.document.deleteString(plan.first, plan.second - 1)
        editor.caretModel.moveToOffset(plan.first)
        return true
    }
    companion object {
        private val joinKey = Key.create<Pair<Int, Int>>("helix.closing.bracket.backspace")
        internal fun join(source: String, offset: Int): Pair<Int, Int>? {
            if (source.getOrNull(offset) !in setOf(']', '}', ')')) return null
            val start = source.lastIndexOf('\n', offset - 1) + 1
            if (start == 0 || source.substring(start, offset).any { it != ' ' && it != '\t' }) return null
            val previousStart = source.lastIndexOf('\n', start - 2) + 1
            if (source.substring(previousStart, start - 1).any { it != ' ' && it != '\t' }) return null
            return start - 1 to offset
        }
    }
}

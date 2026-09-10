package dev.helight.helix.hix

import com.intellij.codeInsight.editorActions.MultiCharQuoteHandler
import com.intellij.codeInsight.editorActions.TypedHandlerDelegate
import com.intellij.lang.BracePair
import com.intellij.lang.PairedBraceMatcher
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.editor.highlighter.HighlighterIterator
import com.intellij.openapi.project.Project
import com.intellij.psi.PsiFile
import com.intellij.psi.tree.IElementType
import dev.helight.helix.hix.generated.HixLexer

class HixBraceMatcher : PairedBraceMatcher {
    override fun getPairs(): Array<BracePair> = arrayOf(
        pair(HixLexer.LC, HixLexer.RC, true),
        pair(HixLexer.BEGIN_PARAMETERS, HixLexer.END_PARAMETERS),
        BracePair(HelixAntlrTypes.tokens[HixLexer.BEGIN_VALUE_INLINE], HixEditorTokenTypes.INLINE_END, false),
        BracePair(HelixAntlrTypes.tokens[HixLexer.BEGIN_METADATA_VALUE], HixEditorTokenTypes.METADATA_VALUE_END, false),
        BracePair(HelixAntlrTypes.tokens[HixLexer.BEGIN_TUPLE], HixEditorTokenTypes.TUPLE_END, false),
        BracePair(HelixAntlrTypes.tokens[HixLexer.BEGIN_TABLE], HixEditorTokenTypes.TABLE_END, false),
        BracePair(HelixAntlrTypes.tokens[HixLexer.BEGIN_VALUE_INTERPOLATE],
            HixEditorTokenTypes.INTERPOLATION_END, false)
    )
    private fun pair(open: Int, close: Int, structural: Boolean = false) =
        BracePair(HelixAntlrTypes.tokens[open], HelixAntlrTypes.tokens[close], structural)
    override fun isPairedBracesAllowedBeforeType(leftBraceType: IElementType, contextType: IElementType?) = true
    override fun getCodeConstructStart(file: PsiFile, openingBraceOffset: Int) = openingBraceOffset
}

class HixQuoteHandler : MultiCharQuoteHandler {
    private val begin = HelixAntlrTypes.tokens[HixLexer.BEGIN_ARGUMENT]
    private val end = HelixAntlrTypes.tokens[HixLexer.ARGUMENT_END]
    private val literal = setOf(begin, end, HelixAntlrTypes.tokens[HixLexer.ARGUMENT_TEXT])

    override fun isOpeningQuote(iterator: HighlighterIterator, offset: Int): Boolean =
        !iterator.atEnd() && iterator.tokenType == begin && iterator.start == offset

    override fun isClosingQuote(iterator: HighlighterIterator, offset: Int): Boolean =
        !iterator.atEnd() && iterator.tokenType == end && iterator.start == offset

    override fun hasNonClosedLiteral(editor: Editor, iterator: HighlighterIterator, offset: Int): Boolean {
        val text = editor.document.charsSequence
        val lineEnd = editor.document.getLineEndOffset(editor.document.getLineNumber(offset))
        var escaped = false
        for (index in offset.coerceAtLeast(0) until lineEnd) {
            val character = text[index]
            if (!escaped && character == '>') return false
            escaped = !escaped && character == '\\'
            if (character != '\\') escaped = false
        }
        return true
    }

    override fun isInsideLiteral(iterator: HighlighterIterator): Boolean =
        !iterator.atEnd() && iterator.tokenType in literal

    override fun getClosingQuote(iterator: HighlighterIterator, offset: Int): CharSequence? {
        if (offset == 0) return null
        if (iterator.atEnd() || iterator.start == offset) iterator.retreat()
        return if (!iterator.atEnd() && iterator.tokenType == begin && iterator.start + 1 == offset) ">" else null
    }
}

/**
 * IntelliJ's generic typed handler only initiates its built-in character pairs for the standard
 * Java-like delimiters. The editor highlighter may also still expose the tokenization from before
 * the keystroke when this callback runs. Pairing therefore follows Hix delimiter characters
 * directly; [HixBraceMatcher] and [HixQuoteHandler] remain responsible for structural matching.
 */
class HixDelimiterTypedHandler : TypedHandlerDelegate() {
    override fun beforeCharTyped(c: Char, project: Project, editor: Editor, file: PsiFile,
                                 fileType: com.intellij.openapi.fileTypes.FileType): Result {
        if (file.language != HixLanguage) return Result.CONTINUE
        val offset = editor.caretModel.offset
        if (editor.document.charsSequence.getOrNull(offset) != c) return Result.CONTINUE
        if (c !in closingCharacters) return Result.CONTINUE
        editor.caretModel.moveToOffset(offset + 1)
        return Result.STOP
    }

    override fun charTyped(c: Char, project: Project, editor: Editor, file: PsiFile): Result {
        if (file.language != HixLanguage) return Result.CONTINUE
        val offset = editor.caretModel.offset
        val closing = pairs[c] ?: return Result.CONTINUE
        if (editor.document.charsSequence.getOrNull(offset) != closing)
            editor.document.insertString(offset, closing.toString())
        return Result.STOP
    }

    private companion object {
        val pairs = mapOf('(' to ')', '[' to ']', '{' to '}', '<' to '>')
        val closingCharacters = pairs.values.toSet()
    }
}

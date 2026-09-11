package dev.helight.helix.hix

import com.intellij.lang.Language
import com.intellij.application.options.CodeStyle
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.project.Project
import com.intellij.psi.codeStyle.lineIndent.LineIndentProvider

/** Fast, document-based indentation used by Enter and smart backspace. */
class HixLineIndentProvider : LineIndentProvider {
    override fun isSuitableFor(language: Language?): Boolean = language?.isKindOf(HixLanguage) == true

    override fun getLineIndent(project: Project, editor: Editor, language: Language?, offset: Int): String? {
        val document = editor.document
        if (document.lineCount == 0) return ""
        val line = document.getLineNumber(offset.coerceIn(0, document.textLength))
        val lines = (0 until document.lineCount).map {
            document.charsSequence.subSequence(document.getLineStartOffset(it), document.getLineEndOffset(it)).toString()
        }
        val indentSize = CodeStyle.getSettings(project).getCommonSettings(HixLanguage)
            .indentOptions?.INDENT_SIZE?.coerceAtLeast(1) ?: 2
        return calculateIndent(lines, line, indentSize)
    }

    companion object {
        internal fun calculateIndent(lines: List<String>, line: Int, indentSize: Int): String {
            if (line <= 0 || lines.isEmpty()) return ""
            val current = lines.getOrElse(line) { "" }.trimStart()
            val previousIndex = (line - 1 downTo 0).firstOrNull { lines[it].isNotBlank() } ?: return ""
            val previous = lines[previousIndex]
            val previousIndent = previous.takeWhile { it == ' ' || it == '\t' }
            val trimmedPrevious = previous.trimEnd()

            var width = previousIndent.fold(0) { value, character ->
                value + if (character == '\t') indentSize else 1
            }
            if (trimmedPrevious.lastOrNull() in setOf('{', '(', '[')) width += indentSize
            if (current.startsWith("}") || current.startsWith(")") || current.startsWith("]"))
                width = (width - indentSize).coerceAtLeast(0)
            return " ".repeat(width)
        }
    }
}

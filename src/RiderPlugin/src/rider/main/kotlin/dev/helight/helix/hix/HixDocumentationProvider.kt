package dev.helight.helix.hix

import com.intellij.lang.documentation.DocumentationProvider
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.util.text.StringUtil
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile

class HixDocumentationProvider : DocumentationProvider {
    override fun getCustomDocumentationElement(editor: Editor, file: PsiFile,
                                               contextElement: PsiElement?, targetOffset: Int): PsiElement? =
        file.findElementAt(targetOffset.coerceIn(0, (file.textLength - 1).coerceAtLeast(0)))

    override fun getQuickNavigateInfo(element: PsiElement, originalElement: PsiElement?): String? = documentation(element)
    override fun generateHoverDoc(element: PsiElement, originalElement: PsiElement?): String? = documentation(element)
    override fun generateDoc(element: PsiElement, originalElement: PsiElement?): String? {
        val fact = fact(element) ?: return null
        val type = StringUtil.escapeXmlEntities(fact.type)
        val body = StringUtil.escapeXmlEntities(fact.documentation).replace("\n", "<br>")
        return "<div class='definition'><code>$type</code></div><div class='content'>$body</div>"
    }

    private fun documentation(element: PsiElement): String? = fact(element)?.let {
        if (it.documentation.isBlank()) it.type else "${it.type}\n${it.documentation}"
    }

    private fun fact(element: PsiElement) = element.containingFile?.let { file ->
        val service = HixSnapshotService.getInstance(file.project)
        val path = file.virtualFile?.path
        val snapshot = file.getUserData(HixSnapshotService.SEMANTIC_SNAPSHOT)
            ?: service.snapshotForText(file.text, path)
        val offset = element.textRange.startOffset
        snapshot?.typeFacts?.filter { offset >= it.range.startOffset && offset < it.range.endOffset }
            ?.minByOrNull { it.range.endOffset - it.range.startOffset }
    }
}

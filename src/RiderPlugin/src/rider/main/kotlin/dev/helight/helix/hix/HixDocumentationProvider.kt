package dev.helight.helix.hix

import com.intellij.lang.documentation.DocumentationProvider
import com.intellij.navigation.ItemPresentation
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.util.text.StringUtil
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile
import com.intellij.psi.PsiManager
import com.intellij.psi.impl.FakePsiElement
import dev.helight.helix.protocol.MixinCompletionItem
import dev.helight.helix.protocol.MixinLanguageDefinition
import dev.helight.helix.protocol.MixinTypeFact
import javax.swing.Icon

class HixDocumentationProvider : DocumentationProvider {
    override fun getCustomDocumentationElement(editor: Editor, file: PsiFile,
                                               contextElement: PsiElement?, targetOffset: Int): PsiElement? =
        file.findElementAt(targetOffset.coerceIn(0, (file.textLength - 1).coerceAtLeast(0)))

    override fun getDocumentationElementForLookupItem(psiManager: PsiManager, item: Any,
                                                      element: PsiElement): PsiElement? = when (item) {
        is MixinLanguageDefinition -> LookupDocumentation(element, HixLookup.signature(item), item.documentation)
        is MixinCompletionItem -> LookupDocumentation(element, item.name, item.documentation)
        else -> null
    }

    override fun getQuickNavigateInfo(element: PsiElement, originalElement: PsiElement?): String? =
        documentation(element, originalElement)
    override fun generateHoverDoc(element: PsiElement, originalElement: PsiElement?): String? =
        documentation(element, originalElement)
    override fun generateDoc(element: PsiElement, originalElement: PsiElement?): String? =
        documentation(element, originalElement)

    private fun documentation(element: PsiElement, originalElement: PsiElement?): String? {
        if (element is LookupDocumentation) return render(element.title, element.documentation)
        val context = originalElement ?: element
        val file = context.containingFile ?: return null
        val service = HixSnapshotService.getInstance(file.project)
        val definitions = service.definitionsFor(file.text)
        file.virtualFile?.let { service.observe(it, file.text) }
        val snapshot = service.snapshotForText(file.text, file.virtualFile?.path)
        val offset = context.textRange.startOffset
        val parsed = HixAntlrSyntax.parse(file.text)
        val metadataArgument = HixLookup.metadataArgumentAt(parsed, offset)
        val metadataDefinition = metadataArgument?.let { argument ->
            definitions.firstOrNull { it.kind == "PatternMetadata" && it.name == argument.name }
        }
        val metadataArgumentKind = metadataArgument?.let { argument ->
            metadataDefinition?.argumentTypes?.getOrNull(argument.index)
                ?: if (metadataDefinition?.variadic == true) metadataDefinition.argumentTypes.lastOrNull() else null
        }
        val role = if (metadataArgumentKind == "Pattern") HixLookup.SemanticRole.Pattern
            else HixLookup.semanticRoleAt(parsed, offset)
        val catalogKind = when (role) {
            HixLookup.SemanticRole.Pattern -> "Kind"
            HixLookup.SemanticRole.PatternMetadata -> "PatternMetadata"
            HixLookup.SemanticRole.FileMetadata -> "FileMetadata"
            HixLookup.SemanticRole.Metadata -> null
            null -> null
        }
        if (catalogKind != null) {
            definitions.firstOrNull { it.name == context.text && it.kind == catalogKind }?.let {
                return render(it.name, it.documentation)
            }
            if (role == HixLookup.SemanticRole.PatternMetadata) return null
        }
        if (role == HixLookup.SemanticRole.Metadata) return null
        if (metadataArgument != null && metadataDefinition == null) return null
        if (role == HixLookup.SemanticRole.Pattern) snapshot?.references?.firstOrNull {
            it.kind == "Pattern" && offset >= it.range.startOffset && offset < it.range.endOffset
        }?.let { reference ->
            val owner = service.snapshot(reference.targetFilePath)
            owner?.typeFacts?.firstOrNull { fact ->
                fact.range.startOffset == reference.targetRange.startOffset &&
                    fact.range.endOffset == reference.targetRange.endOffset
            }?.let { return render(it.type, it.documentation) }
        }
        val fact = snapshot?.typeFacts?.filter { offset >= it.range.startOffset && offset < it.range.endOffset }
            ?.minWithOrNull(compareBy({ if (it.documentation.isBlank()) 1 else 0 },
                { it.range.endOffset - it.range.startOffset }, { if (it.kind == "DynamicCall") 0 else 1 }))
        if (fact != null && fact.documentation.isNotBlank()) return render(fact)
        val site = HixLookup.site(parsed, context.textRange.endOffset)
        val receiver = HixLookup.receiverType(parsed, site.receiverEnd, snapshot)
        val matchingDefinitions = definitions.filter { role == null && it.name == context.text &&
            (!site.chained || HixLookup.acceptsReceiver(it, receiver)) && !site.member }
        if (matchingDefinitions.isNotEmpty()) return matchingDefinitions.distinctBy { HixLookup.signature(it) }
            .joinToString("<br>") { render(HixLookup.signature(it), it.documentation) }
        return fact?.let(::render)
    }

    private fun render(fact: MixinTypeFact): String {
        val lines = fact.documentation.lines()
        val signatures = lines.filter(::isSignature)
        if (signatures.isEmpty()) return render(fact.type, fact.documentation)
        val documentation = lines.filterNot(::isSignature).joinToString("\n").trim()
        return render(signatures.joinToString("\n"), documentation)
    }

    private fun isSignature(line: String): Boolean {
        val open = line.indexOf('(')
        val close = line.indexOf(") -> ", open + 1)
        return open > 0 && close > open
    }

    private fun render(title: String, documentation: String): String =
        "<div class='definition'><code>${StringUtil.escapeXmlEntities(title)}</code></div>" +
            if (documentation.isBlank()) "" else
                "<div class='content'>${StringUtil.escapeXmlEntities(documentation).replace("\n", "<br>")}</div>"

    private class LookupDocumentation(private val context: PsiElement, val title: String,
                                      val documentation: String) : FakePsiElement() {
        override fun getParent(): PsiElement = context
        override fun getName(): String = title
        override fun getPresentation(): ItemPresentation = object : ItemPresentation {
            override fun getPresentableText(): String = title
            override fun getLocationString(): String = "Hix"
            override fun getIcon(unused: Boolean): Icon? = null
        }
        override fun canNavigate(): Boolean = false
        override fun canNavigateToSource(): Boolean = false
        override fun navigate(requestFocus: Boolean) = Unit
    }
}

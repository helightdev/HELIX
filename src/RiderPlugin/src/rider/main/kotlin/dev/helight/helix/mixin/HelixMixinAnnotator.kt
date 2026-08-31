package dev.helight.helix.mixin

import com.intellij.lang.annotation.AnnotationHolder
import com.intellij.lang.annotation.Annotator
import com.intellij.lang.annotation.HighlightSeverity
import com.intellij.openapi.util.TextRange
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile
import com.intellij.psi.util.childrenOfType
import com.intellij.psi.util.elementType

class HelixMixinAnnotator : Annotator {
    override fun annotate(element: PsiElement, holder: AnnotationHolder) {
        if (element !is PsiFile || element.language != HelixMixinLanguage) return
        val path = element.virtualFile.getUserData(HelixMixinSnapshotService.ORIGINAL_PATH)
            ?: element.virtualFile.path

        val snapshot = element.getUserData(HelixMixinSnapshotService.SEMANTIC_SNAPSHOT)
            ?.takeIf { it.sourceHash == HelixMixinSnapshotService.sourceHash(element.text) }
            ?: HelixMixinSnapshotService.getInstance(element.project).snapshotForText(element.text, path)
            ?: return
        snapshot.diagnostics.forEach { diagnostic ->
            val start = diagnostic.range.startOffset.coerceIn(0, element.textLength)
            val end = diagnostic.range.endOffset.coerceIn(start, element.textLength)
            val severity = if (diagnostic.severity == "Warning") HighlightSeverity.WARNING else HighlightSeverity.ERROR
            holder.newAnnotation(severity, diagnostic.message).range(TextRange(start, end)).create()
        }

        snapshot.declarations.forEach { declaration ->
            semanticColor(declaration.kind)?.let { color ->
                val range = clamped(declaration.range.startOffset, declaration.range.endOffset, element.textLength)
                if (!range.isEmpty) holder.newSilentAnnotation(HighlightSeverity.INFORMATION)
                    .range(range).textAttributes(color).create()
            }
        }
        snapshot.references.forEach { reference ->
            if (!isDirectArgumentReference(element, reference.range.startOffset)) return@forEach
            semanticColor(reference.kind)?.let { color ->
                val range = clamped(reference.range.startOffset, reference.range.endOffset, element.textLength)
                if (!range.isEmpty) holder.newSilentAnnotation(HighlightSeverity.INFORMATION)
                    .range(range).textAttributes(color).create()
            }
        }
    }

    private fun semanticColor(kind: String) = when (kind) {
        "Label" -> HelixMixinColors.LABEL
        "Local" -> HelixMixinColors.LOCAL
        "Variable", "TargetVariable", "Carry" -> HelixMixinColors.VARIABLE
        "Function" -> HelixMixinColors.DELEGATE
        "CSharpType", "Annotation", "Derivation" -> HelixMixinColors.TYPE
        else -> null
    }

    private fun clamped(start: Int, end: Int, length: Int): TextRange {
        val safeStart = start.coerceIn(0, length)
        return TextRange(safeStart, end.coerceIn(safeStart, length))
    }

    private fun isDirectArgumentReference(file: PsiFile, start: Int): Boolean {
        var current: PsiElement? = file.findElementAt(start.coerceIn(0, (file.textLength - 1).coerceAtLeast(0)))
        while (current != null && current !== file) {
            when (current.node.elementType) {
                HelixMixinElementTypes.REFERENCE_EXPRESSION -> return false
                HelixMixinElementTypes.DIRECTIVE_ARGUMENT,
                HelixMixinElementTypes.FUNCTION_ARGUMENT -> return true
            }
            current = current.parent
        }
        return false
    }
}

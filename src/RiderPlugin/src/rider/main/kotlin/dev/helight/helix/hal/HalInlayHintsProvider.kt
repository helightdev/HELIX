package dev.helight.helix.hal

import com.intellij.codeInsight.hints.declarative.InlayHintsCollector
import com.intellij.codeInsight.hints.declarative.InlayHintsProvider
import com.intellij.codeInsight.hints.declarative.InlayTreeSink
import com.intellij.codeInsight.hints.declarative.InlineInlayPosition
import com.intellij.codeInsight.hints.declarative.SharedBypassCollector
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.project.DumbAware
import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile
import dev.helight.helix.hal.generated.HalParser
import org.antlr.v4.runtime.ParserRuleContext

class HalInlayHintsProvider : InlayHintsProvider, DumbAware {
    override fun createCollector(file: PsiFile, editor: Editor): InlayHintsCollector {
        val source = file.text
        val manifest = HalManifest.text(file.virtualFile)
        val isManifest = file.virtualFile?.name?.equals("manifest.hal", true) == true
        val hints = fieldHints(source, manifest, isManifest)
        return object : SharedBypassCollector {
            override fun collectFromElement(element: PsiElement, sink: InlayTreeSink) {
                if (element !== file) return
                hints.forEach { (offset, type) ->
                    sink.addPresentation(InlineInlayPosition(offset, true), tooltip = "Schema field type: $type", hasBackground = true) {
                        text(": $type")
                    }
                }
            }
        }
    }
    companion object {
        internal fun fieldHints(source: String, schema: String, isManifest: Boolean = false): List<Pair<Int, String>> {
            fun rules(rule: ParserRuleContext): Sequence<ParserRuleContext> = sequence {
                yield(rule)
                rule.children.orEmpty().filterIsInstance<ParserRuleContext>().forEach { yieldAll(rules(it)) }
            }
            return rules(HalAntlrSyntax.parse(source).tree).mapNotNull { rule ->
                val key = when (rule) { is HalParser.FieldContext -> rule.fieldKey(); is HalParser.TableEntryContext -> rule.fieldKey(); else -> null }
                    ?: return@mapNotNull null
                val name = key.text.removeSurrounding("\"")
                val scope = HalStructure.completionScope(source, key.start.startIndex) ?: return@mapNotNull null
                val fields = when {
                    isManifest && scope.first == "Manifest" -> HalManifest.manifestFields
                    isManifest && scope.first == "ManifestSources" -> HalManifest.sourceFields
                    else -> HalManifest.fieldDefinitions(schema, scope.first)
                }
                val field = fields.firstOrNull { it.name == name } ?: return@mapNotNull null
                (key.stop.stopIndex + 1) to (if (field.many) "${field.type}[]" else field.type)
            }.distinct().toList()
        }
    }
}

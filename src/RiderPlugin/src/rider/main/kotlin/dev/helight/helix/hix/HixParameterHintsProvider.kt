package dev.helight.helix.hix

import com.intellij.codeInsight.hints.HintInfo
import com.intellij.codeInsight.hints.InlayInfo
import com.intellij.codeInsight.hints.InlayParameterHintsProvider
import com.intellij.psi.PsiElement
import dev.helight.helix.hal.HalAntlrTypes
import dev.helight.helix.hal.HalLanguage
import dev.helight.helix.hal.generated.HalParser
import dev.helight.helix.hix.generated.HixParser

open class HixParameterHintsProvider : InlayParameterHintsProvider {
    override fun getDefaultBlackList() = emptySet<String>()
    override fun getDescription() = "Parameter names in positional Hix calls. Explicit named arguments and chain receivers are omitted."
    private fun call(element: PsiElement): HixEditorCall? {
        val file = element.containingFile ?: return null
        val type = element.node?.elementType
        if (file.language == HalLanguage) {
            if (type != HalAntlrTypes.rules[HalParser.RULE_call]) return null
        } else if (type !in setOf(HelixAntlrTypes.rules[HixParser.RULE_callValueList],
                HelixAntlrTypes.rules[HixParser.RULE_metadata])) return null
        return HixEditorCalls.calls(file).firstOrNull { call ->
            if (file.language == HalLanguage || type == HelixAntlrTypes.rules[HixParser.RULE_metadata])
                call.start == element.textRange.startOffset else call.anchor == element.textRange.startOffset
        }
    }
    override fun getParameterHints(element: PsiElement): List<InlayInfo> {
        val call = call(element) ?: return emptyList()
        val signatures = call.resolved?.let { listOf(it) }
            ?: HixEditorCalls.signatures(element.containingFile, call).filter { it.accepts(call) }
        return hints(call, signatures).map { (name, offset) -> InlayInfo(name, offset) }
    }
    override fun getHintInfo(element: PsiElement): HintInfo? {
        val call = call(element) ?: return null
        val signatures = call.resolved?.let { listOf(it) }
            ?: HixEditorCalls.signatures(element.containingFile, call).filter { it.accepts(call) }
        val signature = signatures.singleOrNull() ?: return null
        return HintInfo.MethodInfo(signature.name, signature.parameters.map { it.name }, element.language)
    }
    companion object {
        internal fun hints(call: HixEditorCall, signatures: List<HixEditorSignature>): List<Pair<String, Int>> {
            if (signatures.isEmpty()) return emptyList()
            return call.arguments.mapIndexedNotNull { index, argument ->
                if (argument.name != null) return@mapIndexedNotNull null
                if (signatures.any { it.parameters.getOrNull(it.parameter(call, index))?.obvious == true }) return@mapIndexedNotNull null
                val names = signatures.mapNotNull { signature ->
                    signature.parameters.getOrNull(signature.parameter(call, index))?.name
                }.distinct()
                if (signatures.any { it.parameter(call, index) < 0 }) return@mapIndexedNotNull null
                if (index > 0 && signatures.all {
                        it.parameters.getOrNull(it.parameter(call, index))?.variadic == true &&
                            it.parameter(call, index - 1) == it.parameter(call, index)
                    }) return@mapIndexedNotNull null
                val name = names.singleOrNull()?.takeIf { it.isNotEmpty() && it.toIntOrNull() == null } ?: return@mapIndexedNotNull null
                if (argument.text.trim().removePrefix("$") == name) return@mapIndexedNotNull null
                name to argument.start
            }
        }
    }
}

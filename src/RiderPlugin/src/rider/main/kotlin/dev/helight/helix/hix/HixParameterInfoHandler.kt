package dev.helight.helix.hix

import com.intellij.codeInsight.CodeInsightSettings
import com.intellij.lang.parameterInfo.CreateParameterInfoContext
import com.intellij.lang.parameterInfo.ParameterInfoHandler
import com.intellij.lang.parameterInfo.ParameterInfoUIContext
import com.intellij.lang.parameterInfo.UpdateParameterInfoContext
import com.intellij.openapi.project.DumbAware
import com.intellij.psi.PsiElement

class HixParameterInfoItem(val declaration: HixEditorSignature) {
    @Volatile var signature: HixEditorSignature = declaration
        private set
    fun update(call: HixEditorCall) {
        val resolved = call.resolved
        signature = if (resolved?.instantiates(declaration) == true) resolved.copy(name = declaration.name) else declaration
    }
}

/** All parsing/signature discovery happens outside updateUI, which only renders immutable data. */
open class HixParameterInfoHandler : ParameterInfoHandler<PsiElement, HixParameterInfoItem>, DumbAware {
    override fun findElementForParameterInfo(context: CreateParameterInfoContext): PsiElement? {
        val call = HixEditorCalls.at(HixEditorCalls.calls(context.file), context.offset) ?: return null
        val signatures = HixEditorCalls.signatures(context.file, call)
        if (signatures.isEmpty()) return null
        val owner = HixEditorCalls.owner(context.file, call) ?: return null
        owner.putUserData(HixEditorCalls.CURRENT_CALL, call)
        context.itemsToShow = signatures.map(::HixParameterInfoItem).toTypedArray()
        return owner
    }
    override fun showParameterInfo(element: PsiElement, context: CreateParameterInfoContext) {
        val call = element.getUserData(HixEditorCalls.CURRENT_CALL) ?: return
        context.showHint(element, call.anchor, this)
    }
    override fun findElementForUpdatingParameterInfo(context: UpdateParameterInfoContext): PsiElement? {
        val call = HixEditorCalls.at(HixEditorCalls.calls(context.file), context.offset) ?: return null
        if (call.anchor != context.parameterListStart) return null
        val previous = context.parameterOwner?.getUserData(HixEditorCalls.CURRENT_CALL)
        if (previous != null && (previous.name != call.name || previous.kind != call.kind || previous.chained != call.chained)) return null
        val owner = HixEditorCalls.owner(context.file, call) ?: return null
        owner.putUserData(HixEditorCalls.CURRENT_CALL, call)
        return owner
    }
    override fun updateParameterInfo(element: PsiElement, context: UpdateParameterInfoContext) {
        val call = element.getUserData(HixEditorCalls.CURRENT_CALL) ?: run { context.removeHint(); return }
        context.parameterOwner = element
        context.setCurrentParameter(call.currentArgument(context.offset))
        context.objectsToView.forEachIndexed { index, item ->
            (item as? HixParameterInfoItem)?.update(call)
            context.setUIComponentEnabled(index, (item as? HixParameterInfoItem)?.signature?.accepts(call) == true)
        }
    }
    override fun updateUI(item: HixParameterInfoItem, context: ParameterInfoUIContext) {
        val call = context.parameterOwner?.getUserData(HixEditorCalls.CURRENT_CALL)
        val presentation = presentation(item.signature, call, context.currentParameterIndex,
            CodeInsightSettings.getInstance().SHOW_FULL_SIGNATURES_IN_PARAMETER_INFO)
        context.setupUIComponentPresentation(presentation.text, presentation.start, presentation.end,
            !context.isUIComponentEnabled, false, false, context.defaultParameterColor)
    }
    companion object {
        internal data class Presentation(val text: String, val start: Int, val end: Int)
        internal fun presentation(signature: HixEditorSignature, call: HixEditorCall?, index: Int, full: Boolean): Presentation {
            val selected = call?.let { signature.parameter(it, index) } ?: -1
            val prefix = if (full) signature.name + "(" else ""
            val parts = signature.parameters.map { it.display }
            val parameters = if (parts.isEmpty()) "no parameters" else parts.joinToString(", ")
            val text = prefix + parameters + if (full) ") -> " + signature.result else ""
            val start = if (selected in parts.indices) prefix.length + parts.take(selected).sumOf { it.length + 2 } else -1
            return Presentation(text, start, if (start >= 0) start + parts[selected].length else -1)
        }
    }
    override fun isWhitespaceSensitive() = true
}

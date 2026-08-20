package dev.helight.helix.expression

import com.intellij.codeInsight.codeVision.CodeVisionAnchorKind
import com.intellij.codeInsight.codeVision.CodeVisionEntry
import com.intellij.codeInsight.codeVision.CodeVisionProvider
import com.intellij.codeInsight.codeVision.CodeVisionRelativeOrdering
import com.intellij.codeInsight.codeVision.CodeVisionState
import com.intellij.codeInsight.codeVision.ui.model.ClickableTextCodeVisionEntry
import com.intellij.openapi.components.service
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.util.TextRange
import com.jetbrains.rd.framework.impl.RpcTimeouts
import com.jetbrains.rider.projectView.solution
import dev.helight.helix.HelixMessagesBundle
import dev.helight.helix.protocol.MixinContribution
import dev.helight.helix.protocol.MixinExpressionRequest
import dev.helight.helix.protocol.helixExpressionModel

class HelixMixinCodeVisionProvider : CodeVisionProvider<String?> {
    override val id: String = "helix.mixin.hooks"
    override val name: String = HelixMessagesBundle.message("mixin.code.vision.name")
    override val defaultAnchor: CodeVisionAnchorKind = CodeVisionAnchorKind.Top
    override val relativeOrderings: List<CodeVisionRelativeOrdering> = emptyList()

    override fun precomputeOnUiThread(editor: Editor): String? =
        editor.virtualFile?.takeIf { it.extension.equals("cs", ignoreCase = true) }?.path

    override fun computeCodeVision(
        editor: Editor,
        uiData: String?,
    ): CodeVisionState {
        val filePath = uiData
        if (filePath == null || editor.isDisposed) return CodeVisionState.Ready(emptyList())
        val contributions: List<MixinContribution> = runCatching {
            editor.project?.solution?.helixExpressionModel?.getMixinContributions?.sync(
                MixinExpressionRequest(filePath),
                RpcTimeouts.longRunning,
            )?.contributions.orEmpty().toList()
        }.getOrElse { emptyList<MixinContribution>() }
        editor.project?.service<HelixMixinContributionCache>()?.update(filePath, contributions)

        val entries: List<Pair<TextRange, CodeVisionEntry>> = contributions.groupBy { it.offset to it.target }.map { (key, items) ->
            val offset = key.first.coerceIn(0, editor.document.textLength)
            val text = if (items.size == 1) "1 mixin hoko" else "${items.size} mixin hooks"
            TextRange(offset, offset) to ClickableTextCodeVisionEntry(
                text,
                id,
                { _, clickedEditor -> showContributions(clickedEditor, key.second, items) },
                null,
                "Show mixed hooks for ${key.second.removePrefix("global::").substringAfterLast(".")}",
                "",
                emptyList(),
            )
        }
        return CodeVisionState.Ready(entries)
    }

    private fun showContributions(
        editor: Editor,
        target: String,
        contributions: List<MixinContribution>,
    ) {
        HelixMixinContributionPopup.show(editor, target, contributions)
    }
}

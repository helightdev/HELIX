package dev.helight.helix.expression

import com.intellij.codeInsight.codeVision.CodeVisionAnchorKind
import com.intellij.codeInsight.codeVision.CodeVisionEntry
import com.intellij.codeInsight.codeVision.CodeVisionProvider
import com.intellij.codeInsight.codeVision.CodeVisionRelativeOrdering
import com.intellij.codeInsight.codeVision.CodeVisionState
import com.intellij.codeInsight.codeVision.ui.model.ClickableTextCodeVisionEntry
import com.intellij.openapi.editor.Editor
import com.intellij.openapi.ui.popup.JBPopupFactory
import com.intellij.openapi.util.TextRange
import com.jetbrains.rd.framework.impl.RpcTimeouts
import com.jetbrains.rider.projectView.solution
import dev.helight.helix.HelixMessagesBundle
import dev.helight.helix.protocol.MixinContribution
import dev.helight.helix.protocol.MixinExpressionRequest
import dev.helight.helix.protocol.helixExpressionModel

class HelixMixinCodeVisionProvider : CodeVisionProvider<String?> {
    override val id: String = "helix.mixin.contributions"
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

        val entries: List<Pair<TextRange, CodeVisionEntry>> = contributions.groupBy { it.offset to it.target }.map { (key, items) ->
            val offset = key.first.coerceIn(0, editor.document.textLength)
            val text = if (items.size == 1) "1 mixin contribution" else "${items.size} mixin contributions"
            TextRange(offset, offset) to ClickableTextCodeVisionEntry(
                text,
                id,
                { _, clickedEditor -> showContributions(clickedEditor, key.second, items) },
                null,
                "Show HELIX mixins applied to ${key.second.removePrefix("global::")}",
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
        val rows = contributions
            .sortedWith(compareBy<MixinContribution>({ it.method }, { it.priority }, { it.mixin }))
            .map { "${it.method}  ←  ${it.mixin.removePrefix("global::")}  (priority ${it.priority})" }
        JBPopupFactory.getInstance()
            .createPopupChooserBuilder(rows)
            .setTitle("HELIX mixins · ${target.removePrefix("global::")}")
            .setRequestFocus(true)
            .setResizable(true)
            .createPopup()
            .showInBestPositionFor(editor)
    }
}

package dev.helight.helix.expression

import com.intellij.openapi.editor.Editor
import com.intellij.openapi.fileEditor.FileEditorManager
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.popup.JBPopupFactory
import com.intellij.psi.PsiElement
import com.intellij.ui.ColoredListCellRenderer
import com.intellij.ui.SimpleTextAttributes
import com.intellij.util.Function
import dev.helight.helix.protocol.MixinContribution
import javax.swing.JList

object HelixMixinContributionPopup {
    private val ordering = compareBy<MixinContribution>({ it.method }, { it.priority }, { it.mixin })

    fun show(editor: Editor, target: String, contributions: List<MixinContribution>) {
        create(target, contributions).showInBestPositionFor(editor)
    }

    fun show(project: Project, element: PsiElement, target: String, contributions: List<MixinContribution>, showApplicator: Boolean = true) {
        create(target, contributions, showApplicator).showInBestPositionFor(FileEditorManager.getInstance(project).selectedTextEditor ?: return)
    }

    private fun create(target: String, contributions: List<MixinContribution>, showApplicator: Boolean = true) =
        JBPopupFactory.getInstance()
            .createPopupChooserBuilder(contributions.sortedWith(ordering))
            .setTitle("Mixins Hooks applied to ${target.removePrefix("global::").substringAfterLast(".")}")
            .setRenderer(ContributionRenderer(showApplicator))
            .setNamerForFiltering(Function { contribution ->
                "${contribution.method} ${contribution.mixin} ${contribution.priority}"
            })
            .setFilterAlwaysVisible(contributions.size > 5)
            .setRequestFocus(true)
            .setMovable(true)
            .setResizable(true)
            .createPopup()

    private class ContributionRenderer(val showApplicator: Boolean = true) : ColoredListCellRenderer<MixinContribution>() {
        override fun customizeCellRenderer(
            list: JList<out MixinContribution>,
            value: MixinContribution,
            index: Int,
            selected: Boolean,
            hasFocus: Boolean,
        ) {
            icon = dev.helight.helix.HelixIcons.MixinContribution
            append(value.method, SimpleTextAttributes.REGULAR_BOLD_ATTRIBUTES)
            append(" [${value.priority}]", SimpleTextAttributes.GRAYED_ATTRIBUTES)

            val applicator = value.sourceMember
            if (showApplicator && applicator.isNotBlank()) {
                append(" from ", SimpleTextAttributes.GRAYED_ATTRIBUTES)
                append(value.sourceMember, SimpleTextAttributes.REGULAR_ATTRIBUTES)
            }
            append(" by ", SimpleTextAttributes.GRAYED_ATTRIBUTES)
            append(
                value.mixin.removePrefix("global::").substringAfterLast(".").removeSuffix("Attribute"),
                SimpleTextAttributes.REGULAR_ATTRIBUTES
            )
        }
    }
}

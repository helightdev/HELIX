package dev.helight.helix.workspace

import com.intellij.openapi.options.Configurable
import com.intellij.openapi.project.Project
import com.intellij.openapi.ui.popup.JBPopupFactory
import com.intellij.ui.CollectionListModel
import com.intellij.ui.ColoredListCellRenderer
import com.intellij.ui.SimpleTextAttributes
import com.intellij.ui.ToolbarDecorator
import com.intellij.ui.components.JBList
import dev.helight.helix.HelixMessagesBundle.message
import javax.swing.JComponent
import javax.swing.JList

internal class WorkspaceSettingsConfigurable(private val project: Project) : Configurable {
    private val settings = WorkspaceSettings.getInstance(project)
    private var model = CollectionListModel<WorkspaceEntry>()
    private lateinit var list: JBList<WorkspaceEntry>

    override fun getDisplayName(): String = message("workspace.settings.name")

    override fun createComponent(): JComponent {
        model = CollectionListModel(settings.entries)
        list = JBList(model).apply {
            cellRenderer = object : ColoredListCellRenderer<WorkspaceEntry>() {
                override fun customizeCellRenderer(
                    list: JList<out WorkspaceEntry>, value: WorkspaceEntry, index: Int,
                    selected: Boolean, hasFocus: Boolean,
                ) {
                    val provider = WorkspaceEntryProvider.find(value.typeId)
                    icon = provider?.icon
                    append(value.name.ifBlank { value.options["path"] ?: provider?.displayName ?: value.typeId })
                    append("  ${provider?.displayName ?: value.typeId}", SimpleTextAttributes.GRAYED_ATTRIBUTES)
                }
            }
        }
        return ToolbarDecorator.createDecorator(list)
            .setAddAction { showProviderChooser() }
            .setEditAction { editSelected() }
            .setRemoveAction { model.remove(list.selectedIndex) }
            .setMoveUpAction { move(-1) }
            .setMoveDownAction { move(1) }
            .createPanel()
    }

    override fun isModified(): Boolean = model.items != settings.entries

    override fun apply() {
        settings.entries = model.items
        UnityWorkspaceProjectViewPane.find(project)?.refreshWorkspace()
    }

    override fun reset() = model.replaceAll(settings.entries)

    private fun showProviderChooser() {
        val configuredTypes = model.items.mapTo(mutableSetOf()) { it.typeId }
        val providers = WorkspaceEntryProvider.EP_NAME.extensionList.filter {
            it.allowMultiple || it.typeId !in configuredTypes
        }
        if (providers.isEmpty()) return
        if (providers.size == 1) return add(providers.single())
        JBPopupFactory.getInstance().createPopupChooserBuilder(providers)
            .setTitle(message("workspace.settings.choose.entry.type"))
            .setRenderer(object : ColoredListCellRenderer<WorkspaceEntryProvider>() {
                override fun customizeCellRenderer(
                    list: JList<out WorkspaceEntryProvider>, value: WorkspaceEntryProvider, index: Int,
                    selected: Boolean, hasFocus: Boolean,
                ) {
                    icon = value.icon
                    append(value.displayName)
                }
            })
            .setItemChosenCallback(::add)
            .createPopup().showUnderneathOf(list)
    }

    private fun add(provider: WorkspaceEntryProvider) {
        provider.configure(project)?.let {
            model.add(it.copy(typeId = provider.typeId))
            list.selectedIndex = model.size - 1
        }
    }

    private fun editSelected() {
        val index = list.selectedIndex.takeIf { it in model.items.indices } ?: return
        val entry = model.getElementAt(index)
        WorkspaceEntryProvider.find(entry.typeId)?.configure(project, entry)?.let {
            model.setElementAt(it.copy(typeId = entry.typeId), index)
        }
    }

    private fun move(offset: Int) {
        val from = list.selectedIndex
        val to = from + offset
        if (from !in model.items.indices || to !in model.items.indices) return
        val item = model.getElementAt(from)
        model.remove(from)
        model.add(to, item)
        list.selectedIndex = to
    }
}

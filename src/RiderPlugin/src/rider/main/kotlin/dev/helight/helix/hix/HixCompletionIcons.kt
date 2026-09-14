package dev.helight.helix.hix

import com.intellij.icons.AllIcons
import javax.swing.Icon

internal object HixCompletionIcons {
    fun forKind(kind: String, trait: Boolean = false): Icon = when (kind) {
        "Function" -> AllIcons.Nodes.Function
        "Constructor" -> if (trait) AllIcons.Nodes.Interface else AllIcons.Nodes.Constructor
        "Pattern" -> if (trait) AllIcons.Nodes.Interface else AllIcons.Nodes.Class
        "PrimitivePattern", "Type", "CSharpType" -> AllIcons.Nodes.Type
        "Parameter" -> AllIcons.Nodes.Parameter
        "Variable" -> AllIcons.Nodes.Variable
        "Root" -> AllIcons.Nodes.Gvariable
        "Field" -> AllIcons.Nodes.Field
        "Enum" -> AllIcons.Nodes.Enum
        "Reference" -> AllIcons.Nodes.Alias
        "Dependency" -> AllIcons.Nodes.Module
        "Label" -> AllIcons.Nodes.Bookmark
        "Keyword" -> AllIcons.Nodes.Word
        else -> AllIcons.Nodes.Annotationtype
    }
}

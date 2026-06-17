# IWidgetElement (/reference/HELIX.Widgets.IWidgetElement)

# IWidgetElement

```
public interface IWidgetElement : BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider
```

A <a data-furef-uid="UnityEngine.UIElements.VisualElement">VisualElement</a>s that is associated with a <a data-furef-uid="HELIX.Widgets.Widget">Widget</a>.
Most <a data-furef-uid="HELIX.Widgets.BuildContext">BuildContext</a> implementations implement this interface.

## HierarchyDepth

```
int HierarchyDepth { get; }
```

## CanReconcile(Widget)

```
bool CanReconcile(Widget updated)
```

## Reconcile(Widget)

```
bool Reconcile(Widget updated)
```
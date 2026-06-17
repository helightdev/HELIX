# HScaffold (/reference/HELIX.Widgets.Navigation.HScaffold)

# HScaffold

```
public class HScaffold : SingleChildWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IEnumerable<Widget>, IEnumerable
```

A widget that provides the ability to display overlays on top of its content.

## HScaffold(Widget, Key, object[], IReadOnlyCollection<Modifier>)

```
public HScaffold(Widget child = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a widget that provides the ability to display overlays on top of its content.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.
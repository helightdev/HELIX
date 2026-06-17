# HCenter (/reference/HELIX.Widgets.Universal.HCenter)

# HCenter

```
public class HCenter : SingleChildWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IEnumerable<Widget>, IEnumerable
```

A widget that tries to center its child using flex layouting.

## HCenter(Widget, Key, object[], IReadOnlyCollection<Modifier>)

```
public HCenter(Widget child = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a widget that tries to center its child using flex layouting.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.
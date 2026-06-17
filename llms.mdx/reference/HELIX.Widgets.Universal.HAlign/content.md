# HAlign (/reference/HELIX.Widgets.Universal.HAlign)

# HAlign

```
public class HAlign : SingleChildWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IEnumerable<Widget>, IEnumerable
```

A widget that aligns its child to the specified alignment.

## alignment

```
public readonly Alignment alignment
```

## HAlign(Alignment, Widget, Key, object[], IReadOnlyCollection<Modifier>)

```
public HAlign(Alignment alignment, Widget child = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a widget that aligns its child to the specified alignment.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.
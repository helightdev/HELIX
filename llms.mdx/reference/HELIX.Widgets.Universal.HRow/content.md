# HRow (/reference/HELIX.Widgets.Universal.HRow)

# HRow

```
public class HRow : DirectionalContainerWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IReadOnlyList<Widget>, IReadOnlyCollection<Widget>, IEnumerable<Widget>, IEnumerable
```

Represents a horizontally oriented flexible container widget.

## HRow(Justify, Align, float, bool, IReadOnlyList<Widget>, Key, object[], IReadOnlyCollection<Modifier>)

```
public HRow(Justify mainAxisAlign = Justify.FlexStart, Align crossAxisAlign = Align.Center, float gap = 0, bool reverse = false, IReadOnlyList<Widget> children = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Create a horizontally oriented flexible container widget.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.
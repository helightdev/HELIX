# HColumn (/reference/HELIX.Widgets.Universal.HColumn)

# HColumn

```
public class HColumn : DirectionalContainerWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IReadOnlyList<Widget>, IReadOnlyCollection<Widget>, IEnumerable<Widget>, IEnumerable
```

Represents a vertically oriented flexible container widget.

## HColumn(Justify, Align, float, bool, IReadOnlyList<Widget>, Key, object[], IReadOnlyCollection<Modifier>)

```
public HColumn(Justify mainAxisAlign = Justify.FlexStart, Align crossAxisAlign = Align.Center, float gap = 0, bool reverse = false, IReadOnlyList<Widget> children = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Create a vertically oriented flexible container widget.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.
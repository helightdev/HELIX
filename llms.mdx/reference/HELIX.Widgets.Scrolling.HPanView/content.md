# HPanView (/reference/HELIX.Widgets.Scrolling.HPanView)

# HPanView

```
public class HPanView : MultiChildWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IReadOnlyList<Widget>, IReadOnlyCollection<Widget>, IEnumerable<Widget>, IEnumerable
```

A widget that can be used to scroll through a list of widgets horizontally and vertically.

## horizontalController

```
public readonly ScrollController horizontalController
```

## verticalController

```
public readonly ScrollController verticalController
```

## HPanView(ScrollController, ScrollController, IReadOnlyList<Widget>, Key, object[], IReadOnlyCollection<Modifier>)

```
public HPanView(ScrollController horizontalController = null, ScrollController verticalController = null, IReadOnlyList<Widget> children = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a scrollable view that can be used to scroll through a list of widgets horizontally and vertically.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.
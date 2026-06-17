# HScrollView (/reference/HELIX.Widgets.Scrolling.HScrollView)

# HScrollView

```
public class HScrollView : MultiChildWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IReadOnlyList<Widget>, IReadOnlyCollection<Widget>, IEnumerable<Widget>, IEnumerable
```

A widget that can be used to scroll through a list of widgets.

## axis

```
public readonly Axis axis
```

## controller

```
public readonly ScrollController controller
```

## HScrollView(Axis, ScrollController, IReadOnlyList<Widget>, Key, object[], IReadOnlyCollection<Modifier>)

```
public HScrollView(Axis axis = Axis.Vertical, ScrollController controller = null, IReadOnlyList<Widget> children = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a scrollable view that can be used to scroll through a list of widgets.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.
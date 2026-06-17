# SingleChildWidget (/reference/HELIX.Widgets.SingleChildWidget)

# SingleChildWidget

```
public abstract class SingleChildWidget : Widget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IEnumerable<Widget>, IEnumerable
```

Base class for a <a data-furef-uid="HELIX.Widgets.Widget">Widget</a> that contains at most one child widget.

## child

```
public Widget child
```

## SingleChildWidget(Widget, Key, object[], IReadOnlyCollection<Modifier>)

```
protected SingleChildWidget(Widget child = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Base constructor for a widget with a single child.

## GetEnumerator()

```
public IEnumerator<Widget> GetEnumerator()
```

## DebugDescribeChildren()

```
public override List<DiagnosticsNode> DebugDescribeChildren()
```

## Add(Widget)

```
public void Add(Widget candidate)
```
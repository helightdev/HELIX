# MultiChildWidget (/reference/HELIX.Widgets.MultiChildWidget)

# MultiChildWidget

```
public abstract class MultiChildWidget : Widget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IReadOnlyList<Widget>, IReadOnlyCollection<Widget>, IEnumerable<Widget>, IEnumerable
```

Base class for a <a data-furef-uid="HELIX.Widgets.Widget">Widget</a> that can contain multiple child widgets.

## children

```
public IReadOnlyList<Widget> children
```

## MultiChildWidget(IReadOnlyList<Widget>, Key, object[], IReadOnlyCollection<Modifier>)

```
protected MultiChildWidget(IReadOnlyList<Widget> children = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

## GetEnumerator()

```
public IEnumerator<Widget> GetEnumerator()
```

## Count

```
public int Count { get; }
```

## this[int]

```
public Widget this[int index] { get; }
```

## DebugDescribeChildren()

```
public override List<DiagnosticsNode> DebugDescribeChildren()
```

## Add(IWidgetListCandidate)

```
public void Add(IWidgetListCandidate candidate)
```
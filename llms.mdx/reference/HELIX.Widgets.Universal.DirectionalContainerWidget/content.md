# DirectionalContainerWidget (/reference/HELIX.Widgets.Universal.DirectionalContainerWidget)

# DirectionalContainerWidget

```
public abstract class DirectionalContainerWidget : MultiChildWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IReadOnlyList<Widget>, IReadOnlyCollection<Widget>, IEnumerable<Widget>, IEnumerable
```

## crossAxisAlign

```
public readonly Align crossAxisAlign
```

## gap

```
public readonly float gap
```

## mainAxisAlign

```
public readonly Justify mainAxisAlign
```

## reverse

```
public readonly bool reverse
```

## DirectionalContainerWidget(Justify, Align, float, bool, IReadOnlyList<Widget>, Key, object[], IReadOnlyCollection<Modifier>)

```
protected DirectionalContainerWidget(Justify mainAxisAlign = Justify.FlexStart, Align crossAxisAlign = Align.Center, float gap = 0, bool reverse = false, IReadOnlyList<Widget> children = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```
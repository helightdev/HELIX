# HStack (/reference/HELIX.Widgets.Universal.HStack)

# HStack

```
public class HStack : MultiChildWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IReadOnlyList<Widget>, IReadOnlyCollection<Widget>, IEnumerable<Widget>, IEnumerable
```

Represents a container that tries to arrange its children above each other using absolute positioning.

## axis

```
public readonly Axis axis
```

## crossAxisAlign

```
public readonly Align crossAxisAlign
```

## mainAxisAlign

```
public readonly Justify mainAxisAlign
```

## reverse

```
public readonly bool reverse
```

## wrap

```
public readonly bool wrap
```

## wrapAlign

```
public readonly Align? wrapAlign
```

## wrapReverse

```
public readonly bool wrapReverse
```

## HStack(Axis, Align, Justify, Align?, bool, bool, bool, Key, object[], IReadOnlyList<Widget>, IReadOnlyCollection<Modifier>)

```
public HStack(Axis axis = Axis.Vertical, Align crossAxisAlign = Align.FlexStart, Justify mainAxisAlign = Justify.FlexStart, Align? wrapAlign = null, bool reverse = false, bool wrapReverse = false, bool wrap = false, Key key = default, object[] constants = null, IReadOnlyList<Widget> children = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a container that tries to arrange its children above each other using absolute positioning.
If the children do not use absolute positioning, behavior will be similar to <a data-furef-uid="HELIX.Widgets.Universal.HFlex">HFlex</a>.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```
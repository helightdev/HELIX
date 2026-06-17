# HFlex (/reference/HELIX.Widgets.Universal.HFlex)

# HFlex

```
public class HFlex : MultiChildWidget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IReadOnlyList<Widget>, IReadOnlyCollection<Widget>, IEnumerable<Widget>, IEnumerable
```

Represents a flexible layout widget that arranges its child widgets
along a specified axis with various alignment and wrapping properties.

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

## HFlex(Axis, Align, Justify, Align?, bool, bool, bool, IReadOnlyList<Widget>, Key, object[], IReadOnlyCollection<Modifier>)

```
public HFlex(Axis axis = Axis.Vertical, Align crossAxisAlign = Align.FlexStart, Justify mainAxisAlign = Justify.FlexStart, Align? wrapAlign = null, bool reverse = false, bool wrapReverse = false, bool wrap = false, IReadOnlyList<Widget> children = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Represents a flexible layout widget that arranges its child widgets
along a specified axis, with additional alignment, wrapping, and
layout options.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```
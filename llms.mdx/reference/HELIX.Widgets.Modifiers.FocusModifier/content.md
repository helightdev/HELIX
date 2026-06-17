# FocusModifier (/reference/HELIX.Widgets.Modifiers.FocusModifier)

# FocusModifier

```
public class FocusModifier : Modifier, IDiagnosticable
```

## Focusable

```
public static readonly FocusModifier Focusable
```

## FocusableDelegates

```
public static readonly FocusModifier FocusableDelegates
```

## FocusableNoTab

```
public static readonly FocusModifier FocusableNoTab
```

## Ignore

```
public static readonly FocusModifier Ignore
```

## None

```
public static readonly FocusModifier None
```

## delegatesFocus

```
public readonly bool delegatesFocus
```

## focusable

```
public readonly bool focusable
```

## pickingMode

```
public readonly PickingMode pickingMode
```

## tabIndex

```
public readonly int tabIndex
```

## FocusModifier(bool, PickingMode, int, bool)

```
public FocusModifier(bool focusable, PickingMode pickingMode, int tabIndex, bool delegatesFocus)
```

## Apply(VisualElement)

```
public override void Apply(VisualElement element)
```

## Reset(VisualElement)

```
public override void Reset(VisualElement element)
```

## HasChanged(Modifier)

```
public override bool HasChanged(Modifier previous)
```

## FillModifierProperties(DiagnosticPropertiesBuilder)

```
public override void FillModifierProperties(DiagnosticPropertiesBuilder properties)
```

## FindConstantName()

```
protected override string FindConstantName()
```

## Of(int, bool, PickingMode, bool)

```
public static FocusModifier Of(int tabIndex = 0, bool focusable = true, PickingMode mode = PickingMode.Position, bool delegatesFocus = false)
```
# SizeModifier (/reference/HELIX.Widgets.Modifiers.SizeModifier)

# SizeModifier

```
public class SizeModifier : Modifier, IDiagnosticable
```

## None

```
public static readonly SizeModifier None
```

## constraints

```
public readonly BoxConstraints constraints
```

## SizeModifier(StyleLength2, StyleLength2, StyleLength2)

```
public SizeModifier(StyleLength2 size, StyleLength2 minSize, StyleLength2 maxSize)
```

## SizeModifier(BoxConstraints)

```
public SizeModifier(BoxConstraints constraints)
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

## Of(StyleLength, StyleLength)

```
public static SizeModifier Of(StyleLength width, StyleLength height)
```

## Of(BoxConstraints)

```
public static SizeModifier Of(BoxConstraints constraints)
```

## Tight(StyleLength2)

```
public static SizeModifier Tight(StyleLength2 size)
```

## Tight(StyleLength, StyleLength)

```
public static SizeModifier Tight(StyleLength width, StyleLength height)
```

## Min(StyleLength2)

```
public static SizeModifier Min(StyleLength2 min)
```

## FillModifierProperties(DiagnosticPropertiesBuilder)

```
public override void FillModifierProperties(DiagnosticPropertiesBuilder properties)
```
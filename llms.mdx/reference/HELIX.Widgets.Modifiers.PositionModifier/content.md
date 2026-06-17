# PositionModifier (/reference/HELIX.Widgets.Modifiers.PositionModifier)

# PositionModifier

```
public class PositionModifier : Modifier, IDiagnosticable
```

## Stretch

```
public static readonly PositionModifier Stretch
```

## None

```
public static readonly PositionModifier None
```

## pos

```
public readonly StyleLength4 pos
```

## type

```
public readonly Position type
```

## isStackingOnly

```
public bool isStackingOnly
```

## PositionModifier(StyleLength4, Position)

```
public PositionModifier(StyleLength4 pos, Position type)
```

## PositionModifier()

```
public PositionModifier()
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

## Absolute(StyleLength4)

```
public static PositionModifier Absolute(StyleLength4 offset)
```

## Relative(StyleLength4)

```
public static PositionModifier Relative(StyleLength4 offset)
```

## FillModifierProperties(DiagnosticPropertiesBuilder)

```
public override void FillModifierProperties(DiagnosticPropertiesBuilder properties)
```

## FindConstantName()

```
protected override string FindConstantName()
```
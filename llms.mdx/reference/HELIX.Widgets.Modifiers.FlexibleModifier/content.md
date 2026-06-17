# FlexibleModifier (/reference/HELIX.Widgets.Modifiers.FlexibleModifier)

# FlexibleModifier

```
public class FlexibleModifier : Modifier, IDiagnosticable
```

## Expand

```
public static readonly FlexibleModifier Expand
```

## Shrink

```
public static readonly FlexibleModifier Shrink
```

## Tight

```
public static readonly FlexibleModifier Tight
```

## Fill

```
public static readonly FlexibleModifier Fill
```

## TightStretch

```
public static readonly FlexibleModifier TightStretch
```

## grow

```
public readonly StyleFloat grow
```

## selfCrossAxisAlign

```
public readonly StyleEnum<Align> selfCrossAxisAlign
```

## shrink

```
public readonly StyleFloat shrink
```

## isImplicit

```
public bool isImplicit
```

## FlexibleModifier(StyleFloat, StyleFloat, StyleEnum<Align>)

```
public FlexibleModifier(StyleFloat grow, StyleFloat shrink, StyleEnum<Align> selfCrossAxisAlign)
```

## FlexibleModifier()

```
public FlexibleModifier()
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

## Of(StyleFloat, StyleFloat, StyleEnum<Align>?)

```
public static FlexibleModifier Of(StyleFloat grow, StyleFloat shrink, StyleEnum<Align>? selfCrossAxisAlign = null)
```

## FillModifierProperties(DiagnosticPropertiesBuilder)

```
public override void FillModifierProperties(DiagnosticPropertiesBuilder properties)
```

## FindConstantName()

```
protected override string FindConstantName()
```
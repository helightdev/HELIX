# OpacityModifier (/reference/HELIX.Widgets.Modifiers.OpacityModifier)

# OpacityModifier

```
public class OpacityModifier : Modifier, IDiagnosticable
```

## Opaque

```
public static readonly OpacityModifier Opaque
```

## Transparent

```
public static readonly OpacityModifier Transparent
```

## opacity

```
public readonly float opacity
```

## OpacityModifier(float)

```
public OpacityModifier(float opacity)
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

## Of(float)

```
public static OpacityModifier Of(float opacity)
```

## FillModifierProperties(DiagnosticPropertiesBuilder)

```
public override void FillModifierProperties(DiagnosticPropertiesBuilder properties)
```

## FindConstantName()

```
protected override string FindConstantName()
```
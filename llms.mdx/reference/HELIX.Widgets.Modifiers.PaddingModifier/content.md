# PaddingModifier (/reference/HELIX.Widgets.Modifiers.PaddingModifier)

# PaddingModifier

```
public class PaddingModifier : Modifier, IDiagnosticable
```

## Initial

```
public static readonly PaddingModifier Initial
```

## Zero

```
public static readonly PaddingModifier Zero
```

## padding

```
public readonly StyleLength4 padding
```

## PaddingModifier(StyleLength4)

```
public PaddingModifier(StyleLength4 padding)
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

## Of(StyleLength4)

```
public static PaddingModifier Of(StyleLength4 margin)
```

## Only(StyleLength?, StyleLength?, StyleLength?, StyleLength?)

```
public static PaddingModifier Only(StyleLength? left = null, StyleLength? top = null, StyleLength? right = null, StyleLength? bottom = null)
```
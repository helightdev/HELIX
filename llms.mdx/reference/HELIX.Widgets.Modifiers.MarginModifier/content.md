# MarginModifier (/reference/HELIX.Widgets.Modifiers.MarginModifier)

# MarginModifier

```
public class MarginModifier : Modifier, IDiagnosticable
```

## Initial

```
public static readonly MarginModifier Initial
```

## Zero

```
public static readonly MarginModifier Zero
```

## margin

```
public readonly StyleLength4 margin
```

## MarginModifier(StyleLength4)

```
public MarginModifier(StyleLength4 margin)
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
public static MarginModifier Of(StyleLength4 margin)
```

## Only(StyleLength?, StyleLength?, StyleLength?, StyleLength?)

```
public static MarginModifier Only(StyleLength? left = null, StyleLength? top = null, StyleLength? right = null, StyleLength? bottom = null)
```
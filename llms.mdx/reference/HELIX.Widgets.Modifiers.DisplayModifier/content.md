# DisplayModifier (/reference/HELIX.Widgets.Modifiers.DisplayModifier)

# DisplayModifier

```
public class DisplayModifier : Modifier, IDiagnosticable
```

## Visible

```
public static readonly DisplayModifier Visible
```

## Hidden

```
public static readonly DisplayModifier Hidden
```

## visible

```
public readonly bool visible
```

## DisplayModifier(bool)

```
public DisplayModifier(bool visible)
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
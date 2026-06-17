# VisibilityModifier (/reference/HELIX.Widgets.Modifiers.VisibilityModifier)

# VisibilityModifier

```
public class VisibilityModifier : Modifier, IDiagnosticable
```

## Visible

```
public static readonly VisibilityModifier Visible
```

## Hidden

```
public static readonly VisibilityModifier Hidden
```

## visible

```
public readonly bool visible
```

## VisibilityModifier(bool)

```
public VisibilityModifier(bool visible)
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
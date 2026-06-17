# BorderModifier (/reference/HELIX.Widgets.Modifiers.BorderModifier)

# BorderModifier

```
public class BorderModifier : Modifier, IDiagnosticable
```

## None

```
public static readonly BorderModifier None
```

## border

```
public readonly Border border
```

## radius

```
public readonly BorderRadius radius
```

## BorderModifier(Border, BorderRadius)

```
public BorderModifier(Border border, BorderRadius radius)
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

## FindConstantName()

```
protected override string FindConstantName()
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```

## Of(Border?, BorderRadius?)

```
public static BorderModifier Of(Border? border = null, BorderRadius? radius = null)
```
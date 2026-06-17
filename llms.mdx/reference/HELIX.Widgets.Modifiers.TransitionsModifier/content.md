# TransitionsModifier (/reference/HELIX.Widgets.Modifiers.TransitionsModifier)

# TransitionsModifier

```
public class TransitionsModifier : Modifier, IDiagnosticable
```

## None

```
public static readonly TransitionsModifier None
```

## transitions

```
public readonly Transition[] transitions
```

## TransitionsModifier(Transition[])

```
public TransitionsModifier(Transition[] transitions)
```

## TransitionsModifier()

```
public TransitionsModifier()
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

## Of(params Transition[])

```
public static TransitionsModifier Of(params Transition[] transitions)
```
# FilterModifier (/reference/HELIX.Widgets.Modifiers.FilterModifier)

# FilterModifier

```
public class FilterModifier : Modifier, IDiagnosticable
```

## filters

```
public readonly List<FilterFunction> filters
```

## FilterModifier(List<FilterFunction>)

```
public FilterModifier(List<FilterFunction> filters)
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

## Of(params FilterFunction[])

```
public static FilterModifier Of(params FilterFunction[] filters)
```
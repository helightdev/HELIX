# BoxSubstance (/reference/HELIX.Widgets.Universal.Substances.BoxSubstance)

# BoxSubstance

```
public class BoxSubstance : Substance, IDiagnosticable
```

An implementation of <a data-furef-uid="HELIX.Widgets.Universal.Substance">Substance</a> that builds a generic <a data-furef-uid="HELIX.Widgets.Universal.HBox">HBox</a> widget.

## background

```
public WidgetStateProperty<BackgroundStyle> background
```

## border

```
public WidgetStateProperty<Border> border
```

## borderRadius

```
public WidgetStateProperty<BorderRadius> borderRadius
```

## constraints

```
public WidgetStateProperty<BoxConstraints> constraints
```

## modifiers

```
public WidgetStateProperty<ModifierSet> modifiers
```

## opacity

```
public WidgetStateProperty<float> opacity
```

## position

```
public WidgetStateProperty<StyleLength4> position
```

## transitions

```
public WidgetStateProperty<Transition[]> transitions
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```

## Build(BuildContext, WidgetState)

```
public override IWidgetListCandidate Build(BuildContext context, WidgetState state)
```
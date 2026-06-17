# SliderControllerModifier (/reference/HELIX.Widgets.Modifiers.SliderControllerModifier)

# SliderControllerModifier

```
public class SliderControllerModifier : SingletonModifier, IDiagnosticable
```

## axis

```
public readonly Axis axis
```

## controller

```
public readonly SliderController controller
```

## thumbSize

```
public readonly float thumbSize
```

## widgetState

```
public readonly WidgetStateController widgetState
```

## reverse

```
public bool reverse
```

## SliderControllerModifier(SliderController, Axis, float, bool, WidgetStateController)

```
public SliderControllerModifier(SliderController controller, Axis axis, float thumbSize = -1, bool reverse = false, WidgetStateController widgetState = null)
```

## Hook(VisualElement)

```
public override void Hook(VisualElement element)
```

## Unhook(VisualElement)

```
public override void Unhook(VisualElement element)
```

## HasChanged(Modifier)

```
public override bool HasChanged(Modifier previous)
```
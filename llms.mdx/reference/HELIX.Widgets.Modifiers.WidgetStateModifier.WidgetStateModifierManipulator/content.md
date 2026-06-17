# WidgetStateModifier.WidgetStateModifierManipulator (/reference/HELIX.Widgets.Modifiers.WidgetStateModifier.WidgetStateModifierManipulator)

# WidgetStateModifier.WidgetStateModifierManipulator

```
public class WidgetStateModifier.WidgetStateModifierManipulator : Manipulator, IManipulator
```

## controller

```
public readonly WidgetStateController controller
```

## handleFocus

```
public readonly bool handleFocus
```

## WidgetStateModifierManipulator(WidgetStateController, bool)

```
public WidgetStateModifierManipulator(WidgetStateController controller, bool handleFocus)
```

## RegisterCallbacksOnTarget()

```
protected override void RegisterCallbacksOnTarget()
```

<p>
Called to register event callbacks on the target element.
</p>

## UnregisterCallbacksFromTarget()

```
protected override void UnregisterCallbacksFromTarget()
```

<p>
Called to unregister event callbacks from the target element.
</p>
# ButtonController.ButtonManipulator (/reference/HELIX.Widgets.Universal.Controllers.ButtonController.ButtonManipulator)

# ButtonController.ButtonManipulator

```
public class ButtonController.ButtonManipulator : Clickable, IManipulator
```

## controller

```
public ButtonController controller
```

## ButtonManipulator(ButtonController)

```
public ButtonManipulator(ButtonController controller)
```

## RegisterCallbacksOnTarget()

```
protected override void RegisterCallbacksOnTarget()
```

<p>
Called to register mouse event callbacks on the target element.
</p>

## UnregisterCallbacksFromTarget()

```
protected override void UnregisterCallbacksFromTarget()
```

<p>
Called to unregister event callbacks from the target element.
</p>

## ProcessUpEvent(EventBase, Vector2, int)

```
protected override void ProcessUpEvent(EventBase evt, Vector2 localPosition, int pointerId)
```

<p>
This method processes the up event sent to the target Element.
</p>

## ProcessCancelEvent(EventBase, int)

```
protected override void ProcessCancelEvent(EventBase evt, int pointerId)
```

<p>
This method processes the up cancel sent to the target Element.
</p>

## ProcessDownEvent(EventBase, Vector2, int)

```
protected override void ProcessDownEvent(EventBase evt, Vector2 localPosition, int pointerId)
```

<p>
This method processes the down event sent to the target Element.
</p>
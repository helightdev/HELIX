# GenericButton.Manipulator (/reference/HELIX.Widgets.Elements.GenericButton.Manipulator)

# GenericButton.Manipulator

```
public class GenericButton.Manipulator : Clickable, IManipulator
```

## button

```
public GenericButton button
```

## Manipulator(GenericButton)

```
public Manipulator(GenericButton button)
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
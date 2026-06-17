# AnimationController (/reference/HELIX.Animation.AnimationController)

# AnimationController

```
public class AnimationController
```

Manages the animation sequence by controlling its playback direction, duration, and looping behavior.

## AnimationController(IVisualElementScheduler)

```
public AnimationController(IVisualElementScheduler scheduler)
```

## AnimationController(IVisualElementScheduler, Action<float>)

```
public AnimationController(IVisualElementScheduler scheduler, Action<float> onUpdate)
```

## AnimationController(VisualElement)

```
public AnimationController(VisualElement element)
```

## AnimationController(VisualElement, Action<float>)

```
public AnimationController(VisualElement element, Action<float> onUpdate)
```

## Duration

```
public long Duration { get; set; }
```

## Value

```
public float Value { get; set; }
```

## LoopType

```
public LoopType LoopType { get; set; }
```

## OnUpdate

```
public event Action<float> OnUpdate
```

Event triggered during each update of the animation's progress.
This event allows subscribers to receive the current normalized value of the animation,
ranging between 0.0 and 1.0, as it progresses.

## OnComplete

```
public event Action OnComplete
```

Event triggered when the animation sequence completes its progress in the current direction.
This can occur at the end of a single animation, after a loop iteration,
or as part of a ping-pong transition, depending on the set <a data-furef-uid="HELIX.Animation.AnimationController.LoopType">AnimationController.LoopType</a>.

## Forward(bool)

```
public void Forward(bool restart = false)
```

Plays the animation in the forward direction, starting from the beginning if specified.

## Backward(bool)

```
public void Backward(bool restart = false)
```

Plays the animation in the backward direction, optionally starting from the end.

## Stop()

```
public void Stop()
```

Stops the animation playback and pauses any scheduled updates.
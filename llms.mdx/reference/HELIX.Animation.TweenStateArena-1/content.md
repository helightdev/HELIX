# TweenStateArena<T> (/reference/HELIX.Animation.TweenStateArena-1)

# TweenStateArena<T>

```
public class TweenStateArena<T> : TweenStateArena
```

## TweenStateArena(IVisualElementScheduler, long, LerpFunc, Action<T>, EasingMode)

```
public TweenStateArena(IVisualElementScheduler scheduler, long durationMs, TweenStateArena<T>.LerpFunc lerp, Action<T> update, EasingMode easingMode = EasingMode.Linear)
```

## DurationMs

```
public long DurationMs { get; set; }
```

## Duration

```
public TimeValue Duration { get; set; }
```

## EasingMode

```
public EasingMode EasingMode { get; set; }
```

## Value

```
public T Value { get; set; }
```

## Push(T)

```
public void Push(T newValue)
```

Initiates a tweening operation transitioning the current value to the specified new value
over the configured duration using the provided interpolation function.
The transition updates the value at regular intervals and invokes the update action
accordingly.

## Set(T)

```
public void Set(T newValue)
```

Sets the current value of the tween without initiating a transition or animation.
Immediately applies the specified value and invokes the update action to reflect the change.
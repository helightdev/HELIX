# ScrollController (/reference/HELIX.Widgets.Scrolling.ScrollController)

# ScrollController

```
public class ScrollController : ValueSignal<float>, IDiagnosticable, IDisposable, IPossiblyDisposed, ISignalObserver
```

## ScrollController()

```
public ScrollController()
```

## InitialScrollOffset

```
public float InitialScrollOffset { get; set; }
```

## KeepScrollOffset

```
public bool KeepScrollOffset { get; set; }
```

## Offset

```
public float Offset { get; set; }
```

## NormalizedScrollOffset

```
public float NormalizedScrollOffset { get; set; }
```

## MinOffset

```
public float MinOffset { get; }
```

## MaxOffset

```
public float MaxOffset { get; }
```

## ScrollPosition

```
public ScrollPosition ScrollPosition { get; }
```

## OnSignalChanged(Signal)

```
public void OnSignalChanged(Signal signal)
```

## OnSignalRemoved(Signal)

```
public void OnSignalRemoved(Signal signal)
```

## JumpTo(float)

```
public void JumpTo(float offset)
```

## AnimateTo(float, TimeValue, EasingMode)

```
public void AnimateTo(float offset, TimeValue duration, EasingMode easing = EasingMode.Linear)
```

## ScrollTo(VisualElement)

```
public void ScrollTo(VisualElement element)
```

## Attach(ScrollPosition)

```
public void Attach(ScrollPosition position)
```

## Detach(ScrollPosition)

```
public void Detach(ScrollPosition position)
```
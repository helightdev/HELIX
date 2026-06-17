# ScrollPosition (/reference/HELIX.Widgets.Scrolling.ScrollPosition)

# ScrollPosition

```
public abstract class ScrollPosition : Signal, IDiagnosticable, IDisposable, IPossiblyDisposed
```

## Min

```
public abstract float Min { get; }
```

## Max

```
public abstract float Max { get; }
```

## Extent

```
public abstract float Extent { get; set; }
```

## ExtentInside

```
public abstract float ExtentInside { get; }
```

## ExtentTotal

```
public abstract float ExtentTotal { get; }
```

## NormalizedOffset

```
public virtual float NormalizedOffset { get; set; }
```

## Restore(float)

```
public virtual void Restore(float offset)
```

## AnimateTo(float, TimeValue, EasingMode)

```
public virtual void AnimateTo(float offset, TimeValue durationMs, EasingMode easing)
```

## ScrollTo(VisualElement)

```
public virtual void ScrollTo(VisualElement element)
```
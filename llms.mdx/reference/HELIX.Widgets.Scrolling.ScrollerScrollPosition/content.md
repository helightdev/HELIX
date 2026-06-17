# ScrollerScrollPosition (/reference/HELIX.Widgets.Scrolling.ScrollerScrollPosition)

# ScrollerScrollPosition

```
public class ScrollerScrollPosition : ScrollPosition, IDiagnosticable, IDisposable, IPossiblyDisposed
```

## scroller

```
public readonly Scroller scroller
```

## scrollView

```
public readonly ScrollView scrollView
```

## ScrollerScrollPosition(Scroller, ScrollView)

```
public ScrollerScrollPosition(Scroller scroller, ScrollView scrollView)
```

## Min

```
public override float Min { get; }
```

## Max

```
public override float Max { get; }
```

## ExtentInside

```
public override float ExtentInside { get; }
```

## ExtentTotal

```
public override float ExtentTotal { get; }
```

## Extent

```
public override float Extent { get; set; }
```

## Dispose()

```
public override void Dispose()
```

## Restore(float)

```
public override void Restore(float offset)
```

## AnimateTo(float, TimeValue, EasingMode)

```
public override void AnimateTo(float offset, TimeValue duration, EasingMode easing)
```

## ScrollTo(VisualElement)

```
public override void ScrollTo(VisualElement element)
```
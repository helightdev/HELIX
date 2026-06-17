# SliderController (/reference/HELIX.Widgets.Universal.Controllers.SliderController)

# SliderController

```
public class SliderController : ValueSignal<float>, IDiagnosticable, IDisposable, IPossiblyDisposed, ISignalObserver
```

## widgetState

```
public readonly WidgetStateController widgetState
```

## enabled

```
public bool enabled
```

## onChanged

```
public Action<float> onChanged
```

## SliderController(WidgetStateController, float)

```
public SliderController(WidgetStateController widgetState = null, float initialValue = 0)
```

## ThumbRange

```
public float ThumbRange { get; set; }
```

## Enabled

```
public bool Enabled { get; }
```

## LinkedScrollController

```
public ScrollController LinkedScrollController { get; }
```

## OnSignalChanged(Signal)

```
public void OnSignalChanged(Signal signal)
```

## OnSignalRemoved(Signal)

```
public void OnSignalRemoved(Signal signal)
```

## SetValue(float)

```
public override void SetValue(float newValue)
```

## SetWithoutNotify(float)

```
public override void SetWithoutNotify(float newValue)
```

## LinkScrollController(ScrollController, bool)

```
public void LinkScrollController(ScrollController scrollController, bool syncValueFromScroll = true)
```

## UnlinkScrollController()

```
public void UnlinkScrollController()
```

## RefreshFromLinkedScroll(bool)

```
public void RefreshFromLinkedScroll(bool syncValue = true)
```

## Dispose()

```
public override void Dispose()
```
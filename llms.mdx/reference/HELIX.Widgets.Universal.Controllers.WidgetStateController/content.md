# WidgetStateController (/reference/HELIX.Widgets.Universal.Controllers.WidgetStateController)

# WidgetStateController

```
public class WidgetStateController : ValueSignal<WidgetState>, IDiagnosticable, IDisposable, IPossiblyDisposed, ISignalObserver
```

## LastNavigated

```
public static bool LastNavigated
```

## inheritance

```
public readonly WidgetState inheritance
```

## mask

```
public readonly WidgetState mask
```

## WidgetStateController(WidgetState, WidgetState)

```
public WidgetStateController(WidgetState mask = WidgetState.MetaAll, WidgetState inheritanceMask = WidgetState.MetaAll)
```

## Children

```
public IEnumerable<WidgetStateController> Children { get; }
```

## OnSignalChanged(Signal)

```
public void OnSignalChanged(Signal signal)
```

## AddInherited(WidgetStateController)

```
public void AddInherited(WidgetStateController child)
```

## Enable(WidgetState)

```
public void Enable(WidgetState state)
```

## Disable(WidgetState)

```
public void Disable(WidgetState state)
```

## DisableEnable(WidgetState, WidgetState)

```
public void DisableEnable(WidgetState disable, WidgetState enable)
```

## Toggle(WidgetState)

```
public void Toggle(WidgetState state)
```

## Toggle(WidgetState, bool)

```
public void Toggle(WidgetState state, bool toggle)
```

## Clear()

```
public void Clear()
```

## SetValue(WidgetState)

```
public override void SetValue(WidgetState newValue)
```
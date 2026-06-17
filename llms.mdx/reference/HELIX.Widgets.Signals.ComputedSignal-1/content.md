# ComputedSignal<T> (/reference/HELIX.Widgets.Signals.ComputedSignal-1)

# ComputedSignal<T>

```
public class ComputedSignal<T> : Signal<T>, IDiagnosticable, IDisposable, IPossiblyDisposed, ISignalObserver
```

A signal that recomputes its value whenever any of its dependencies change.

## ComputedSignal(Func<T>)

```
public ComputedSignal(Func<T> computeFunc)
```

## OnSignalDirty(Signal)

```
public void OnSignalDirty(Signal signal)
```

## OnSignalChanged(Signal)

```
public void OnSignalChanged(Signal signal)
```

## PeekValue()

```
public override T PeekValue()
```

## SetValue(T)

```
public override void SetValue(T newValue)
```

## SetWithoutNotify(T)

```
public override void SetWithoutNotify(T newValue)
```

## Dispose()

```
public override void Dispose()
```

## ToStringShort()

```
public override string ToStringShort()
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```
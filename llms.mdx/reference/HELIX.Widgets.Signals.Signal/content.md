# Signal (/reference/HELIX.Widgets.Signals.Signal)

# Signal

```
public abstract class Signal : DiagnosticableBase, IDiagnosticable, IDisposable, IPossiblyDisposed
```

<p>A signal represents a piece of reactive state that can be observed for changes.</p>
<p>
Signals can be observed by implementing the <a data-furef-uid="HELIX.Widgets.Signals.ISignalObserver">ISignalObserver</a> interface and subscribing to the signal
using <a data-furef-uid="HELIX.Widgets.Signals.Signal.AddObserver(HELIX.Widgets.Signals.ISignalObserver)">Signal.AddObserver</a>. <a data-furef-uid="HELIX.Widgets.StatefulWidget%601">StatefulWidget</a>s may also access signals inside
their build methods, which will automatically subscribe to the signal and rebuild the widget when the signal changes.
</p>

## Dispose()

```
public virtual void Dispose()
```

## IsDisposed

```
public bool IsDisposed { get; }
```

## NotifyDirty()

```
protected void NotifyDirty()
```

## NotifyObservers()

```
protected void NotifyObservers()
```

## AddObserver(ISignalObserver)

```
public virtual bool AddObserver(ISignalObserver observer)
```

## RemoveObserver(ISignalObserver)

```
public virtual bool RemoveObserver(ISignalObserver observer)
```

## AddObserver(Action, bool)

```
public FunctionSignalObserver AddObserver(Action onChanged, bool fireImmediately = false)
```

## Value<T>(T, bool)

```
public static ValueSignal<T> Value<T>(T initialValue = default, bool equality = true)
```

Creates a signal that holds a single value.

## Computed<T>(Func<T>)

```
public static ComputedSignal<T> Computed<T>(Func<T> computeFunc)
```

Creates a signal that recomputes its value whenever any of its dependencies change.
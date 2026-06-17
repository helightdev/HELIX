# Signal<T> (/reference/HELIX.Widgets.Signals.Signal-1)

# Signal<T>

```
public abstract class Signal<T> : Signal, IDiagnosticable, IDisposable, IPossiblyDisposed
```

## Value

```
public T Value { get; set; }
```

## PeekValue()

```
public abstract T PeekValue()
```

## SetValue(T)

```
public abstract void SetValue(T newValue)
```

## SetWithoutNotify(T)

```
public abstract void SetWithoutNotify(T newValue)
```

## AddObserver(Action<T>, bool)

```
public IDisposable AddObserver(Action<T> onChanged, bool fireImmediately = false)
```

## T(Signal<T>)

```
public static implicit operator T(Signal<T> signal)
```
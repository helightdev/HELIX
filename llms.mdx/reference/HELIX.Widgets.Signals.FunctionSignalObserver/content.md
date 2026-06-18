# FunctionSignalObserver (/reference/HELIX.Widgets.Signals.FunctionSignalObserver)

# FunctionSignalObserver

```
public class FunctionSignalObserver : IDisposable, IPossiblyDisposed, ISignalObserver
```

## IsDisposed

```
public bool IsDisposed { get; }
```

## Current

```
public Signal Current { get; }
```

## FunctionSignalObserver(Action<Signal>, Action<Signal>, Action<Signal>, bool)

```
public FunctionSignalObserver(Action<Signal> onSignalChanged, Action<Signal> onSignalRemoved = null, Action<Signal> onSignalAdded = null, bool weak = true)
```

## Typed<T>(Action<T>, Action<Signal>, Action<Signal>, bool)

```
public static FunctionSignalObserver Typed<T>(Action<T> onSignalChanged, Action<Signal> onSignalRemoved = null, Action<Signal> onSignalAdded = null, bool weak = true)
```

## Observe(Signal)

```
public void Observe(Signal signal)
```

## OnSignalChanged(Signal)

```
public void OnSignalChanged(Signal signal)
```

## OnSignalRemoved(Signal)

```
public void OnSignalRemoved(Signal signal)
```

## OnSignalAdded(Signal)

```
public void OnSignalAdded(Signal signal)
```

## Dispose()

```
public void Dispose()
```
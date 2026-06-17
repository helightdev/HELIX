# WeakSignalObserver (/reference/HELIX.Widgets.Signals.WeakSignalObserver)

# WeakSignalObserver

```
public class WeakSignalObserver : ISignalObserver, IPossiblyDisposed
```

## WeakSignalObserver(ISignalObserver)

```
public WeakSignalObserver(ISignalObserver observer)
```

## IsDisposed

```
public bool IsDisposed { get; }
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

## OnSignalDirty(Signal)

```
public void OnSignalDirty(Signal signal)
```

## Equals(object)

```
public override bool Equals(object obj)
```

## GetHashCode()

```
public override int GetHashCode()
```
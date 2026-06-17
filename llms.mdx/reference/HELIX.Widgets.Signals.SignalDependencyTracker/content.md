# SignalDependencyTracker (/reference/HELIX.Widgets.Signals.SignalDependencyTracker)

# SignalDependencyTracker

```
public class SignalDependencyTracker : DiagnosticableBase, IDiagnosticable, ISignalObserver, IDisposable, IPossiblyDisposed
```

<p>Provides a mechanism for tracking dependencies between signals.</p>
<p>
When inside the scope of this tracker, all direct value accesses
to a signal will track the signal as a dependency.
</p>

## Current

```
public static SignalDependencyTracker Current
```

## dependencies

```
public readonly Dictionary<Signal, SignalDependencyType> dependencies
```

## owner

```
public object owner
```

## OnDependenciesChanged

```
public event Action<Signal> OnDependenciesChanged
```

## SignalDependencyTracker(Action)

```
public SignalDependencyTracker(Action onDependenciesChanged)
```

## SignalDependencyTracker(ISignalObserver)

```
public SignalDependencyTracker(ISignalObserver forwarder)
```

## IsBuilding

```
public bool IsBuilding { get; }
```

## Dispose()

```
public void Dispose()
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

## RunBuild(Action)

```
public void RunBuild(Action action)
```

## DependOn(Signal)

```
public void DependOn(Signal signal)
```

## DependOnExplicit(Signal, bool)

```
public void DependOnExplicit(Signal signal, bool weak = false)
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```
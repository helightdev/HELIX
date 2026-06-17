# WidgetStateProperty<T> (/reference/HELIX.Widgets.WidgetStateProperty-1)

# WidgetStateProperty<T>

```
public abstract class WidgetStateProperty<T> : DiagnosticableBase, IDiagnosticable
```

## TryResolve(WidgetState, out T)

```
public abstract bool TryResolve(WidgetState state, out T value)
```

## ResolveOrDefault(WidgetState, T)

```
public T ResolveOrDefault(WidgetState state, T defaultValue = default)
```

## WidgetStateProperty<T>(T)

```
public static implicit operator WidgetStateProperty<T>(T constant)
```
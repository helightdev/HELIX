# FuncWidgetStateProperty<T> (/reference/HELIX.Widgets.FuncWidgetStateProperty-1)

# FuncWidgetStateProperty<T>

```
public class FuncWidgetStateProperty<T> : WidgetStateProperty<T>, IDiagnosticable, IEquatable<FuncWidgetStateProperty<T>>
```

## FuncWidgetStateProperty(Func<WidgetState, T>)

```
public FuncWidgetStateProperty(Func<WidgetState, T> resolver)
```

## Equals(FuncWidgetStateProperty<T>)

```
public bool Equals(FuncWidgetStateProperty<T> other)
```

## TryResolve(WidgetState, out T)

```
public override bool TryResolve(WidgetState state, out T value)
```

## Equals(object)

```
public override bool Equals(object obj)
```

## GetHashCode()

```
public override int GetHashCode()
```
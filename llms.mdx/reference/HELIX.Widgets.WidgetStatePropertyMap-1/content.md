# WidgetStatePropertyMap<T> (/reference/HELIX.Widgets.WidgetStatePropertyMap-1)

# WidgetStatePropertyMap<T>

```
public class WidgetStatePropertyMap<T> : WidgetStateProperty<T>, IDiagnosticable
```

## this[WidgetState]

```
public T this[WidgetState state] { get; set; }
```

## TryResolve(WidgetState, out T)

```
public override bool TryResolve(WidgetState state, out T value)
```

## Equals(WidgetStatePropertyMap<T>)

```
protected bool Equals(WidgetStatePropertyMap<T> other)
```

## Equals(object)

```
public override bool Equals(object obj)
```

## GetHashCode()

```
public override int GetHashCode()
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```
# ValueSignal<T> (/reference/HELIX.Widgets.Signals.ValueSignal-1)

# ValueSignal<T>

```
public class ValueSignal<T> : Signal<T>, IDiagnosticable, IDisposable, IPossiblyDisposed
```

A signal that holds a single value.

## comparer

```
protected IEqualityComparer<T> comparer
```

## value

```
protected T value
```

## ValueSignal(T, bool)

```
public ValueSignal(T value = default, bool equality = true)
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

## NotifyListeners()

```
public void NotifyListeners()
```

## ToStringShort()

```
public override string ToStringShort()
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```
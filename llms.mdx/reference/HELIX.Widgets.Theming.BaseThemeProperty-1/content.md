# BaseThemeProperty<T> (/reference/HELIX.Widgets.Theming.BaseThemeProperty-1)

# BaseThemeProperty<T>

```
public abstract class BaseThemeProperty<T> : ThemeProperty, IDiagnosticable
```

## defaultValue

```
protected T defaultValue
```

## BaseThemeProperty(string, T)

```
protected BaseThemeProperty(string key, T defaultValue)
```

## BaseThemeProperty(string)

```
protected BaseThemeProperty(string key)
```

## BaseThemeProperty()

```
protected BaseThemeProperty()
```

## DefaultValue

```
public override object DefaultValue { get; }
```

## TypedDefaultValue

```
public T TypedDefaultValue { get; }
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```

## ResolveStyle(ICustomStyle, out T)

```
public virtual bool ResolveStyle(ICustomStyle customStyle, out T result)
```

## TryCompute(ThemeProviderElement, out T)

```
public virtual bool TryCompute(ThemeProviderElement provider, out T result)
```
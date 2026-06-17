# ThemeProperty (/reference/HELIX.Widgets.Theming.ThemeProperty)

# ThemeProperty

```
public abstract class ThemeProperty : DiagnosticableBase, IDiagnosticable
```

## Loaders

```
public static readonly Dictionary<Type, IThemeStyleValueLoader> Loaders
```

## key

```
protected readonly string key
```

## isDefaultValid

```
protected bool isDefaultValid
```

## ThemeProperty(string)

```
protected ThemeProperty(string key)
```

## ThemeProperty()

```
protected ThemeProperty()
```

## Key

```
public virtual string Key { get; }
```

## ComputedStyleName

```
public virtual string ComputedStyleName { get; }
```

## DefaultValue

```
public virtual object DefaultValue { get; }
```

## IsDefaultValid

```
public virtual bool IsDefaultValid { get; }
```

## TryExtractComponent(Type, ThemeComponent, out object)

```
public virtual bool TryExtractComponent(Type type, ThemeComponent component, out object value)
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```

## ExtractMaybe<T, V>(V, Func<V, IMaybeThemeValue<T>>)

```
public static ThemeProperty<T> ExtractMaybe<T, V>(V value, Func<V, IMaybeThemeValue<T>> resolver) where V : ThemeComponent
```

## ExtractMaybe<T, V>(string, V, Func<V, IMaybeThemeValue<T>>)

```
public static ThemeProperty<T> ExtractMaybe<T, V>(string key, V value, Func<V, IMaybeThemeValue<T>> resolver) where V : ThemeComponent
```

## Extract<T, V>(V, Func<V, T>)

```
public static ThemeProperty<T> Extract<T, V>(V value, Func<V, T> resolver) where V : ThemeComponent
```

## Extract<T, V>(string, V, Func<V, T>)

```
public static ThemeProperty<T> Extract<T, V>(string key, V value, Func<V, T> resolver) where V : ThemeComponent
```
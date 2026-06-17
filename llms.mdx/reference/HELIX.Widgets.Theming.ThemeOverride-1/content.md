# ThemeOverride<T> (/reference/HELIX.Widgets.Theming.ThemeOverride-1)

# ThemeOverride<T>

```
[Serializable]
public class ThemeOverride<T> : ThemeOverride, IMaybeThemeValue<T>, IMaybeThemeValue
```

## type

```
public ThemeOverrideType type
```

## constantValue

```
public T constantValue
```

## propertyReference

```
public string propertyReference
```

## TryGetThemeValue(out object)

```
public override bool TryGetThemeValue(out object value)
```

## ThemeOverride<T>(T)

```
public static implicit operator ThemeOverride<T>(T value)
```
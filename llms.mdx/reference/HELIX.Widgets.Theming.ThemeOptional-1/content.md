# ThemeOptional<T> (/reference/HELIX.Widgets.Theming.ThemeOptional-1)

# ThemeOptional<T>

```
[Serializable]
public class ThemeOptional<T> : ThemeOptional, IMaybeThemeValue<T>, IMaybeThemeValue
```

## hasValue

```
public bool hasValue
```

## value

```
public T value
```

## None

```
public static ThemeOptional<T> None { get; }
```

## TryGetThemeValue(out object)

```
public override bool TryGetThemeValue(out object result)
```

## ThemeOptional<T>(T)

```
public static implicit operator ThemeOptional<T>(T value)
```

## ThemeOptional<T>(ThemeOverride<T>)

```
public static implicit operator ThemeOptional<T>(ThemeOverride<T> themeOverride)
```

## T(ThemeOptional<T>)

```
public static implicit operator T(ThemeOptional<T> optional)
```

## ThemeOverride<T>(ThemeOptional<T>)

```
public static implicit operator ThemeOverride<T>(ThemeOptional<T> optional)
```
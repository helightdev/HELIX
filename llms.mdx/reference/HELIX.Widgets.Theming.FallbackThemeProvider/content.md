# FallbackThemeProvider (/reference/HELIX.Widgets.Theming.FallbackThemeProvider)

# FallbackThemeProvider

```
public class FallbackThemeProvider : IThemeProvider
```

## Instance

```
public static readonly FallbackThemeProvider Instance
```

## GetThemed<T>(BaseThemeProperty<T>, bool)

```
public T GetThemed<T>(BaseThemeProperty<T> property, bool listen = true)
```

Resolves the theme value for the given property.

## TryGetThemed<S>(BaseThemeProperty<S>, out S, bool)

```
public bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true)
```

Tries to resolve the theme value for the given property.
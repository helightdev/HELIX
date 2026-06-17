# IThemeProvider (/reference/HELIX.Widgets.Theming.IThemeProvider)

# IThemeProvider

```
public interface IThemeProvider
```

Interface for a component that provides access to theme values.

## GetThemed<T>(BaseThemeProperty<T>, bool)

```
T GetThemed<T>(BaseThemeProperty<T> property, bool listen = true)
```

Resolves the theme value for the given property.

## TryGetThemed<S>(BaseThemeProperty<S>, out S, bool)

```
bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true)
```

Tries to resolve the theme value for the given property.
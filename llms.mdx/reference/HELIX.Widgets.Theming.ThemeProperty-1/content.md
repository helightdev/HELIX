# ThemeProperty<T> (/reference/HELIX.Widgets.Theming.ThemeProperty-1)

# ThemeProperty<T>

```
public class ThemeProperty<T> : BaseThemeProperty<T>, IDiagnosticable
```

## ThemeProperty(string, object, string)

```
public ThemeProperty(string key = null, object defaultValue = null, string styleName = null)
```

## StyleLoader(string, IThemeStyleValueLoader<T>)

```
public ThemeProperty<T> StyleLoader(string styleName = null, IThemeStyleValueLoader<T> loader = null)
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```

## ComponentExtractor<TypeOfComponent>(Func<TypeOfComponent, T>)

```
public ThemeProperty<T> ComponentExtractor<TypeOfComponent>(Func<TypeOfComponent, T> extractor)
```

## ComponentExtractor<TypeOfComponent, TypeOfMaybe>(Func<TypeOfComponent, TypeOfMaybe>)

```
public ThemeProperty<T> ComponentExtractor<TypeOfComponent, TypeOfMaybe>(Func<TypeOfComponent, TypeOfMaybe> extractor) where TypeOfMaybe : IMaybeThemeValue<T>
```

## ComponentExtractorDefault<TypeOfComponent>(TypeOfComponent, Func<TypeOfComponent, T>)

```
public ThemeProperty<T> ComponentExtractorDefault<TypeOfComponent>(TypeOfComponent value, Func<TypeOfComponent, T> extractor)
```

## ComponentExtractorDefault<TypeOfComponent, TypeOfMaybe>(TypeOfComponent, Func<TypeOfComponent, TypeOfMaybe>)

```
public ThemeProperty<T> ComponentExtractorDefault<TypeOfComponent, TypeOfMaybe>(TypeOfComponent value, Func<TypeOfComponent, TypeOfMaybe> extractor) where TypeOfMaybe : IMaybeThemeValue<T>
```

## Compute(Func<ThemeProviderElement, T>)

```
public ThemeProperty<T> Compute(Func<ThemeProviderElement, T> computeFunc)
```

## ResolveStyle(ICustomStyle, out T)

```
public override bool ResolveStyle(ICustomStyle customStyle, out T result)
```

## TryCompute(ThemeProviderElement, out T)

```
public override bool TryCompute(ThemeProviderElement provider, out T result)
```

## TryExtractComponent(Type, ThemeComponent, out object)

```
public override bool TryExtractComponent(Type type, ThemeComponent component, out object value)
```
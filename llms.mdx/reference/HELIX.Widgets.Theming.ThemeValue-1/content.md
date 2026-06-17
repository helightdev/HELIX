# ThemeValue<T> (/reference/HELIX.Widgets.Theming.ThemeValue-1)

# ThemeValue<T>

```
public class ThemeValue<T> : ThemeValue, IDiagnosticable
```

## valueComparer

```
public EqualityComparer<T> valueComparer
```

## ThemeValue(BaseElement, BaseThemeProperty<T>)

```
public ThemeValue(BaseElement owner, BaseThemeProperty<T> property)
```

## ThemeValue(BaseElement, BaseThemeProperty<T>, OnValueChangedDelegate)

```
public ThemeValue(BaseElement owner, BaseThemeProperty<T> property, ThemeValue<T>.OnValueChangedDelegate onValueChanged)
```

## ThemeProperty

```
public override ThemeProperty ThemeProperty { get; }
```

## ThemeValueState

```
public override ThemeValueState ThemeValueState { get; }
```

## Override

```
public ThemeOverride<T> Override { get; set; }
```

## Value

```
public T Value { get; set; }
```

## OnValueChanged

```
public event ThemeValue<T>.OnValueChangedDelegate OnValueChanged
```

## ApplyOverrides(ThemeOverride<T>)

```
public void ApplyOverrides(ThemeOverride<T> value)
```

## Notify()

```
public void Notify()
```

## SwapProperty(BaseThemeProperty<T>)

```
public void SwapProperty(BaseThemeProperty<T> newProperty)
```

## SwapPropertyByReference(string)

```
public void SwapPropertyByReference(string reference)
```

## ReloadStyles()

```
public override void ReloadStyles()
```

## DebugFillProperties(DiagnosticPropertiesBuilder)

```
public override void DebugFillProperties(DiagnosticPropertiesBuilder properties)
```
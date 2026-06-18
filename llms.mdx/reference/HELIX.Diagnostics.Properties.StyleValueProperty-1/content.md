# StyleValueProperty<T> (/reference/HELIX.Diagnostics.Properties.StyleValueProperty-1)

# StyleValueProperty<T>

```
public class StyleValueProperty<T> : DiagnosticsProperty<IStyleValue<T>>
```

## StyleValueProperty(string, IStyleValue<T>, string, string, bool, object, string, DiagnosticsTreeStyle, DiagnosticLevel)

```
public StyleValueProperty(string name, IStyleValue<T> value, string ifNull = null, string ifInitial = null, bool showName = true, object defaultValue = null, string tooltip = null, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine, DiagnosticLevel level = DiagnosticLevel.Info)
```

## IfInitial

```
public string IfInitial { get; set; }
```

## Level

```
public override DiagnosticLevel Level { get; }
```

## UnwrapStyleValueNode(T)

```
public virtual DiagnosticsNode UnwrapStyleValueNode(T value)
```

## ValueToString(TextTreeConfiguration)

```
public override string ValueToString(TextTreeConfiguration parentConfiguration = null)
```
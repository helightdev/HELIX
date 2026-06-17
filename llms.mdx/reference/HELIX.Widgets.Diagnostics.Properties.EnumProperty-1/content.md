# EnumProperty<T> (/reference/HELIX.Widgets.Diagnostics.Properties.EnumProperty-1)

# EnumProperty<T>

```
public sealed class EnumProperty<T> : DiagnosticsProperty<T> where T : Enum
```

## EnumProperty(string, T, object, DiagnosticLevel, bool)

```
public EnumProperty(string name, T value, object defaultValue = null, DiagnosticLevel level = DiagnosticLevel.Info, bool showName = true)
```

## ValueToString(TextTreeConfiguration)

```
public override string ValueToString(TextTreeConfiguration parentConfiguration = null)
```
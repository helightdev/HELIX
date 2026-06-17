# IntProperty (/reference/HELIX.Widgets.Diagnostics.Properties.IntProperty)

# IntProperty

```
public sealed class IntProperty : DiagnosticsProperty<int?>
```

## IntProperty(string, int?, string, string, bool, object, DiagnosticsTreeStyle, DiagnosticLevel)

```
public IntProperty(string name, int? value, string ifNull = null, string unit = null, bool showName = true, object defaultValue = null, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine, DiagnosticLevel level = DiagnosticLevel.Info)
```

## Unit

```
public string Unit { get; }
```

## ValueToString(TextTreeConfiguration)

```
public override string ValueToString(TextTreeConfiguration parentConfiguration = null)
```
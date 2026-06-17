# FloatProperty (/reference/HELIX.Widgets.Diagnostics.Properties.FloatProperty)

# FloatProperty

```
public sealed class FloatProperty : DiagnosticsProperty<float?>
```

## FloatProperty(string, float?, string, string, string, object, bool, DiagnosticsTreeStyle, DiagnosticLevel)

```
public FloatProperty(string name, float? value, string ifNull = null, string unit = null, string tooltip = null, object defaultValue = null, bool showName = true, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine, DiagnosticLevel level = DiagnosticLevel.Info)
```

## Level

```
public override DiagnosticLevel Level { get; }
```

## Unit

```
public string Unit { get; }
```

## ValueToString(TextTreeConfiguration)

```
public override string ValueToString(TextTreeConfiguration parentConfiguration = null)
```
# PercentProperty (/reference/HELIX.Diagnostics.Properties.PercentProperty)

# PercentProperty

```
public sealed class PercentProperty : DiagnosticsProperty<float?>
```

## PercentProperty(string, float?, string, bool, string, string, object, DiagnosticLevel)

```
public PercentProperty(string name, float? fraction, string ifNull = null, bool showName = true, string tooltip = null, string unit = null, object defaultValue = null, DiagnosticLevel level = DiagnosticLevel.Info)
```

## Unit

```
public string Unit { get; }
```

## Level

```
public override DiagnosticLevel Level { get; }
```

## ValueToString(TextTreeConfiguration)

```
public override string ValueToString(TextTreeConfiguration parentConfiguration = null)
```
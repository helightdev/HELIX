# NumProperty<T> (/reference/HELIX.Diagnostics.Properties.NumProperty-1)

# NumProperty<T>

```
public abstract class NumProperty<T> : DiagnosticsProperty<T> where T : struct, IFormattable
```

## NumProperty(string, T?, string, string, bool, object, string, DiagnosticsTreeStyle, DiagnosticLevel)

```
protected NumProperty(string name, T? value, string ifNull = null, string unit = null, bool showName = true, object defaultValue = null, string tooltip = null, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine, DiagnosticLevel level = DiagnosticLevel.Info)
```

## HasValue

```
protected bool HasValue { get; }
```

## Unit

```
public string Unit { get; }
```

## NumberToString()

```
public abstract string NumberToString()
```

## ValueToString(TextTreeConfiguration)

```
public override string ValueToString(TextTreeConfiguration parentConfiguration = null)
```
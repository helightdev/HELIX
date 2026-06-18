# StringProperty (/reference/HELIX.Diagnostics.Properties.StringProperty)

# StringProperty

```
public class StringProperty : DiagnosticsProperty<string>
```

## StringProperty(string, string, string, string, bool, object, bool, string, DiagnosticsTreeStyle, DiagnosticLevel)

```
public StringProperty(string name, string value, string description = null, string tooltip = null, bool showName = true, object defaultValue = null, bool quoted = true, string ifEmpty = null, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine, DiagnosticLevel level = DiagnosticLevel.Info)
```

## Quoted

```
public bool Quoted { get; }
```

## ValueToString(TextTreeConfiguration)

```
public override string ValueToString(TextTreeConfiguration parentConfiguration = null)
```
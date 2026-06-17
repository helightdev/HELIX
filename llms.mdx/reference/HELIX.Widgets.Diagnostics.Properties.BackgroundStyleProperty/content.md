# BackgroundStyleProperty (/reference/HELIX.Widgets.Diagnostics.Properties.BackgroundStyleProperty)

# BackgroundStyleProperty

```
public class BackgroundStyleProperty : DiagnosticsProperty<BackgroundStyle>
```

## BackgroundStyleProperty(string, BackgroundStyle, string, bool, object, string, DiagnosticsTreeStyle, DiagnosticLevel)

```
public BackgroundStyleProperty(string name, BackgroundStyle value, string ifNull = null, bool showName = true, object defaultValue = null, string tooltip = null, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine, DiagnosticLevel level = DiagnosticLevel.Info)
```

## Level

```
public override DiagnosticLevel Level { get; }
```

## GetProperties()

```
public override List<DiagnosticsNode> GetProperties()
```

## ValueToString(TextTreeConfiguration)

```
public override string ValueToString(TextTreeConfiguration parentConfiguration = null)
```
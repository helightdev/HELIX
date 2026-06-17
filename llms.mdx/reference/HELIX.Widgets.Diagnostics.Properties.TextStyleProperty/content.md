# TextStyleProperty (/reference/HELIX.Widgets.Diagnostics.Properties.TextStyleProperty)

# TextStyleProperty

```
public class TextStyleProperty : DiagnosticsProperty<TextStyle>
```

## TextStyleProperty(string, TextStyle, string, bool, object, string, DiagnosticsTreeStyle, DiagnosticLevel)

```
public TextStyleProperty(string name, TextStyle value, string ifNull = null, bool showName = true, object defaultValue = null, string tooltip = null, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine, DiagnosticLevel level = DiagnosticLevel.Info)
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
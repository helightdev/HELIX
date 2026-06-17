# FormattingProperty<T> (/reference/HELIX.Widgets.Diagnostics.Properties.FormattingProperty-1)

# FormattingProperty<T>

```
public class FormattingProperty<T> : DiagnosticsProperty<T>
```

## FormattingProperty(string, T, Func<T, string>, string, string, string, bool, bool, object, string, bool, string, bool, bool, bool, DiagnosticsTreeStyle, DiagnosticLevel)

```
public FormattingProperty(string name, T value, Func<T, string> formatter, string description = null, string ifNull = null, string ifEmpty = null, bool showName = true, bool showSeparator = true, object defaultValue = null, string tooltip = null, bool missingIfNull = false, string linePrefix = null, bool expandableValue = false, bool allowWrap = true, bool allowNameWrap = true, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine, DiagnosticLevel level = DiagnosticLevel.Info)
```

## ValueToString(TextTreeConfiguration)

```
public override string ValueToString(TextTreeConfiguration parentConfiguration = null)
```
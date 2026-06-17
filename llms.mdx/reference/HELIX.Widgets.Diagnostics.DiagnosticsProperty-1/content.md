# DiagnosticsProperty<T> (/reference/HELIX.Widgets.Diagnostics.DiagnosticsProperty-1)

# DiagnosticsProperty<T>

```
public class DiagnosticsProperty<T> : DiagnosticsNode
```

## DiagnosticsProperty(string, T, string, string, string, bool, bool, object, string, bool, string, bool, bool, bool, DiagnosticsTreeStyle, DiagnosticLevel)

```
public DiagnosticsProperty(string name, T value, string description = null, string ifNull = null, string ifEmpty = null, bool showName = true, bool showSeparator = true, object defaultValue = null, string tooltip = null, bool missingIfNull = false, string linePrefix = null, bool expandableValue = false, bool allowWrap = true, bool allowNameWrap = true, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine, DiagnosticLevel level = DiagnosticLevel.Info)
```

## DiagnosticsProperty(string, ComputePropertyValueCallback<T>, string, string, string, bool, bool, object, string, bool, bool, bool, bool, DiagnosticsTreeStyle, DiagnosticLevel)

```
public DiagnosticsProperty(string name, ComputePropertyValueCallback<T> computeValue, string description, string ifNull, string ifEmpty, bool showName, bool showSeparator, object defaultValue, string tooltip, bool missingIfNull, bool expandableValue, bool allowWrap, bool allowNameWrap, DiagnosticsTreeStyle style, DiagnosticLevel level)
```

## AllowWrap

```
public override bool AllowWrap { get; }
```

## AllowNameWrap

```
public override bool AllowNameWrap { get; }
```

## AllowWrapImpl

```
public bool AllowWrapImpl { get; }
```

## AllowNameWrapImpl

```
public bool AllowNameWrapImpl { get; }
```

## ExpandableValue

```
public bool ExpandableValue { get; }
```

## IfNull

```
public string IfNull { get; }
```

## IfEmpty

```
public string IfEmpty { get; }
```

## Tooltip

```
public string Tooltip { get; }
```

## MissingIfNull

```
public bool MissingIfNull { get; }
```

## DefaultValue

```
public object DefaultValue { get; }
```

## Exception

```
public Exception Exception { get; }
```

## Value

```
public override object Value { get; }
```

## ValueTyped

```
public virtual T ValueTyped { get; }
```

## IsInteresting

```
protected virtual bool IsInteresting { get; }
```

## Level

```
public override DiagnosticLevel Level { get; }
```

## Lazy(string, ComputePropertyValueCallback<T>, string, string, string, bool, bool, object, string, bool, bool, bool, bool, DiagnosticsTreeStyle, DiagnosticLevel)

```
public static DiagnosticsProperty<T> Lazy(string name, ComputePropertyValueCallback<T> computeValue, string description = null, string ifNull = null, string ifEmpty = null, bool showName = true, bool showSeparator = true, object defaultValue = null, string tooltip = null, bool missingIfNull = false, bool expandableValue = false, bool allowWrap = true, bool allowNameWrap = true, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine, DiagnosticLevel level = DiagnosticLevel.Info)
```

## MaybeCacheValue()

```
protected void MaybeCacheValue()
```

## ValueToString(TextTreeConfiguration)

```
public virtual string ValueToString(TextTreeConfiguration parentConfiguration = null)
```

## ToDescription(TextTreeConfiguration)

```
public override string ToDescription(TextTreeConfiguration parentConfiguration = null)
```

## AddTooltip(string)

```
protected string AddTooltip(string text)
```

## GetProperties()

```
public override List<DiagnosticsNode> GetProperties()
```

## GetChildren()

```
public override List<DiagnosticsNode> GetChildren()
```
# DiagnosticsNode (/reference/HELIX.Widgets.Diagnostics.DiagnosticsNode)

# DiagnosticsNode

```
public abstract class DiagnosticsNode
```

## Null

```
public static readonly DiagnosticsNode Null
```

## DiagnosticsNode(string, DiagnosticsTreeStyle?, bool, bool, string)

```
protected DiagnosticsNode(string name, DiagnosticsTreeStyle? style = null, bool showName = true, bool showSeparator = true, string linePrefix = null)
```

## Name

```
public string Name { get; }
```

## Value

```
public virtual object Value { get; }
```

## Style

```
public DiagnosticsTreeStyle? Style { get; }
```

## ShowSeparator

```
public bool ShowSeparator { get; }
```

## ShowName

```
public bool ShowName { get; }
```

## LinePrefix

```
public string LinePrefix { get; }
```

## AllowWrap

```
public virtual bool AllowWrap { get; }
```

## AllowNameWrap

```
public virtual bool AllowNameWrap { get; }
```

## AllowTruncate

```
public virtual bool AllowTruncate { get; }
```

## EmptyBodyDescription

```
public virtual string EmptyBodyDescription { get; }
```

## Level

```
public virtual DiagnosticLevel Level { get; }
```

## TextTreeConfiguration

```
protected virtual TextTreeConfiguration TextTreeConfiguration { get; }
```

## IsFiltered(DiagnosticLevel)

```
public bool IsFiltered(DiagnosticLevel minLevel)
```

## ToDescription(TextTreeConfiguration)

```
public abstract string ToDescription(TextTreeConfiguration parentConfiguration = null)
```

## GetProperties()

```
public abstract List<DiagnosticsNode> GetProperties()
```

## GetChildren()

```
public abstract List<DiagnosticsNode> GetChildren()
```

## ToStringDeep(string, string, TextTreeConfiguration, DiagnosticLevel, int)

```
public string ToStringDeep(string prefixLineOne = "", string prefixOtherLines = null, TextTreeConfiguration parentConfiguration = null, DiagnosticLevel minLevel = DiagnosticLevel.Debug, int wrapWidth = 65)
```

## ToString()

```
public override string ToString()
```

## Message(string, DiagnosticsTreeStyle, DiagnosticLevel, bool)

```
public static DiagnosticsNode Message(string message, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine, DiagnosticLevel level = DiagnosticLevel.Info, bool allowWrap = true)
```
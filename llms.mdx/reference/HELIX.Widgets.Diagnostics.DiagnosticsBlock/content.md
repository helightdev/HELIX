# DiagnosticsBlock (/reference/HELIX.Widgets.Diagnostics.DiagnosticsBlock)

# DiagnosticsBlock

```
public class DiagnosticsBlock : DiagnosticsNode
```

## DiagnosticsBlock(string, DiagnosticsTreeStyle, bool, bool, string, object, string, DiagnosticLevel, bool, List<DiagnosticsNode>, List<DiagnosticsNode>)

```
public DiagnosticsBlock(string name = null, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.Whitespace, bool showName = true, bool showSeparator = true, string linePrefix = null, object value = null, string description = "", DiagnosticLevel level = DiagnosticLevel.Info, bool allowTruncate = false, List<DiagnosticsNode> children = null, List<DiagnosticsNode> properties = null)
```

## ValueObject

```
public object ValueObject { get; }
```

## LevelValue

```
public DiagnosticLevel LevelValue { get; }
```

## AllowTruncateValue

```
public bool AllowTruncateValue { get; }
```

## Value

```
public override object Value { get; }
```

## Level

```
public override DiagnosticLevel Level { get; }
```

## AllowTruncate

```
public override bool AllowTruncate { get; }
```

## GetChildren()

```
public override List<DiagnosticsNode> GetChildren()
```

## GetProperties()

```
public override List<DiagnosticsNode> GetProperties()
```

## ToDescription(TextTreeConfiguration)

```
public override string ToDescription(TextTreeConfiguration parentConfiguration = null)
```
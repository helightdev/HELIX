# DiagnosticableNode (/reference/HELIX.Widgets.Diagnostics.DiagnosticableNode)

# DiagnosticableNode

```
public class DiagnosticableNode : DiagnosticsNode
```

## DiagnosticableNode(string, IDiagnosticable, DiagnosticsTreeStyle?)

```
public DiagnosticableNode(string name, IDiagnosticable value, DiagnosticsTreeStyle? style)
```

## ValueTyped

```
public IDiagnosticable ValueTyped { get; }
```

## Value

```
public override object Value { get; }
```

## EmptyBodyDescription

```
public override string EmptyBodyDescription { get; }
```

## ToDescription(TextTreeConfiguration)

```
public override string ToDescription(TextTreeConfiguration parentConfiguration = null)
```

## GetProperties()

```
public override List<DiagnosticsNode> GetProperties()
```

## GetChildren()

```
public override List<DiagnosticsNode> GetChildren()
```
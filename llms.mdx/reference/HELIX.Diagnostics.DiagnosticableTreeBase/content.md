# DiagnosticableTreeBase (/reference/HELIX.Diagnostics.DiagnosticableTreeBase)

# DiagnosticableTreeBase

```
public abstract class DiagnosticableTreeBase : DiagnosticableBase, IDiagnosticableTree, IDiagnosticable
```

## DebugDescribeChildren()

```
public virtual List<DiagnosticsNode> DebugDescribeChildren()
```

## ToStringDeep(string, string, DiagnosticLevel, int)

```
public virtual string ToStringDeep(string prefixLineOne = "", string prefixOtherLines = null, DiagnosticLevel minLevel = DiagnosticLevel.Debug, int wrapWidth = 65)
```

## ToStringShallow(string, DiagnosticLevel)

```
public virtual string ToStringShallow(string joiner = ", ", DiagnosticLevel minLevel = DiagnosticLevel.Debug)
```

## ToDiagnosticsNode(string, DiagnosticsTreeStyle?)

```
public override DiagnosticsNode ToDiagnosticsNode(string name = null, DiagnosticsTreeStyle? style = null)
```
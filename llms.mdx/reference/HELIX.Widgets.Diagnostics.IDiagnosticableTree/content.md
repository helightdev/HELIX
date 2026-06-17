# IDiagnosticableTree (/reference/HELIX.Widgets.Diagnostics.IDiagnosticableTree)

# IDiagnosticableTree

```
public interface IDiagnosticableTree : IDiagnosticable
```

## DebugDescribeChildren()

```
List<DiagnosticsNode> DebugDescribeChildren()
```

## ToStringDeep(string, string, DiagnosticLevel, int)

```
string ToStringDeep(string prefixLineOne = "", string prefixOtherLines = null, DiagnosticLevel minLevel = DiagnosticLevel.Debug, int wrapWidth = 65)
```

## ToStringShallow(string, DiagnosticLevel)

```
string ToStringShallow(string joiner = ", ", DiagnosticLevel minLevel = DiagnosticLevel.Debug)
```

## DefaultToStringShallow(IDiagnosticableTree, string, DiagnosticLevel)

```
public static string DefaultToStringShallow(IDiagnosticableTree tree, string joiner = ", ", DiagnosticLevel minLevel = DiagnosticLevel.Debug)
```
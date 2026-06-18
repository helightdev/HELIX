# HelixDiagnosticException (/reference/HELIX.Diagnostics.Error.HelixDiagnosticException)

# HelixDiagnosticException

```
public sealed class HelixDiagnosticException : Exception, ISerializable
```

## HelixDiagnosticException(string)

```
public HelixDiagnosticException(string message)
```

## HelixDiagnosticException(IEnumerable<DiagnosticsNode>)

```
public HelixDiagnosticException(IEnumerable<DiagnosticsNode> diagnostics)
```

## Diagnostics

```
public IReadOnlyList<DiagnosticsNode> Diagnostics { get; }
```

## Message

```
public override string Message { get; }
```

## FromParts(params DiagnosticsNode[])

```
public static HelixDiagnosticException FromParts(params DiagnosticsNode[] diagnostics)
```

## FromException(Exception, string)

```
public static HelixDiagnosticException FromException(Exception exception, string summary = null)
```

## ToStringDeep(int)

```
public string ToStringDeep(int wrapWidth = 100)
```

## ToString()

```
public override string ToString()
```

## ToDiagnosticsNode()

```
public DiagnosticsNode ToDiagnosticsNode()
```
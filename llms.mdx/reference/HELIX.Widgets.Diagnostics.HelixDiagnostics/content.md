# HelixDiagnostics (/reference/HELIX.Widgets.Diagnostics.HelixDiagnostics)

# HelixDiagnostics

```
public static class HelixDiagnostics
```

## PresentError

```
public static Func<HelixDiagnosticException, string> PresentError
```

## OnErrorReported

```
public static event Action<HelixDiagnosticException, DiagnosticLevel> OnErrorReported
```

## LogSignature

```
public static string LogSignature
```

## FormatException(Exception, string)

```
public static string FormatException(Exception exception, string summary = null)
```

## Build(string, string, IEnumerable<DiagnosticsNode>, Exception, string, IEnumerable<DiagnosticsNode>)

```
public static HelixDiagnosticException Build(string summary, string description = null, IEnumerable<DiagnosticsNode> details = null, Exception exception = null, string stackTrace = null, IEnumerable<DiagnosticsNode> hints = null)
```

## Build(string, Action<InformationCollector>, Exception, string)

```
public static HelixDiagnosticException Build(string summary, Action<InformationCollector> collectInformation, Exception exception = null, string stackTrace = null)
```

## Throw(string, string, IEnumerable<DiagnosticsNode>, Exception, string, IEnumerable<DiagnosticsNode>)

```
public static void Throw(string summary, string description = null, IEnumerable<DiagnosticsNode> details = null, Exception exception = null, string stackTrace = null, IEnumerable<DiagnosticsNode> hints = null)
```

## Report(HelixDiagnosticException, DiagnosticLevel)

```
public static void Report(this HelixDiagnosticException exception, DiagnosticLevel level)
```

## ShortHash(object)

```
public static string ShortHash(this object obj)
```

## DescribeIdentity(object)

```
public static string DescribeIdentity(this object obj)
```

## ToStringNullable(object)

```
public static string ToStringNullable(this object obj)
```
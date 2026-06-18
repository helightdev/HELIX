# IterableProperty<T> (/reference/HELIX.Diagnostics.Properties.IterableProperty-1)

# IterableProperty<T>

```
public class IterableProperty<T> : DiagnosticsProperty<IEnumerable<T>>
```

## IterableProperty(string, IEnumerable<T>, object, string, string, DiagnosticsTreeStyle, bool, bool, bool, DiagnosticLevel)

```
public IterableProperty(string name, IEnumerable<T> value, object defaultValue = null, string ifNull = null, string ifEmpty = "[]", DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine, bool showName = true, bool showSeparator = true, bool identityOnly = false, DiagnosticLevel level = DiagnosticLevel.Info)
```

## IdentityOnly

```
public bool IdentityOnly { get; set; }
```

## Level

```
public override DiagnosticLevel Level { get; }
```

## ValueToString(TextTreeConfiguration)

```
public override string ValueToString(TextTreeConfiguration parentConfiguration = null)
```

## FormatItem(T, TextTreeConfiguration)

```
protected virtual string FormatItem(T v, TextTreeConfiguration parentConfiguration)
```
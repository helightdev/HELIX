# ObjectFlagProperty<T> (/reference/HELIX.Diagnostics.Properties.ObjectFlagProperty-1)

# ObjectFlagProperty<T>

```
public sealed class ObjectFlagProperty<T> : DiagnosticsProperty<T>
```

## ObjectFlagProperty(string, T, string, string, bool, DiagnosticLevel)

```
public ObjectFlagProperty(string name, T value, string ifPresent = null, string ifNull = null, bool showName = false, DiagnosticLevel level = DiagnosticLevel.Info)
```

## IfPresent

```
public string IfPresent { get; }
```

## Level

```
public override DiagnosticLevel Level { get; }
```

## ValueToString(TextTreeConfiguration)

```
public override string ValueToString(TextTreeConfiguration parentConfiguration = null)
```
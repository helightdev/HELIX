# FlagProperty (/reference/HELIX.Widgets.Diagnostics.Properties.FlagProperty)

# FlagProperty

```
public sealed class FlagProperty : DiagnosticsProperty<bool?>
```

## FlagProperty(string, bool?, string, string, bool, object, DiagnosticLevel)

```
public FlagProperty(string name, bool? value, string ifTrue = null, string ifFalse = null, bool showName = false, object defaultValue = null, DiagnosticLevel level = DiagnosticLevel.Info)
```

## IfTrue

```
public string IfTrue { get; }
```

## IfFalse

```
public string IfFalse { get; }
```

## Level

```
public override DiagnosticLevel Level { get; }
```

## ValueToString(TextTreeConfiguration)

```
public override string ValueToString(TextTreeConfiguration parentConfiguration = null)
```
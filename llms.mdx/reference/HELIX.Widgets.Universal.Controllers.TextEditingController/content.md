# TextEditingController (/reference/HELIX.Widgets.Universal.Controllers.TextEditingController)

# TextEditingController

```
public class TextEditingController : ValueSignal<string>, IDiagnosticable, IDisposable, IPossiblyDisposed
```

## widgetState

```
public readonly WidgetStateController widgetState
```

## enabled

```
public bool enabled
```

## onBeginEditing

```
public Action onBeginEditing
```

## onCanceled

```
public Action onCanceled
```

## onChanged

```
public Action<string> onChanged
```

## onEndEditing

```
public Action onEndEditing
```

## onSubmitted

```
public Action<string> onSubmitted
```

## TextEditingController(WidgetStateController, string)

```
public TextEditingController(WidgetStateController widgetState = null, string initialValue = "")
```

## Enabled

```
public bool Enabled { get; }
```

## SetValue(string)

```
public override void SetValue(string newValue)
```
# GlobalKey (/reference/HELIX.Widgets.GlobalKey)

# GlobalKey

```
public class GlobalKey : BaseKey, IDiagnosticable
```

## debugName

```
public readonly string debugName
```

## GlobalKey(string)

```
public GlobalKey(string debugName = null)
```

## Target

```
public IWidgetElement Target { get; protected set; }
```

## OnMounted(IWidgetElement, Widget)

```
public override void OnMounted(IWidgetElement element, Widget descriptor)
```

## OnUnmounted(IWidgetElement)

```
public override void OnUnmounted(IWidgetElement element)
```

## ToStringShort()

```
public override string ToStringShort()
```
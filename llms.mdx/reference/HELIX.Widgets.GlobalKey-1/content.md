# GlobalKey<T> (/reference/HELIX.Widgets.GlobalKey-1)

# GlobalKey<T>

```
public class GlobalKey<T> : GlobalKey, IDiagnosticable where T : VisualElement
```

## GlobalKey(string)

```
public GlobalKey(string debugName = null)
```

## Element

```
public T Element { get; }
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
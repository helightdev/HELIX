# Reconciler (/reference/HELIX.Widgets.Reconciler)

# Reconciler

```
public static class Reconciler
```

## CanReuse(Widget, Widget)

```
public static bool CanReuse(Widget previous, Widget current)
```

## ExpandElement(VisualElement)

```
public static IWidgetElement ExpandElement(VisualElement child)
```

## ReconcileSingle(ISingleChildContainer, Widget, IWidgetElement)

```
public static void ReconcileSingle(ISingleChildContainer container, Widget descriptor, IWidgetElement owner)
```

## ReconcileCollection(IWidgetElementCollection, IReadOnlyList<Widget>, IWidgetElement)

```
public static void ReconcileCollection(IWidgetElementCollection collection, IReadOnlyList<Widget> descriptors, IWidgetElement owner)
```

## Reconcile(IWidgetElement, Widget)

```
public static void Reconcile(IWidgetElement element, Widget descriptor)
```
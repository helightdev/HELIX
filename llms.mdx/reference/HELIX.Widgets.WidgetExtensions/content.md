# WidgetExtensions (/reference/HELIX.Widgets.WidgetExtensions)

# WidgetExtensions

```
public static class WidgetExtensions
```

## ToBuildable(Widget)

```
public static IBuildable ToBuildable(this Widget widget)
```

Converts a <a data-furef-uid="HELIX.Widgets.Widget">Widget</a> instance to an <a data-furef-uid="HELIX.Widgets.IBuildable">IBuildable</a>.

## ToFactory(Widget, Action<VisualElement>)

```
public static ElementFactory<VisualElement> ToFactory(this Widget widget, Action<VisualElement> apply = null)
```

Converts a <a data-furef-uid="HELIX.Widgets.Widget">Widget</a> instance to an <a data-furef-uid="HELIX.Widgets.Theming.ElementFactory">ElementFactory</a>.

## ToWidget(ElementFactory)

```
public static Widget ToWidget(this ElementFactory factory)
```

Converts a generic <a data-furef-uid="HELIX.Widgets.Theming.ElementFactory">ElementFactory</a> to a <a data-furef-uid="HELIX.Widgets.Widget">Widget</a>.

## WithSpace(InformationCollector)

```
public static InformationCollector WithSpace(this InformationCollector collector)
```

## OffendingWidget(InformationCollector, Widget)

```
public static InformationCollector OffendingWidget(this InformationCollector collector, Widget widget)
```

Adds information about the offending widget to the collector, including a spacer before for better readability.

## OffendingElement(InformationCollector, IWidgetElement)

```
public static InformationCollector OffendingElement(this InformationCollector collector, IWidgetElement widget)
```

Adds information about the offending element to the collector, including a spacer before for better readability.

## OwnerChain(InformationCollector, BuildContext)

```
public static InformationCollector OwnerChain(this InformationCollector collector, BuildContext context)
```

Adds information about this context's owner chain to the collector, including a spacer before for better readability.

## Get<T>(ThemeProperty<T>, IThemeProvider, bool)

```
public static T Get<T>(this ThemeProperty<T> property, IThemeProvider context, bool listen = true)
```

Resolves the value of the given <a data-furef-uid="HELIX.Widgets.Theming.ThemeProperty%601">ThemeProperty</a> using the given <a data-furef-uid="HELIX.Widgets.Theming.IThemeProvider">IThemeProvider</a>.
If the context is null, global theme values will be used as a fallback before
resorting to the default value of the property.

## TryGet<T>(ThemeProperty<T>, IThemeProvider, out T, bool)

```
public static bool TryGet<T>(this ThemeProperty<T> property, IThemeProvider context, out T value, bool listen = true)
```

Attempts to resolve the value of the given <a data-furef-uid="HELIX.Widgets.Theming.ThemeProperty%601">ThemeProperty</a> using the given <a data-furef-uid="HELIX.Widgets.Theming.IThemeProvider">IThemeProvider</a>.
If the context is null, global theme values will be used as a fallback before
resorting to the default value of the property.
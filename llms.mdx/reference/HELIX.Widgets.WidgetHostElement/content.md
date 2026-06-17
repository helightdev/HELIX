# WidgetHostElement (/reference/HELIX.Widgets.WidgetHostElement)

# WidgetHostElement

```
public class WidgetHostElement : BuildingWidgetBaseElement<WidgetHostElement.WidgetType>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IThemeProvider, ISingleChildContainer, IHierarchyDisposable, IDisposable, IElement
```

Represents a <a data-furef-uid="UnityEngine.UIElements.VisualElement">VisualElement</a> that hosts a <a data-furef-uid="HELIX.Widgets.Widget">Widget</a> derived from an <a data-furef-uid="HELIX.Widgets.IBuildable">IBuildable</a>.

This is the preferred entry point for integrating <a data-furef-uid="HELIX.Widgets.Widget">Widget</a>s into a UI-Toolkit-based application.
It may be used to integrate widgets into non-widget-based parts of the visual tree.

## Instances

```
public static readonly HashSet<WidgetHostElement> Instances
```

## WidgetHostElement()

```
public WidgetHostElement()
```

## Buildable

```
public IBuildable Buildable { get; set; }
```

## Dispose()

```
public void Dispose()
```

## OnAttached(AttachToPanelEvent)

```
protected override void OnAttached(AttachToPanelEvent evt)
```

## GetBuildableForWidget(WidgetType, WidgetType)

```
protected override IBuildable GetBuildableForWidget(WidgetHostElement.WidgetType previous, WidgetHostElement.WidgetType widget)
```
# ScaffoldElement (/reference/HELIX.Widgets.Navigation.ScaffoldElement)

# ScaffoldElement

```
[UxmlElement]
public class ScaffoldElement : SingleChildWidgetBaseElement<HScaffold>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider, ISingleChildContainer
```

## ScaffoldElement()

```
public ScaffoldElement()
```

## contentContainer

```
public override VisualElement contentContainer { get; }
```

<p>
Logical container where child elements are added.
If a child is added to this element, the child is added to this element's content container instead.
</p>

## AddOverlay(VisualElement)

```
public OverlayEntry AddOverlay(VisualElement element)
```

## AddOverlay(VisualElement, Vector2, Vector2)

```
public OverlayEntry AddOverlay(VisualElement element, Vector2 localPosition, Vector2 size)
```

## AddAnchoredOverlay(VisualElement, VisualElement, bool)

```
public OverlayEntry AddAnchoredOverlay(VisualElement anchor, VisualElement overlay, bool link = true)
```

## RemoveOverlay(OverlayEntry)

```
public void RemoveOverlay(OverlayEntry entry)
```

## Get(VisualElement)

```
public static ScaffoldElement Get(VisualElement context)
```
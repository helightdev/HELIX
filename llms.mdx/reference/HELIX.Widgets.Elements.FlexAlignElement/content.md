# FlexAlignElement (/reference/HELIX.Widgets.Elements.FlexAlignElement)

# FlexAlignElement

```
[UxmlElement]
public class FlexAlignElement : SingleChildWidgetBaseElement<HAlign>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider, ISingleChildContainer, IPreferExplicitFlex
```

## FlexAlignElement()

```
public FlexAlignElement()
```

## contentContainer

```
public override VisualElement contentContainer { get; }
```

<p>
Logical container where child elements are added.
If a child is added to this element, the child is added to this element's content container instead.
</p>

## Alignment

```
[UxmlAttribute]
public Alignment Alignment { get; set; }
```

## Child

```
public override VisualElement Child { get; set; }
```

## Refresh()

```
public void Refresh()
```

## Apply(HAlign, HAlign)

```
public override void Apply(HAlign previous, HAlign widget)
```
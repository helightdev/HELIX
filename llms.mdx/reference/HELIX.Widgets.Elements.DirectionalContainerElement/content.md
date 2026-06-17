# DirectionalContainerElement (/reference/HELIX.Widgets.Elements.DirectionalContainerElement)

# DirectionalContainerElement

```
[UxmlElement]
public abstract class DirectionalContainerElement : MultiChildWidgetBaseElement<DirectionalContainerWidget>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider, IMultiChildContainer, IWidgetElementCollection, IPreferExplicitFlex
```

## DirectionalContainerElement()

```
protected DirectionalContainerElement()
```

## Gap

```
[UxmlAttribute]
public float Gap { get; set; }
```

## MainAxisAlign

```
[UxmlAttribute]
public Justify MainAxisAlign { get; set; }
```

## CrossAxisAlign

```
[UxmlAttribute]
public Align CrossAxisAlign { get; set; }
```

## Reverse

```
[UxmlAttribute]
public bool Reverse { get; set; }
```

## Childs

```
public override IEnumerable<VisualElement> Childs { get; set; }
```

## GetFlexDirection(bool)

```
protected abstract FlexDirection GetFlexDirection(bool reverse)
```

## GetAxis()

```
protected abstract Axis GetAxis()
```

## PreferredFlexAxis

```
public Axis PreferredFlexAxis { get; }
```

## Apply(DirectionalContainerWidget, DirectionalContainerWidget)

```
public override void Apply(DirectionalContainerWidget previous, DirectionalContainerWidget widget)
```

## LoadWidgetElements(List<IWidgetElement>)

```
public override void LoadWidgetElements(List<IWidgetElement> elements)
```

## UpdateWidgetElements(IWidgetElement[], ReconcilerCollectionDelta[])

```
public override void UpdateWidgetElements(IWidgetElement[] result, ReconcilerCollectionDelta[] deltas)
```
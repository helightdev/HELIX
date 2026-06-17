# GenericSlider (/reference/HELIX.Widgets.Elements.GenericSlider)

# GenericSlider

```
[UxmlElement]
public class GenericSlider : BaseElement, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IElement
```

## GenericSlider()

```
public GenericSlider()
```

## Value

```
[UxmlAttribute]
[Range(0, 1)]
public float Value { get; set; }
```

## ThumbRange

```
[UxmlAttribute]
[Range(0, 1)]
public float ThumbRange { get; set; }
```

## Axis

```
[UxmlAttribute]
public Axis Axis { get; set; }
```

## TrackFactory

```
[UxmlObjectReference("track-factory")]
public GenericSliderTrackFactory TrackFactory { get; set; }
```

## ThumbFactory

```
[UxmlObjectReference("thumb-factory")]
public GenericSliderThumbFactory ThumbFactory { get; set; }
```

## TestFactory

```
[UxmlObjectReference("base-test")]
public VisualElementFactory TestFactory { get; set; }
```

## OnValueChanged

```
public event Action<float> OnValueChanged
```

## SetValueWithoutNotify(float)

```
public void SetValueWithoutNotify(float newValue)
```

## OnAttached(AttachToPanelEvent)

```
protected override void OnAttached(AttachToPanelEvent evt)
```

## Rebuild()

```
public void Rebuild()
```
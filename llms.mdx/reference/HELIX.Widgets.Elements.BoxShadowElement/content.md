# BoxShadowElement (/reference/HELIX.Widgets.Elements.BoxShadowElement)

# BoxShadowElement

```
[UxmlElement]
public class BoxShadowElement : SingleChildWidgetBaseElement<HBoxShadow>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IReconcileScheduler, IScheduledReconcileRunner, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider, ISingleChildContainer
```

## BoxShadowElement()

```
public BoxShadowElement()
```

## SpreadRadius

```
[UxmlAttribute]
public float SpreadRadius { get; set; }
```

## BlurRadius

```
[UxmlAttribute]
public float BlurRadius { get; set; }
```

## Offset

```
[UxmlAttribute]
public Vector2 Offset { get; set; }
```

## ShadowColor

```
[UxmlAttribute]
public Color ShadowColor { get; set; }
```

## TransitionDuration

```
public TimeValue TransitionDuration { get; set; }
```

## EasingFunction

```
public EasingMode EasingFunction { get; set; }
```

## BorderRadius

```
public Vector4 BorderRadius { get; set; }
```

## Corners

```
[UxmlAttribute]
public Rect Corners { get; set; }
```

## Child

```
public override VisualElement Child { get; set; }
```

## Apply(HBoxShadow, HBoxShadow)

```
public override void Apply(HBoxShadow previous, HBoxShadow widget)
```
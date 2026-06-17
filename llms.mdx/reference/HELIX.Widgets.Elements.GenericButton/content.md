# GenericButton (/reference/HELIX.Widgets.Elements.GenericButton)

# GenericButton

```
[UxmlElement]
[Obsolete("Use HButton or the ButtonControllerModifier itself directly or create a custom element")]
public class GenericButton : BuildingWidgetBaseElement<ButtonBuilder>, IEventHandler, IResolvedStyle, ITransform, ITransitionAnimations, IExperimentalFeatures, IVisualElementScheduler, IWidgetElement, BuildContext, IDiagnosticableTree, IDiagnosticable, IElement, IThemeProvider, ISingleChildContainer
```

## onClick

```
public Action onClick
```

## state

```
public WidgetState state
```

## GenericButton()

```
public GenericButton()
```

## Enabled

```
public bool Enabled { get; set; }
```

## Selected

```
public bool Selected { get; set; }
```

## HandleClick()

```
public void HandleClick()
```

## UpdateWidgetState(WidgetState)

```
public void UpdateWidgetState(WidgetState newState)
```

## Apply(ButtonBuilder, ButtonBuilder)

```
public override void Apply(ButtonBuilder previous, ButtonBuilder widget)
```

## GetBuildableForWidget(ButtonBuilder, ButtonBuilder)

```
protected override IBuildable GetBuildableForWidget(ButtonBuilder previous, ButtonBuilder widget)
```
# HSliderState (/reference/HELIX.Widgets.Universal.HSliderState)

# HSliderState

```
public class HSliderState : State<HSlider>, IDiagnosticable, IBuildable
```

## InitState()

```
public override void InitState()
```

<p>Called immediately before the first <a data-furef-uid="HELIX.Widgets.State%601.Build(HELIX.Widgets.BuildContext)">Build</a> call to initialize the state.</p>

## CanReconcile(HSlider)

```
public override bool CanReconcile(HSlider oldWidget)
```

<p>Called when the state is about to be rebuilt with a new widget configuration.</p>

## DidUpdateWidget(HSlider)

```
public override void DidUpdateWidget(HSlider oldWidget)
```

<p>Called when the state is rebuilt with a new widget configuration.</p>

## Build(BuildContext)

```
public override Widget Build(BuildContext context)
```

Builds the widget in the given context.
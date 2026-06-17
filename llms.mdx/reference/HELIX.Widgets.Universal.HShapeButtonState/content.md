# HShapeButtonState (/reference/HELIX.Widgets.Universal.HShapeButtonState)

# HShapeButtonState

```
public class HShapeButtonState : State<HButton>, IDiagnosticable, IBuildable
```

## InitState()

```
public override void InitState()
```

<p>Called immediately before the first <a data-furef-uid="HELIX.Widgets.State%601.Build(HELIX.Widgets.BuildContext)">Build</a> call to initialize the state.</p>

## CanReconcile(HButton)

```
public override bool CanReconcile(HButton oldWidget)
```

<p>Called when the state is about to be rebuilt with a new widget configuration.</p>

## DidUpdateWidget(HButton)

```
public override void DidUpdateWidget(HButton oldWidget)
```

<p>Called when the state is rebuilt with a new widget configuration.</p>

## Build(BuildContext)

```
public override Widget Build(BuildContext context)
```

Builds the widget in the given context.
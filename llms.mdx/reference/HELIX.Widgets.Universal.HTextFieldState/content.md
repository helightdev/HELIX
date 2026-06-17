# HTextFieldState (/reference/HELIX.Widgets.Universal.HTextFieldState)

# HTextFieldState

```
public class HTextFieldState : State<HTextField>, IDiagnosticable, IBuildable
```

## InitState()

```
public override void InitState()
```

<p>Called immediately before the first <a data-furef-uid="HELIX.Widgets.State%601.Build(HELIX.Widgets.BuildContext)">Build</a> call to initialize the state.</p>

## DidUpdateWidget(HTextField)

```
public override void DidUpdateWidget(HTextField oldWidget)
```

<p>Called when the state is rebuilt with a new widget configuration.</p>

## CanReconcile(HTextField)

```
public override bool CanReconcile(HTextField oldWidget)
```

<p>Called when the state is about to be rebuilt with a new widget configuration.</p>

## Build(BuildContext)

```
public override Widget Build(BuildContext context)
```

Builds the widget in the given context.
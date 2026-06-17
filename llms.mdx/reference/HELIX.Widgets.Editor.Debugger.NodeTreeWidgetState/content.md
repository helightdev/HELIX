# NodeTreeWidgetState (/reference/HELIX.Widgets.Editor.Debugger.NodeTreeWidgetState)

# NodeTreeWidgetState

```
public class NodeTreeWidgetState : State<NodeTreeWidget>, IDiagnosticable, IBuildable
```

## InitState()

```
public override void InitState()
```

<p>Called immediately before the first <a data-furef-uid="HELIX.Widgets.State%601.Build(HELIX.Widgets.BuildContext)">Build</a> call to initialize the state.</p>

## DidUpdateWidget(NodeTreeWidget)

```
public override void DidUpdateWidget(NodeTreeWidget oldWidget)
```

<p>Called when the state is rebuilt with a new widget configuration.</p>

## Build(BuildContext)

```
public override Widget Build(BuildContext context)
```

Builds the widget in the given context.
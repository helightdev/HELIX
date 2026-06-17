# StatelessWidget<T> (/reference/HELIX.Widgets.StatelessWidget-1)

# StatelessWidget<T>

```
public abstract class StatelessWidget<T> : Widget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IStatelessWidget, IBuildable where T : StatelessWidget<T>
```

## StatelessWidget(Key, object[], IReadOnlyCollection<Modifier>)

```
protected StatelessWidget(Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

## Build(BuildContext)

```
public abstract Widget Build(BuildContext context)
```

Builds the widget in the given context.

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.
# StatefulWidget<T> (/reference/HELIX.Widgets.StatefulWidget-1)

# StatefulWidget<T>

```
public abstract class StatefulWidget<T> : Widget, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IStatefulWidget where T : StatefulWidget<T>
```

<p>Base class of widgets that maintain state.</p>
<p>
The behavior of a StatefulWidget is defined by its <a data-furef-uid="HELIX.Widgets.State%601">State</a> object.
</p>

## StatefulWidget(Key, object[], IReadOnlyCollection<Modifier>)

```
protected StatefulWidget(Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

## CreateState()

```
public abstract State<T> CreateState()
```

## CreateElement()

```
public override IWidgetElement CreateElement()
```

Creates a new <a data-furef-uid="HELIX.Widgets.IWidgetElement">IWidgetElement</a> for the given widget configuration.
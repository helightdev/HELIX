# HStatefulBuilder (/reference/HELIX.Widgets.Universal.HStatefulBuilder)

# HStatefulBuilder

```
public class HStatefulBuilder : StatefulWidget<HStatefulBuilder>, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IStatefulWidget
```

## builder

```
public readonly BuildFunction<State<HStatefulBuilder>> builder
```

## HStatefulBuilder(BuildFunction<State<HStatefulBuilder>>, Key, object[], IReadOnlyCollection<Modifier>)

```
public HStatefulBuilder(BuildFunction<State<HStatefulBuilder>> builder, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates an inline wrapper around <a data-furef-uid="HELIX.Widgets.StatefulWidget%601">StatefulWidget</a>.

## CreateState()

```
public override State<HStatefulBuilder> CreateState()
```
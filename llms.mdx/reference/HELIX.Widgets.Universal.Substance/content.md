# Substance (/reference/HELIX.Widgets.Universal.Substance)

# Substance

```
public abstract class Substance : DiagnosticableBase, IDiagnosticable
```

Represents a generic state-dependent visual layer of a widget.
<br>
<p>
Makes use of a <a data-furef-uid="HELIX.Widgets.WidgetState">WidgetState</a> to determine the current semantic state of a widget and derives a
<a data-furef-uid="HELIX.Widgets.IWidgetListCandidate">IWidgetListCandidate</a> that can be materialized by consumer like the <a data-furef-uid="HELIX.Widgets.Universal.HSubstanceBox">HSubstanceBox</a>.
</p>

Substances are core components used in building widget structures. They define
the foundation or behavior of a widget and are designed to work within the
HELIX universal widget system.

## Factory

```
public static SubstanceFactory Factory { get; }
```

## Build(BuildContext, WidgetState)

```
public abstract IWidgetListCandidate Build(BuildContext context, WidgetState state)
```

## Builder(BuildFunction<WidgetState>)

```
public static BuilderSubstance Builder(BuildFunction<WidgetState> builder)
```
# HSubstanceBox (/reference/HELIX.Widgets.Universal.HSubstanceBox)

# HSubstanceBox

```
public class HSubstanceBox : StatefulWidget<HSubstanceBox>, IDiagnosticableTree, IDiagnosticable, IWidgetListCandidate, IStatefulWidget
```

A widget that materializes <a data-furef-uid="HELIX.Widgets.Universal.SubstanceLayers">SubstanceLayers</a>.

## alignment

```
public readonly WidgetStateProperty<Alignment> alignment
```

## boxKey

```
public readonly Key boxKey
```

## boxModifiers

```
public readonly WidgetStateProperty<ModifierSet> boxModifiers
```

## builder

```
public readonly BuildFunction<WidgetState> builder
```

## controller

```
public readonly WidgetStateController controller
```

## substances

```
public readonly SubstanceLayers substances
```

## HSubstanceBox(WidgetStateController, SubstanceLayers, BuildFunction<WidgetState>, Key, WidgetStateProperty<Alignment>, WidgetStateProperty<ModifierSet>, Key, object[], IReadOnlyCollection<Modifier>)

```
public HSubstanceBox(WidgetStateController controller = null, SubstanceLayers substances = default, BuildFunction<WidgetState> builder = null, Key boxKey = default, WidgetStateProperty<Alignment> alignment = null, WidgetStateProperty<ModifierSet> boxModifiers = null, Key key = default, object[] constants = null, IReadOnlyCollection<Modifier> modifiers = null)
```

Creates a widget that materializes <a data-furef-uid="HELIX.Widgets.Universal.SubstanceLayers">SubstanceLayers</a>.

## CreateState()

```
public override State<HSubstanceBox> CreateState()
```
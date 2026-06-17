# MergingModifiersWidgetState (/reference/HELIX.Widgets.MergingModifiersWidgetState)

# MergingModifiersWidgetState

```
public class MergingModifiersWidgetState : WidgetStateProperty<ModifierSet>, IDiagnosticable
```

## fallback

```
public readonly WidgetStateProperty<ModifierSet> fallback
```

## overrides

```
public readonly WidgetStateProperty<ModifierSet> overrides
```

## MergingModifiersWidgetState(WidgetStateProperty<ModifierSet>, WidgetStateProperty<ModifierSet>)

```
public MergingModifiersWidgetState(WidgetStateProperty<ModifierSet> overrides, WidgetStateProperty<ModifierSet> fallback)
```

## TryResolve(WidgetState, out ModifierSet)

```
public override bool TryResolve(WidgetState state, out ModifierSet value)
```
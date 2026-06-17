# WidgetStateProperties (/reference/HELIX.Widgets.WidgetStateProperties)

# WidgetStateProperties

```
public static class WidgetStateProperties
```

## Never<T>()

```
public static WidgetStateProperty<T> Never<T>()
```

## All<T>(T)

```
public static WidgetStateProperty<T> All<T>(T constant)
```

## Func<T>(Func<WidgetState, T>)

```
public static WidgetStateProperty<T> Func<T>(Func<WidgetState, T> resolver)
```

## Modifiers(WidgetStateProperty<ModifierSet>, WidgetStateProperty<ModifierSet>)

```
public static WidgetStateProperty<ModifierSet> Modifiers(WidgetStateProperty<ModifierSet> overrides, WidgetStateProperty<ModifierSet> fallback)
```
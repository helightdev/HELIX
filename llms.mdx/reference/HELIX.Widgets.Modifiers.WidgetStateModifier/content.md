# WidgetStateModifier (/reference/HELIX.Widgets.Modifiers.WidgetStateModifier)

# WidgetStateModifier

```
public class WidgetStateModifier : SingletonModifier, IDiagnosticable
```

## controller

```
public readonly WidgetStateController controller
```

## handleFocus

```
public bool handleFocus
```

## WidgetStateModifier(WidgetStateController, bool)

```
public WidgetStateModifier(WidgetStateController controller, bool handleFocus = true)
```

## Hook(VisualElement)

```
public override void Hook(VisualElement element)
```

## Unhook(VisualElement)

```
public override void Unhook(VisualElement element)
```

## HasChanged(Modifier)

```
public override bool HasChanged(Modifier previous)
```
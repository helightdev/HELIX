# ManipulatorModifier (/reference/HELIX.Widgets.Modifiers.ManipulatorModifier)

# ManipulatorModifier

```
public class ManipulatorModifier : SingletonModifier, IDiagnosticable
```

## manipulator

```
public IManipulator manipulator
```

## ManipulatorModifier(IManipulator)

```
public ManipulatorModifier(IManipulator manipulator)
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

## Of(IManipulator)

```
public static ManipulatorModifier Of(IManipulator manipulator)
```
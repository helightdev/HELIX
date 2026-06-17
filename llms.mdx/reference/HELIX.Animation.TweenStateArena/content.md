# TweenStateArena (/reference/HELIX.Animation.TweenStateArena)

# TweenStateArena

```
public abstract class TweenStateArena
```

Represents a base class for managing the transition of states using tweening logic.
Provides static creation methods to construct typed tweening arenas such as for colors or floats.

## ColorArena(VisualElement, Action<Color>, long, Color, EasingMode)

```
public static TweenStateArena<Color> ColorArena(VisualElement scheduler, Action<Color> update, long durationMs = 100, Color startValue = default, EasingMode easingMode = EasingMode.Linear)
```

## FloatArena(VisualElement, Action<float>, long, float, EasingMode)

```
public static TweenStateArena<float> FloatArena(VisualElement scheduler, Action<float> update, long durationMs = 100, float startValue = 0, EasingMode easingMode = EasingMode.Linear)
```
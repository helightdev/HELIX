# EasingExtensions (/reference/HELIX.Extensions.EasingExtensions)

# EasingExtensions

```
public static class EasingExtensions
```

## ToMilliseconds(TimeValue)

```
public static long ToMilliseconds(this TimeValue value)
```

## Eval(EasingMode, float)

```
public static float Eval(this EasingMode mode, float t)
```

Evaluates an easing function at normalized time t (typically 0..1),
matching how Unity UI Toolkit / web (CSS) easings behave:
- Linear, ease, ease-in, ease-out, ease-in-out use CSS cubic-bezier curves.
- The named Sine/Cubic/Circ/Elastic/Back/Bounce use the same standard formulas
you’ll find in common web easing references (e.g., easings.net style).
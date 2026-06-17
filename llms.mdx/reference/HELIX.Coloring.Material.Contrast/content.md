# Contrast (/reference/HELIX.Coloring.Material.Contrast)

# Contrast

```
public static class Contrast
```

Utility methods for calculating contrast given two colors,
or calculating a color given one color and a contrast ratio.

## RatioOfTones(double, double)

```
public static double RatioOfTones(double toneA, double toneB)
```

Returns a contrast ratio, which ranges from 1 to 21.

## Lighter(double, double)

```
public static double Lighter(double tone, double ratio)
```

Returns a tone &gt;= tone that ensures ratio.
Returns -1 if ratio cannot be achieved.

## Darker(double, double)

```
public static double Darker(double tone, double ratio)
```

Returns a tone &lt;= tone that ensures ratio.
Returns -1 if ratio cannot be achieved.

## LighterUnsafe(double, double)

```
public static double LighterUnsafe(double tone, double ratio)
```

Unsafe lighter version. Returns 100 if ratio cannot be achieved.

## DarkerUnsafe(double, double)

```
public static double DarkerUnsafe(double tone, double ratio)
```

Unsafe darker version. Returns 0 if ratio cannot be achieved.
# TemperatureCache (/reference/HELIX.Coloring.Material.TemperatureCache)

# TemperatureCache

```
public sealed class TemperatureCache
```

Design utilities using color temperature theory.
Analogous colors, complementary color, and cache to efficiently, lazily,
generate data for calculations when needed.

## TemperatureCache(Hct)

```
public TemperatureCache(Hct input)
```

## Input

```
public Hct Input { get; }
```

## Warmest

```
public Hct Warmest { get; }
```

## Coldest

```
public Hct Coldest { get; }
```

## Complement

```
public Hct Complement { get; }
```

A color that complements the input color aesthetically.

## InputRelativeTemperature

```
public double InputRelativeTemperature { get; }
```

Relative temperature of the input color.

## HctsByTemp

```
public List<Hct> HctsByTemp { get; }
```

HCTs for all hues, with the same chroma/tone as the input.
Sorted from coldest first to warmest last.

## TempsByHct

```
public Dictionary<Hct, double> TempsByHct { get; }
```

A map with keys of HCTs in HctsByTemp, values of raw temperature.

## HctsByHue

```
public List<Hct> HctsByHue { get; }
```

HCTs for all hues, with the same chroma/tone as the input.
Sorted ascending, hue 0 to 360.

## Analogous(int, int)

```
public List<Hct> Analogous(int count = 5, int divisions = 12)
```

A set of colors with differing hues, equidistant in temperature.

## RelativeTemperature(Hct)

```
public double RelativeTemperature(Hct hct)
```

Temperature relative to all colors with the same chroma and tone.
Value on a scale from 0 to 1.

## IsBetween(double, double, double)

```
public static bool IsBetween(double angle, double a, double b)
```

Determines if an angle is between two other angles, rotating clockwise.

## RawTemperature(Hct)

```
public static double RawTemperature(Hct color)
```

Value representing cool-warm factor of a color.
Values below 0 are considered cool, above 0 warm.
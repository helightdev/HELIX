# ViewingConditions (/reference/HELIX.Coloring.Material.ViewingConditions)

# ViewingConditions

```
public sealed class ViewingConditions
```

In traditional color spaces, a color can be identified solely by the
observer's measurement of the color. Color appearance models such as CAM16
also use information about the environment where the color was observed,
known as the viewing conditions.

## SRgb

```
public static readonly ViewingConditions SRgb
```

## Standard

```
public static readonly ViewingConditions Standard
```

## WhitePoint

```
public double3 WhitePoint { get; }
```

## AdaptingLuminance

```
public double AdaptingLuminance { get; }
```

## BackgroundLstar

```
public double BackgroundLstar { get; }
```

## Surround

```
public double Surround { get; }
```

## DiscountingIlluminant

```
public bool DiscountingIlluminant { get; }
```

## BackgroundYTowhitePointY

```
public double BackgroundYTowhitePointY { get; }
```

## Aw

```
public double Aw { get; }
```

## Nbb

```
public double Nbb { get; }
```

## Ncb

```
public double Ncb { get; }
```

## C

```
public double C { get; }
```

## Nc

```
public double Nc { get; }
```

## DrgbInverse

```
public double3 DrgbInverse { get; }
```

## RgbD

```
public double3 RgbD { get; }
```

## Fl

```
public double Fl { get; }
```

## FlRoot

```
public double FlRoot { get; }
```

## Z

```
public double Z { get; }
```

## Make(double3?, double, double, double, bool)

```
public static ViewingConditions Make(double3? whitePoint = null, double adaptingLuminance = -1, double backgroundLstar = 50, double surround = 2, bool discountingIlluminant = false)
```

Convenience constructor for ViewingConditions.
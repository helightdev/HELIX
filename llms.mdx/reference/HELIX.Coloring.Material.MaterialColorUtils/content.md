# MaterialColorUtils (/reference/HELIX.Coloring.Material.MaterialColorUtils)

# MaterialColorUtils

```
public static class MaterialColorUtils
```

Color science utilities.
Utility methods for color science constants and color space
conversions that aren't HCT or CAM16.

## ArgbFromRgb(int, int, int)

```
public static int ArgbFromRgb(int red, int green, int blue)
```

Converts a color from RGB components to ARGB format.

## ArgbFromLinrgb(double3)

```
public static int ArgbFromLinrgb(double3 linrgb)
```

Converts a color from linear RGB components to ARGB format.
Expects linear RGB in the same 0..100 domain as the Dart implementation.

## AlphaFromArgb(int)

```
public static int AlphaFromArgb(int argb)
```

Returns the alpha component of a color in ARGB format.

## RedFromArgb(int)

```
public static int RedFromArgb(int argb)
```

Returns the red component of a color in ARGB format.

## GreenFromArgb(int)

```
public static int GreenFromArgb(int argb)
```

Returns the green component of a color in ARGB format.

## BlueFromArgb(int)

```
public static int BlueFromArgb(int argb)
```

Returns the blue component of a color in ARGB format.

## IsOpaque(int)

```
public static bool IsOpaque(int argb)
```

Returns whether a color in ARGB format is opaque.

## ArgbFromXyz(double, double, double)

```
public static int ArgbFromXyz(double x, double y, double z)
```

Converts a color from XYZ to ARGB.

## XyzFromArgb(int)

```
public static double3 XyzFromArgb(int argb)
```

Converts a color from ARGB to XYZ.

## ArgbFromLab(double, double, double)

```
public static int ArgbFromLab(double l, double a, double b)
```

Converts a color represented in Lab color space into an ARGB integer.

## LabFromArgb(int)

```
public static double3 LabFromArgb(int argb)
```

Converts a color from ARGB representation to L*a*b* representation.

## ArgbFromLstar(double)

```
public static int ArgbFromLstar(double lstar)
```

Converts an L* value to an ARGB representation.
Returns a grayscale color with matching lightness.

## LstarFromArgb(int)

```
public static double LstarFromArgb(int argb)
```

Computes the L* value of a color in ARGB representation.

## YFromLstar(double)

```
public static double YFromLstar(double lstar)
```

Converts an L* value to a Y value.

## LstarFromY(double)

```
public static double LstarFromY(double y)
```

Converts a Y value to an L* value.

## Linearized(int)

```
public static double Linearized(int rgbComponent)
```

Linearizes an RGB component.
Input:  0..255
Output: 0..100

## Delinearized(double)

```
public static int Delinearized(double rgbComponent)
```

Delinearizes an RGB component.
Input:  0..100
Output: 0..255

## WhitePointD65()

```
public static double3 WhitePointD65()
```

Returns the standard white point; white on a sunny day.
# MaterialSchemeFactory (/reference/HELIX.Coloring.Material.MaterialSchemeFactory)

# MaterialSchemeFactory

```
public static class MaterialSchemeFactory
```

Unity-facing helpers and extensions for Material Color Utilities.
This file is intended to sit on top of the lower-level ported types:
Hct, Cam16, Blend, DynamicScheme, DynamicColor, TonalPalette,
MaterialDynamicColors, Contrast, Variant, and the scheme preset classes.

## CreateScheme(int, Variant, bool, double)

```
public static DynamicScheme CreateScheme(int sourceArgb, Variant variant = Variant.TonalSpot, bool isDark = false, double contrastLevel = 0)
```

Creates a dynamic scheme from an ARGB seed color and a preset variant.

## CreateScheme(Color, Variant, bool, double)

```
public static DynamicScheme CreateScheme(Color sourceColor, Variant variant = Variant.TonalSpot, bool isDark = false, double contrastLevel = 0)
```

Creates a dynamic scheme from a Unity gamma-space color and a preset variant.

## CreateScheme(Hct, Variant, bool, double)

```
public static DynamicScheme CreateScheme(Hct sourceColorHct, Variant variant = Variant.TonalSpot, bool isDark = false, double contrastLevel = 0)
```

Creates a dynamic scheme from an HCT seed color and a preset variant.
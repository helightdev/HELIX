# MaterialColorExtensions (/reference/HELIX.Coloring.Material.MaterialColorExtensions)

# MaterialColorExtensions

```
public static class MaterialColorExtensions
```

Extension methods for working with Unity colors and Material Color Utilities.
Unity Color values are treated as gamma-space colors.

## ArgbToColor(int)

```
public static Color ArgbToColor(this int argb)
```

Converts an ARGB integer into a Unity gamma-space Color.

## ArgbToColor(uint)

```
public static Color ArgbToColor(this uint argb)
```

Converts an ARGB integer into a Unity gamma-space Color.

## ArgbToHct(int)

```
public static Hct ArgbToHct(this int argb)
```

Converts an ARGB integer into HCT.

## ArgbToHct(uint)

```
public static Hct ArgbToHct(this uint argb)
```

Converts an ARGB integer into HCT.

## ArgbToTonalPalette(int)

```
public static TonalPalette ArgbToTonalPalette(this int argb)
```

Converts an ARGB integer into a tonal palette.

## ArgbToMaterialColor(int, string)

```
public static MaterialColor ArgbToMaterialColor(this int argb, string name = null)
```

Converts an ARGB integer into a MaterialColor wrapper.

## ToArgb(Color)

```
public static int ToArgb(this Color color)
```

Converts a Unity gamma-space Color into ARGB.

## ToHct(Color)

```
public static Hct ToHct(this Color color)
```

Converts a Unity gamma-space Color into HCT.

## ToTonalPalette(Color)

```
public static TonalPalette ToTonalPalette(this Color color)
```

Converts a Unity gamma-space Color into a tonal palette.

## ToMaterialColor(Color, string)

```
public static MaterialColor ToMaterialColor(this Color color, string name = null)
```

Converts a Unity gamma-space Color into a MaterialColor wrapper.

## ToColor(Hct)

```
public static Color ToColor(this Hct hct)
```

Converts an HCT color into a Unity gamma-space Color.

## ToStyleColor(Hct)

```
public static StyleColor ToStyleColor(this Hct hct)
```

Converts an HCT color into a UI Toolkit StyleColor.

## ToMaterialColor(Hct, string)

```
public static MaterialColor ToMaterialColor(this Hct hct, string name = null)
```

Converts an HCT color into a MaterialColor wrapper.

## ToMaterialColor(TonalPalette, string)

```
public static MaterialColor ToMaterialColor(this TonalPalette tonalPalette, string name = null)
```

Converts a tonal palette into a MaterialColor wrapper.

## ToColor(TonalPalette)

```
public static Color ToColor(this TonalPalette tonalPalette)
```

Returns the key color of a tonal palette as a Unity gamma-space Color.

## ToStyleColor(TonalPalette)

```
public static StyleColor ToStyleColor(this TonalPalette tonalPalette)
```

Returns the key color of a tonal palette as a UI Toolkit StyleColor.

## CreateScheme(int, Variant, bool, double)

```
public static DynamicScheme CreateScheme(this int sourceArgb, Variant variant = Variant.TonalSpot, bool isDark = false, double contrastLevel = 0)
```

Creates a dynamic scheme from an ARGB integer.

## CreateScheme(Color, Variant, bool, double)

```
public static DynamicScheme CreateScheme(this Color sourceColor, Variant variant = Variant.TonalSpot, bool isDark = false, double contrastLevel = 0)
```

Creates a dynamic scheme from a Unity gamma-space Color.

## CreateScheme(Hct, Variant, bool, double)

```
public static DynamicScheme CreateScheme(this Hct sourceColorHct, Variant variant = Variant.TonalSpot, bool isDark = false, double contrastLevel = 0)
```

Creates a dynamic scheme from an HCT color.

## CreateScheme(Variant, int, bool, double)

```
public static DynamicScheme CreateScheme(this Variant variant, int sourceArgb, bool isDark = false, double contrastLevel = 0)
```

Creates a dynamic scheme from a preset variant and ARGB seed.

## CreateScheme(Variant, Color, bool, double)

```
public static DynamicScheme CreateScheme(this Variant variant, Color sourceColor, bool isDark = false, double contrastLevel = 0)
```

Creates a dynamic scheme from a preset variant and Unity gamma-space Color seed.

## CreateScheme(Variant, Hct, bool, double)

```
public static DynamicScheme CreateScheme(this Variant variant, Hct sourceColorHct, bool isDark = false, double contrastLevel = 0)
```

Creates a dynamic scheme from a preset variant and HCT seed.

## GetArgb(DynamicScheme, DynamicColor)

```
public static int GetArgb(this DynamicScheme scheme, DynamicColor role)
```

Resolves a dynamic role to ARGB from a scheme.

## GetHct(DynamicScheme, DynamicColor)

```
public static Hct GetHct(this DynamicScheme scheme, DynamicColor role)
```

Resolves a dynamic role to HCT from a scheme.

## GetColor(DynamicScheme, DynamicColor)

```
public static Color GetColor(this DynamicScheme scheme, DynamicColor role)
```

Resolves a dynamic role to a Unity gamma-space Color from a scheme.

## HarmonizeArgb(int, int)

```
public static int HarmonizeArgb(this int designArgb, int sourceArgb)
```

Harmonizes one ARGB color toward another.

## Harmonize(Color, Color)

```
public static Color Harmonize(this Color designColor, Color sourceColor)
```

Harmonizes one Unity gamma-space Color toward another.

## BlendHctHueArgb(int, int, double)

```
public static int BlendHctHueArgb(this int fromArgb, int toArgb, double amount)
```

Blends hue in HCT from one ARGB color toward another.

## BlendHctHue(Color, Color, double)

```
public static Color BlendHctHue(this Color fromColor, Color toColor, double amount)
```

Blends hue in HCT from one Unity gamma-space Color toward another.

## BlendCam16UcsArgb(int, int, double)

```
public static int BlendCam16UcsArgb(this int fromArgb, int toArgb, double amount)
```

Blends in CAM16-UCS from one ARGB color toward another.

## BlendCam16Ucs(Color, Color, double)

```
public static Color BlendCam16Ucs(this Color fromColor, Color toColor, double amount)
```

Blends in CAM16-UCS from one Unity gamma-space Color toward another.

## WithTone(Hct, double)

```
public static Hct WithTone(this Hct hct, double tone)
```

Creates a new HCT color at the same hue/chroma but a different tone.

## WithHue(Hct, double)

```
public static Hct WithHue(this Hct hct, double hue)
```

Creates a new HCT color at the same chroma/tone but a different hue.

## WithChroma(Hct, double)

```
public static Hct WithChroma(this Hct hct, double chroma)
```

Creates a new HCT color at the same hue/tone but a different chroma.

## WithTone(Color, double)

```
public static Color WithTone(this Color color, double tone)
```

Creates a new Unity gamma-space Color by changing tone in HCT.

## WithHue(Color, double)

```
public static Color WithHue(this Color color, double hue)
```

Creates a new Unity gamma-space Color by changing hue in HCT.

## WithChroma(Color, double)

```
public static Color WithChroma(this Color color, double chroma)
```

Creates a new Unity gamma-space Color by changing chroma in HCT.

## ToIntArgbHex(int)

```
public static string ToIntArgbHex(this int argb)
```

Converts an ARGB integer to a #AARRGGBB string.
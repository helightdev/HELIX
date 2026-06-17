# DynamicScheme (/reference/HELIX.Coloring.Material.DynamicScheme)

# DynamicScheme

```
public class DynamicScheme
```

Constructed by a set of values representing the current UI state and
provides a set of TonalPalettes.

## DynamicScheme(Hct, Variant, bool, TonalPalette, TonalPalette, TonalPalette, TonalPalette, TonalPalette, double, TonalPalette)

```
public DynamicScheme(Hct sourceColorHct, Variant variant, bool isDark, TonalPalette primaryPalette, TonalPalette secondaryPalette, TonalPalette tertiaryPalette, TonalPalette neutralPalette, TonalPalette neutralVariantPalette, double contrastLevel = 0, TonalPalette errorPalette = null)
```

## SourceColorArgb

```
public int SourceColorArgb { get; }
```

## SourceColorHct

```
public Hct SourceColorHct { get; }
```

## Variant

```
public Variant Variant { get; }
```

## IsDark

```
public bool IsDark { get; }
```

## ContrastLevel

```
public double ContrastLevel { get; }
```

## PrimaryPalette

```
public TonalPalette PrimaryPalette { get; }
```

## SecondaryPalette

```
public TonalPalette SecondaryPalette { get; }
```

## TertiaryPalette

```
public TonalPalette TertiaryPalette { get; }
```

## NeutralPalette

```
public TonalPalette NeutralPalette { get; }
```

## NeutralVariantPalette

```
public TonalPalette NeutralVariantPalette { get; }
```

## ErrorPalette

```
public TonalPalette ErrorPalette { get; }
```

## PrimaryPaletteKeyColor

```
public int PrimaryPaletteKeyColor { get; }
```

## SecondaryPaletteKeyColor

```
public int SecondaryPaletteKeyColor { get; }
```

## TertiaryPaletteKeyColor

```
public int TertiaryPaletteKeyColor { get; }
```

## NeutralPaletteKeyColor

```
public int NeutralPaletteKeyColor { get; }
```

## NeutralVariantPaletteKeyColor

```
public int NeutralVariantPaletteKeyColor { get; }
```

## Background

```
public int Background { get; }
```

## OnBackground

```
public int OnBackground { get; }
```

## Surface

```
public int Surface { get; }
```

## SurfaceDim

```
public int SurfaceDim { get; }
```

## SurfaceBright

```
public int SurfaceBright { get; }
```

## SurfaceContainerLowest

```
public int SurfaceContainerLowest { get; }
```

## SurfaceContainerLow

```
public int SurfaceContainerLow { get; }
```

## SurfaceContainer

```
public int SurfaceContainer { get; }
```

## SurfaceContainerHigh

```
public int SurfaceContainerHigh { get; }
```

## SurfaceContainerHighest

```
public int SurfaceContainerHighest { get; }
```

## OnSurface

```
public int OnSurface { get; }
```

## SurfaceVariant

```
public int SurfaceVariant { get; }
```

## OnSurfaceVariant

```
public int OnSurfaceVariant { get; }
```

## InverseSurface

```
public int InverseSurface { get; }
```

## InverseOnSurface

```
public int InverseOnSurface { get; }
```

## Outline

```
public int Outline { get; }
```

## OutlineVariant

```
public int OutlineVariant { get; }
```

## Shadow

```
public int Shadow { get; }
```

## Scrim

```
public int Scrim { get; }
```

## SurfaceTint

```
public int SurfaceTint { get; }
```

## Primary

```
public int Primary { get; }
```

## OnPrimary

```
public int OnPrimary { get; }
```

## PrimaryContainer

```
public int PrimaryContainer { get; }
```

## OnPrimaryContainer

```
public int OnPrimaryContainer { get; }
```

## InversePrimary

```
public int InversePrimary { get; }
```

## Secondary

```
public int Secondary { get; }
```

## OnSecondary

```
public int OnSecondary { get; }
```

## SecondaryContainer

```
public int SecondaryContainer { get; }
```

## OnSecondaryContainer

```
public int OnSecondaryContainer { get; }
```

## Tertiary

```
public int Tertiary { get; }
```

## OnTertiary

```
public int OnTertiary { get; }
```

## TertiaryContainer

```
public int TertiaryContainer { get; }
```

## OnTertiaryContainer

```
public int OnTertiaryContainer { get; }
```

## Error

```
public int Error { get; }
```

## OnError

```
public int OnError { get; }
```

## ErrorContainer

```
public int ErrorContainer { get; }
```

## OnErrorContainer

```
public int OnErrorContainer { get; }
```

## PrimaryFixed

```
public int PrimaryFixed { get; }
```

## PrimaryFixedDim

```
public int PrimaryFixedDim { get; }
```

## OnPrimaryFixed

```
public int OnPrimaryFixed { get; }
```

## OnPrimaryFixedVariant

```
public int OnPrimaryFixedVariant { get; }
```

## SecondaryFixed

```
public int SecondaryFixed { get; }
```

## SecondaryFixedDim

```
public int SecondaryFixedDim { get; }
```

## OnSecondaryFixed

```
public int OnSecondaryFixed { get; }
```

## OnSecondaryFixedVariant

```
public int OnSecondaryFixedVariant { get; }
```

## TertiaryFixed

```
public int TertiaryFixed { get; }
```

## TertiaryFixedDim

```
public int TertiaryFixedDim { get; }
```

## OnTertiaryFixed

```
public int OnTertiaryFixed { get; }
```

## OnTertiaryFixedVariant

```
public int OnTertiaryFixedVariant { get; }
```

## GetRotatedHue(Hct, IReadOnlyList<double>, IReadOnlyList<double>)

```
public static double GetRotatedHue(Hct sourceColor, IReadOnlyList<double> hues, IReadOnlyList<double> rotations)
```

## GetHct(DynamicColor)

```
public Hct GetHct(DynamicColor dynamicColor)
```

## GetArgb(DynamicColor)

```
public int GetArgb(DynamicColor dynamicColor)
```
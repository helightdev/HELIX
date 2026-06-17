# DynamicColor (/reference/HELIX.Coloring.Material.DynamicColor)

# DynamicColor

```
public sealed class DynamicColor
```

A color that adjusts itself based on UI state provided by DynamicScheme.

## DynamicColor(string, Func<DynamicScheme, TonalPalette>, Func<DynamicScheme, double>, bool, Func<DynamicScheme, DynamicColor>, Func<DynamicScheme, DynamicColor>, ContrastCurve, Func<DynamicScheme, ToneDeltaPair>)

```
public DynamicColor(string name, Func<DynamicScheme, TonalPalette> palette, Func<DynamicScheme, double> tone, bool isBackground, Func<DynamicScheme, DynamicColor> background, Func<DynamicScheme, DynamicColor> secondBackground, ContrastCurve contrastCurve, Func<DynamicScheme, ToneDeltaPair> toneDeltaPair)
```

## Name

```
public string Name { get; }
```

## Palette

```
public Func<DynamicScheme, TonalPalette> Palette { get; }
```

## Tone

```
public Func<DynamicScheme, double> Tone { get; }
```

## IsBackground

```
public bool IsBackground { get; }
```

## Background

```
public Func<DynamicScheme, DynamicColor> Background { get; }
```

## SecondBackground

```
public Func<DynamicScheme, DynamicColor> SecondBackground { get; }
```

## ContrastCurve

```
public ContrastCurve ContrastCurve { get; }
```

## ToneDeltaPair

```
public Func<DynamicScheme, ToneDeltaPair> ToneDeltaPair { get; }
```

## FromPalette(Func<DynamicScheme, TonalPalette>, Func<DynamicScheme, double>, string, bool, Func<DynamicScheme, DynamicColor>, Func<DynamicScheme, DynamicColor>, ContrastCurve, Func<DynamicScheme, ToneDeltaPair>)

```
public static DynamicColor FromPalette(Func<DynamicScheme, TonalPalette> palette, Func<DynamicScheme, double> tone, string name = "", bool isBackground = false, Func<DynamicScheme, DynamicColor> background = null, Func<DynamicScheme, DynamicColor> secondBackground = null, ContrastCurve contrastCurve = null, Func<DynamicScheme, ToneDeltaPair> toneDeltaPair = null)
```

## GetArgb(DynamicScheme)

```
public int GetArgb(DynamicScheme scheme)
```

## GetHct(DynamicScheme)

```
public Hct GetHct(DynamicScheme scheme)
```

## GetTone(DynamicScheme)

```
public double GetTone(DynamicScheme scheme)
```

## ForegroundTone(double, double)

```
public static double ForegroundTone(double bgTone, double ratio)
```

## EnableLightForeground(double)

```
public static double EnableLightForeground(double tone)
```

## TonePrefersLightForeground(double)

```
public static bool TonePrefersLightForeground(double tone)
```

## ToneAllowsLightForeground(double)

```
public static bool ToneAllowsLightForeground(double tone)
```
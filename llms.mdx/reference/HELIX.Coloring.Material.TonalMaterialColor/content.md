# TonalMaterialColor (/reference/HELIX.Coloring.Material.TonalMaterialColor)

# TonalMaterialColor

```
public sealed class TonalMaterialColor : MaterialColor
```

Generated swatch backed by a tonal palette.
This replaces the old MaterialColor wrapper behavior.

## TonalMaterialColor(int, string)

```
public TonalMaterialColor(int argb, string name = null)
```

## TonalMaterialColor(uint, string)

```
public TonalMaterialColor(uint argb, string name = null)
```

## TonalMaterialColor(Hct, string)

```
public TonalMaterialColor(Hct hct, string name = null)
```

## TonalMaterialColor(TonalPalette, string)

```
public TonalMaterialColor(TonalPalette tonalPalette, string name = null)
```

## Palette

```
public TonalPalette Palette { get; }
```

## Seed

```
public Hct Seed { get; }
```

## this[int]

```
public override Color this[int weight] { get; }
```

Resolves a shade by weight.

## ToToneSwatch(params double[])

```
public Color[] ToToneSwatch(params double[] tones)
```

Returns a swatch for arbitrary HCT tone values.

## Hct(TonalMaterialColor)

```
public static implicit operator Hct(TonalMaterialColor materialColor)
```

## TonalPalette(TonalMaterialColor)

```
public static implicit operator TonalPalette(TonalMaterialColor materialColor)
```
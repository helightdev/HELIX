# TonalMaterialAccentColor (/reference/HELIX.Coloring.Material.TonalMaterialAccentColor)

# TonalMaterialAccentColor

```
public sealed class TonalMaterialAccentColor : MaterialAccentColor
```

Generated accent swatch backed by a tonal palette.
Useful if you want accent-style indexing but still generated from HCT.

## TonalMaterialAccentColor(int, string)

```
public TonalMaterialAccentColor(int argb, string name = null)
```

## TonalMaterialAccentColor(uint, string)

```
public TonalMaterialAccentColor(uint argb, string name = null)
```

## TonalMaterialAccentColor(Hct, string)

```
public TonalMaterialAccentColor(Hct hct, string name = null)
```

## TonalMaterialAccentColor(TonalPalette, string)

```
public TonalMaterialAccentColor(TonalPalette tonalPalette, string name = null)
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

## Hct(TonalMaterialAccentColor)

```
public static implicit operator Hct(TonalMaterialAccentColor materialColor)
```

## TonalPalette(TonalMaterialAccentColor)

```
public static implicit operator TonalPalette(TonalMaterialAccentColor materialColor)
```
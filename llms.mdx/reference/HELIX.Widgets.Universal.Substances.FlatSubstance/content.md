# FlatSubstance (/reference/HELIX.Widgets.Universal.Substances.FlatSubstance)

# FlatSubstance

```
public static class FlatSubstance
```

## Flat(ColorTokenPalette, ColorTokenPalette, SurfaceColorPalette, BorderRadius, LayerOpacityProgression, bool)

```
public static BoxSubstance Flat(ColorTokenPalette inactive, ColorTokenPalette palette, SurfaceColorPalette surface, BorderRadius borderRadius, LayerOpacityProgression progression, bool withSelected = true)
```

## Flat<TBuilder>(ISubstanceBuilder<TBuilder>, ColorTokenPalette, ColorTokenPalette, SurfaceColorPalette, BorderRadius?, HInputRadius, bool)

```
public static BuilderAndSubstance<TBuilder, BoxSubstance> Flat<TBuilder>(this ISubstanceBuilder<TBuilder> builder, ColorTokenPalette inactive = null, ColorTokenPalette palette = null, SurfaceColorPalette surface = null, BorderRadius? borderRadius = null, HInputRadius inputRadius = HInputRadius.Medium, bool withSelected = true) where TBuilder : ISubstanceBuilder<TBuilder>
```
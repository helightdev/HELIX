# FlatSubstance (/reference/HELIX.Widgets.Universal.Substances.FlatSubstance)

# FlatSubstance

```
public static class FlatSubstance
```

## Flat(ColorTokenPalette, ColorTokenPalette, SurfaceColorPalette, BorderRadius, LayerOpacityProgression)

```
public static BoxSubstance Flat(ColorTokenPalette inactive, ColorTokenPalette palette, SurfaceColorPalette surface, BorderRadius borderRadius, LayerOpacityProgression progression)
```

## Flat<TBuilder>(ISubstanceBuilder<TBuilder>, ColorTokenPalette, ColorTokenPalette, SurfaceColorPalette, BorderRadius?, HInputRadius)

```
public static BuilderAndSubstance<TBuilder, BoxSubstance> Flat<TBuilder>(this ISubstanceBuilder<TBuilder> builder, ColorTokenPalette inactive = null, ColorTokenPalette palette = null, SurfaceColorPalette surface = null, BorderRadius? borderRadius = null, HInputRadius inputRadius = HInputRadius.Medium) where TBuilder : ISubstanceBuilder<TBuilder>
```
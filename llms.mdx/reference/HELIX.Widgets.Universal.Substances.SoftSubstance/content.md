# SoftSubstance (/reference/HELIX.Widgets.Universal.Substances.SoftSubstance)

# SoftSubstance

```
public static class SoftSubstance
```

## Soft(ColorTokenPalette, SurfaceColorPalette, BorderRadius, LayerOpacityProgression)

```
public static BoxSubstance Soft(ColorTokenPalette palette, SurfaceColorPalette surface, BorderRadius borderRadius, LayerOpacityProgression progression)
```

## Soft<TBuilder>(ISubstanceBuilder<TBuilder>, ColorTokenPalette, SurfaceColorPalette, BorderRadius?, HInputRadius)

```
public static BuilderAndSubstance<TBuilder, BoxSubstance> Soft<TBuilder>(this ISubstanceBuilder<TBuilder> builder, ColorTokenPalette palette = null, SurfaceColorPalette surface = null, BorderRadius? borderRadius = null, HInputRadius inputRadius = HInputRadius.Medium) where TBuilder : ISubstanceBuilder<TBuilder>
```
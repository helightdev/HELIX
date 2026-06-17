# OutlineSubstance (/reference/HELIX.Widgets.Universal.Substances.OutlineSubstance)

# OutlineSubstance

```
public static class OutlineSubstance
```

## Outline(ColorTokenPalette, SurfaceColorPalette, BorderRadius, LayerOpacityProgression, ColorTokenPalette, bool)

```
public static BoxSubstance Outline(ColorTokenPalette palette, SurfaceColorPalette surface, BorderRadius borderRadius, LayerOpacityProgression progression, ColorTokenPalette error, bool contrast = false)
```

## Outline<TBuilder>(ISubstanceBuilder<TBuilder>, ColorTokenPalette, SurfaceColorPalette, BorderRadius?, HInputRadius, bool)

```
public static BuilderAndSubstance<TBuilder, BoxSubstance> Outline<TBuilder>(this ISubstanceBuilder<TBuilder> builder, ColorTokenPalette palette = null, SurfaceColorPalette surface = null, BorderRadius? borderRadius = null, HInputRadius inputRadius = HInputRadius.Medium, bool contrast = false) where TBuilder : ISubstanceBuilder<TBuilder>
```
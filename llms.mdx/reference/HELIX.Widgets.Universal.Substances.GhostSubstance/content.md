# GhostSubstance (/reference/HELIX.Widgets.Universal.Substances.GhostSubstance)

# GhostSubstance

```
public static class GhostSubstance
```

## Ghost(ColorTokenPalette, SurfaceColorPalette, BorderRadius, LayerOpacityProgression, bool)

```
public static BoxSubstance Ghost(ColorTokenPalette palette, SurfaceColorPalette surface, BorderRadius borderRadius, LayerOpacityProgression progression, bool contrast = false)
```

## Ghost<TBuilder>(ISubstanceBuilder<TBuilder>, ColorTokenPalette, SurfaceColorPalette, BorderRadius?, HInputRadius, bool)

```
public static BuilderAndSubstance<TBuilder, BoxSubstance> Ghost<TBuilder>(this ISubstanceBuilder<TBuilder> builder, ColorTokenPalette palette = null, SurfaceColorPalette surface = null, BorderRadius? borderRadius = null, HInputRadius inputRadius = HInputRadius.Medium, bool contrast = false) where TBuilder : ISubstanceBuilder<TBuilder>
```
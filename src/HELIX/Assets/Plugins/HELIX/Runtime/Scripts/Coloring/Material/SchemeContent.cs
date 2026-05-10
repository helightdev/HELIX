using Unity.Mathematics;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   A scheme that places the source color in PrimaryContainer.
    ///   Primary Container is the source color, adjusted for color relativity.
    ///   Tertiary Container is an analogous color, specifically the analog found
    ///   by increasing hue on a 6-division wheel.
    /// </summary>
    public sealed class SchemeContent : DynamicScheme {
    public SchemeContent(
      Hct sourceColorHct,
      bool isDark,
      double contrastLevel = 0.0
    )
      : base(
        sourceColorHct,
        Variant.Content,
        isDark,
        contrastLevel: contrastLevel,
        primaryPalette: TonalPalette.Of(
          sourceColorHct.Hue,
          sourceColorHct.Chroma
        ),
        secondaryPalette: TonalPalette.Of(
          sourceColorHct.Hue,
          math.max(sourceColorHct.Chroma - 32.0, sourceColorHct.Chroma * 0.5)
        ),
        tertiaryPalette: TonalPalette.FromHct(
          DislikeAnalyzer.FixIfDisliked(new TemperatureCache(sourceColorHct).Analogous(3, 6)[2])
        ),
        neutralPalette: TonalPalette.Of(
          sourceColorHct.Hue,
          sourceColorHct.Chroma / 8.0
        ),
        neutralVariantPalette: TonalPalette.Of(
          sourceColorHct.Hue,
          sourceColorHct.Chroma / 8.0 + 4.0
        )
      ) { }
  }
}
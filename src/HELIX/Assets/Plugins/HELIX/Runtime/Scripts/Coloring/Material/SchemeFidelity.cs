using Unity.Mathematics;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   A scheme that places the source color in PrimaryContainer.
    ///   Tertiary Container is the complement to the source color.
    /// </summary>
    public sealed class SchemeFidelity : DynamicScheme {
    public SchemeFidelity(
      Hct sourceColorHct,
      bool isDark,
      double contrastLevel = 0.0
    )
      : base(
        sourceColorHct,
        Variant.Fidelity,
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
          DislikeAnalyzer.FixIfDisliked(new TemperatureCache(sourceColorHct).Complement)
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
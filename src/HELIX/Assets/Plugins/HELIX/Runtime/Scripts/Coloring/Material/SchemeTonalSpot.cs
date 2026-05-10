namespace HELIX.Coloring.Material {
    /// <summary>
    ///   A Dynamic Color theme with low to medium colorfulness and a tertiary
    ///   palette with a hue related to the source color.
    /// </summary>
    public sealed class SchemeTonalSpot : DynamicScheme {
    public SchemeTonalSpot(
      Hct sourceColorHct,
      bool isDark,
      double contrastLevel = 0.0
    )
      : base(
        sourceColorHct,
        Variant.TonalSpot,
        isDark,
        contrastLevel: contrastLevel,
        primaryPalette: TonalPalette.Of(sourceColorHct.Hue, 36.0),
        secondaryPalette: TonalPalette.Of(sourceColorHct.Hue, 16.0),
        tertiaryPalette: TonalPalette.Of(
          MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue + 60.0),
          24.0
        ),
        neutralPalette: TonalPalette.Of(sourceColorHct.Hue, 6.0),
        neutralVariantPalette: TonalPalette.Of(sourceColorHct.Hue, 8.0)
      ) { }
  }
}
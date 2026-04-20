namespace HELIX.Coloring.Material {
    /// <summary>
    ///   A playful theme - the source color's hue does not appear in the theme.
    /// </summary>
    public sealed class SchemeFruitSalad : DynamicScheme {
    public SchemeFruitSalad(
      Hct sourceColorHct,
      bool isDark,
      double contrastLevel = 0.0
    )
      : base(
        sourceColorHct,
        Variant.FruitSalad,
        isDark,
        contrastLevel: contrastLevel,
        primaryPalette: TonalPalette.Of(
          MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue - 50.0),
          48.0
        ),
        secondaryPalette: TonalPalette.Of(
          MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue - 50.0),
          36.0
        ),
        tertiaryPalette: TonalPalette.Of(sourceColorHct.Hue, 36.0),
        neutralPalette: TonalPalette.Of(sourceColorHct.Hue, 10.0),
        neutralVariantPalette: TonalPalette.Of(sourceColorHct.Hue, 16.0)
      ) { }
  }
}
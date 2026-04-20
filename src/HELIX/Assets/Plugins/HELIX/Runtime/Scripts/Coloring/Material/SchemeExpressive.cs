namespace HELIX.Coloring.Material {
    /// <summary>
    ///   A Dynamic Color theme that is intentionally detached from the input color.
    /// </summary>
    public sealed class SchemeExpressive : DynamicScheme {
    private static readonly double[] _hues = { 0, 21, 51, 121, 151, 191, 271, 321, 360 };

    private static readonly double[] _secondaryRotations = { 45, 95, 45, 20, 45, 90, 45, 45, 45 };

    private static readonly double[] _tertiaryRotations = { 120, 120, 20, 45, 20, 15, 20, 120, 120 };

    public SchemeExpressive(
      Hct sourceColorHct,
      bool isDark,
      double contrastLevel = 0.0
    )
      : base(
        sourceColorHct,
        Variant.Expressive,
        isDark,
        contrastLevel: contrastLevel,
        primaryPalette: TonalPalette.Of(
          MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue + 240.0),
          40.0
        ),
        secondaryPalette: TonalPalette.Of(
          GetRotatedHue(sourceColorHct, _hues, _secondaryRotations),
          24.0
        ),
        tertiaryPalette: TonalPalette.Of(
          GetRotatedHue(sourceColorHct, _hues, _tertiaryRotations),
          32.0
        ),
        neutralPalette: TonalPalette.Of(sourceColorHct.Hue + 15.0, 8.0),
        neutralVariantPalette: TonalPalette.Of(sourceColorHct.Hue + 15.0, 12.0)
      ) { }
  }
}
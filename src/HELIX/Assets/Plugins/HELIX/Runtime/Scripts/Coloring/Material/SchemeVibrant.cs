namespace HELIX.Coloring.Material {
    /// <summary>
    ///   A Dynamic Color theme that maxes out colorfulness at each position in the
    ///   primary tonal palette.
    /// </summary>
    public sealed class SchemeVibrant : DynamicScheme {
    private static readonly double[] _hues = { 0, 41, 61, 101, 131, 181, 251, 301, 360 };

    private static readonly double[] _secondaryRotations = { 18, 15, 10, 12, 15, 18, 15, 12, 12 };

    private static readonly double[] _tertiaryRotations = { 35, 30, 20, 25, 30, 35, 30, 25, 25 };

    public SchemeVibrant(
      Hct sourceColorHct,
      bool isDark,
      double contrastLevel = 0.0
    )
      : base(
        sourceColorHct,
        Variant.Vibrant,
        isDark,
        contrastLevel: contrastLevel,
        primaryPalette: TonalPalette.Of(sourceColorHct.Hue, 200.0),
        secondaryPalette: TonalPalette.Of(
          GetRotatedHue(sourceColorHct, _hues, _secondaryRotations),
          24.0
        ),
        tertiaryPalette: TonalPalette.Of(
          GetRotatedHue(sourceColorHct, _hues, _tertiaryRotations),
          32.0
        ),
        neutralPalette: TonalPalette.Of(sourceColorHct.Hue, 10.0),
        neutralVariantPalette: TonalPalette.Of(sourceColorHct.Hue, 12.0)
      ) { }
  }
}
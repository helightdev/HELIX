namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     A playful theme - the source color's hue does not appear in the theme.
    /// </summary>
    public sealed class SchemeRainbow : DynamicScheme {
        public SchemeRainbow(
            Hct sourceColorHct,
            bool isDark,
            double contrastLevel = 0.0
        )
            : base(
                sourceColorHct,
                Variant.Rainbow,
                isDark,
                contrastLevel: contrastLevel,
                primaryPalette: TonalPalette.Of(sourceColorHct.Hue, 48.0),
                secondaryPalette: TonalPalette.Of(sourceColorHct.Hue, 16.0),
                tertiaryPalette: TonalPalette.Of(
                    MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue + 60.0),
                    24.0
                ),
                neutralPalette: TonalPalette.Of(sourceColorHct.Hue, 0.0),
                neutralVariantPalette: TonalPalette.Of(sourceColorHct.Hue, 0.0)
            ) { }
    }
}
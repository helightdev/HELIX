namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     A Dynamic Color theme that is grayscale.
    /// </summary>
    public sealed class SchemeMonochrome : DynamicScheme {
        public SchemeMonochrome(
            Hct sourceColorHct,
            bool isDark,
            double contrastLevel = 0.0
        )
            : base(
                sourceColorHct,
                Variant.Monochrome,
                isDark,
                contrastLevel: contrastLevel,
                primaryPalette: TonalPalette.Of(sourceColorHct.Hue, 0.0),
                secondaryPalette: TonalPalette.Of(sourceColorHct.Hue, 0.0),
                tertiaryPalette: TonalPalette.Of(sourceColorHct.Hue, 0.0),
                neutralPalette: TonalPalette.Of(sourceColorHct.Hue, 0.0),
                neutralVariantPalette: TonalPalette.Of(sourceColorHct.Hue, 0.0)
            ) { }
    }
}
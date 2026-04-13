namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     A Dynamic Color theme that is near grayscale.
    /// </summary>
    public sealed class SchemeNeutral : DynamicScheme {
        public SchemeNeutral(
            Hct sourceColorHct,
            bool isDark,
            double contrastLevel = 0.0
        )
            : base(
                sourceColorHct,
                Variant.Neutral,
                isDark,
                contrastLevel: contrastLevel,
                primaryPalette: TonalPalette.Of(sourceColorHct.Hue, 12.0),
                secondaryPalette: TonalPalette.Of(sourceColorHct.Hue, 8.0),
                tertiaryPalette: TonalPalette.Of(sourceColorHct.Hue, 16.0),
                neutralPalette: TonalPalette.Of(sourceColorHct.Hue, 2.0),
                neutralVariantPalette: TonalPalette.Of(sourceColorHct.Hue, 2.0)
            ) { }
    }
}
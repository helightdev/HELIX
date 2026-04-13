using Unity.Mathematics;

namespace MaterialColorUtilities {
    /// <summary>
    /// A scheme that places the source color in PrimaryContainer.
    ///
    /// Primary Container is the source color, adjusted for color relativity.
    /// Tertiary Container is an analogous color, specifically the analog found
    /// by increasing hue on a 6-division wheel.
    /// </summary>
    public sealed class SchemeContent : DynamicScheme {
        public SchemeContent(
            Hct sourceColorHct,
            bool isDark,
            double contrastLevel = 0.0
        )
            : base(
                sourceColorHct: sourceColorHct,
                variant: Variant.Content,
                isDark: isDark,
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
                    DislikeAnalyzer.FixIfDisliked(
                        new TemperatureCache(sourceColorHct).Analogous(count: 3, divisions: 6)[2]
                    )
                ),
                neutralPalette: TonalPalette.Of(
                    sourceColorHct.Hue,
                    sourceColorHct.Chroma / 8.0
                ),
                neutralVariantPalette: TonalPalette.Of(
                    sourceColorHct.Hue,
                    (sourceColorHct.Chroma / 8.0) + 4.0
                )
            ) { }
    }

    /// <summary>
    /// A Dynamic Color theme that is intentionally detached from the input color.
    /// </summary>
    public sealed class SchemeExpressive : DynamicScheme {
        private static readonly double[] Hues = { 0, 21, 51, 121, 151, 191, 271, 321, 360 };

        private static readonly double[] SecondaryRotations = { 45, 95, 45, 20, 45, 90, 45, 45, 45 };

        private static readonly double[] TertiaryRotations = { 120, 120, 20, 45, 20, 15, 20, 120, 120 };

        public SchemeExpressive(
            Hct sourceColorHct,
            bool isDark,
            double contrastLevel = 0.0
        )
            : base(
                sourceColorHct: sourceColorHct,
                variant: Variant.Expressive,
                isDark: isDark,
                contrastLevel: contrastLevel,
                primaryPalette: TonalPalette.Of(
                    MathUtils.SanitizeDegreesDouble(sourceColorHct.Hue + 240.0),
                    40.0
                ),
                secondaryPalette: TonalPalette.Of(
                    DynamicScheme.GetRotatedHue(sourceColorHct, Hues, SecondaryRotations),
                    24.0
                ),
                tertiaryPalette: TonalPalette.Of(
                    DynamicScheme.GetRotatedHue(sourceColorHct, Hues, TertiaryRotations),
                    32.0
                ),
                neutralPalette: TonalPalette.Of(sourceColorHct.Hue + 15.0, 8.0),
                neutralVariantPalette: TonalPalette.Of(sourceColorHct.Hue + 15.0, 12.0)
            ) { }
    }

    /// <summary>
    /// A scheme that places the source color in PrimaryContainer.
    ///
    /// Tertiary Container is the complement to the source color.
    /// </summary>
    public sealed class SchemeFidelity : DynamicScheme {
        public SchemeFidelity(
            Hct sourceColorHct,
            bool isDark,
            double contrastLevel = 0.0
        )
            : base(
                sourceColorHct: sourceColorHct,
                variant: Variant.Fidelity,
                isDark: isDark,
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
                    (sourceColorHct.Chroma / 8.0) + 4.0
                )
            ) { }
    }

    /// <summary>
    /// A playful theme - the source color's hue does not appear in the theme.
    /// </summary>
    public sealed class SchemeFruitSalad : DynamicScheme {
        public SchemeFruitSalad(
            Hct sourceColorHct,
            bool isDark,
            double contrastLevel = 0.0
        )
            : base(
                sourceColorHct: sourceColorHct,
                variant: Variant.FruitSalad,
                isDark: isDark,
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

    /// <summary>
    /// A Dynamic Color theme that is grayscale.
    /// </summary>
    public sealed class SchemeMonochrome : DynamicScheme {
        public SchemeMonochrome(
            Hct sourceColorHct,
            bool isDark,
            double contrastLevel = 0.0
        )
            : base(
                sourceColorHct: sourceColorHct,
                variant: Variant.Monochrome,
                isDark: isDark,
                contrastLevel: contrastLevel,
                primaryPalette: TonalPalette.Of(sourceColorHct.Hue, 0.0),
                secondaryPalette: TonalPalette.Of(sourceColorHct.Hue, 0.0),
                tertiaryPalette: TonalPalette.Of(sourceColorHct.Hue, 0.0),
                neutralPalette: TonalPalette.Of(sourceColorHct.Hue, 0.0),
                neutralVariantPalette: TonalPalette.Of(sourceColorHct.Hue, 0.0)
            ) { }
    }

    /// <summary>
    /// A Dynamic Color theme that is near grayscale.
    /// </summary>
    public sealed class SchemeNeutral : DynamicScheme {
        public SchemeNeutral(
            Hct sourceColorHct,
            bool isDark,
            double contrastLevel = 0.0
        )
            : base(
                sourceColorHct: sourceColorHct,
                variant: Variant.Neutral,
                isDark: isDark,
                contrastLevel: contrastLevel,
                primaryPalette: TonalPalette.Of(sourceColorHct.Hue, 12.0),
                secondaryPalette: TonalPalette.Of(sourceColorHct.Hue, 8.0),
                tertiaryPalette: TonalPalette.Of(sourceColorHct.Hue, 16.0),
                neutralPalette: TonalPalette.Of(sourceColorHct.Hue, 2.0),
                neutralVariantPalette: TonalPalette.Of(sourceColorHct.Hue, 2.0)
            ) { }
    }

    /// <summary>
    /// A playful theme - the source color's hue does not appear in the theme.
    /// </summary>
    public sealed class SchemeRainbow : DynamicScheme {
        public SchemeRainbow(
            Hct sourceColorHct,
            bool isDark,
            double contrastLevel = 0.0
        )
            : base(
                sourceColorHct: sourceColorHct,
                variant: Variant.Rainbow,
                isDark: isDark,
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

    /// <summary>
    /// A Dynamic Color theme with low to medium colorfulness and a tertiary
    /// palette with a hue related to the source color.
    /// </summary>
    public sealed class SchemeTonalSpot : DynamicScheme {
        public SchemeTonalSpot(
            Hct sourceColorHct,
            bool isDark,
            double contrastLevel = 0.0
        )
            : base(
                sourceColorHct: sourceColorHct,
                variant: Variant.TonalSpot,
                isDark: isDark,
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

    /// <summary>
    /// A Dynamic Color theme that maxes out colorfulness at each position in the
    /// primary tonal palette.
    /// </summary>
    public sealed class SchemeVibrant : DynamicScheme {
        private static readonly double[] Hues = { 0, 41, 61, 101, 131, 181, 251, 301, 360 };

        private static readonly double[] SecondaryRotations = { 18, 15, 10, 12, 15, 18, 15, 12, 12 };

        private static readonly double[] TertiaryRotations = { 35, 30, 20, 25, 30, 35, 30, 25, 25 };

        public SchemeVibrant(
            Hct sourceColorHct,
            bool isDark,
            double contrastLevel = 0.0
        )
            : base(
                sourceColorHct: sourceColorHct,
                variant: Variant.Vibrant,
                isDark: isDark,
                contrastLevel: contrastLevel,
                primaryPalette: TonalPalette.Of(sourceColorHct.Hue, 200.0),
                secondaryPalette: TonalPalette.Of(
                    DynamicScheme.GetRotatedHue(sourceColorHct, Hues, SecondaryRotations),
                    24.0
                ),
                tertiaryPalette: TonalPalette.Of(
                    DynamicScheme.GetRotatedHue(sourceColorHct, Hues, TertiaryRotations),
                    32.0
                ),
                neutralPalette: TonalPalette.Of(sourceColorHct.Hue, 10.0),
                neutralVariantPalette: TonalPalette.Of(sourceColorHct.Hue, 12.0)
            ) { }
    }
}
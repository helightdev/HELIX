using Unity.Mathematics;

namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     Tokens, or named colors, in the Material Design system.
    /// </summary>
    public static class MaterialDynamicColors {
        public const double ContentAccentToneDelta = 15.0;

        public static readonly DynamicColor PrimaryPaletteKeyColor = DynamicColor.FromPalette(
            name: "primary_palette_key_color",
            palette: s => s.PrimaryPalette,
            tone: s => s.PrimaryPalette.KeyColor.Tone
        );

        public static readonly DynamicColor SecondaryPaletteKeyColor = DynamicColor.FromPalette(
            name: "secondary_palette_key_color",
            palette: s => s.SecondaryPalette,
            tone: s => s.SecondaryPalette.KeyColor.Tone
        );

        public static readonly DynamicColor TertiaryPaletteKeyColor = DynamicColor.FromPalette(
            name: "tertiary_palette_key_color",
            palette: s => s.TertiaryPalette,
            tone: s => s.TertiaryPalette.KeyColor.Tone
        );

        public static readonly DynamicColor NeutralPaletteKeyColor = DynamicColor.FromPalette(
            name: "neutral_palette_key_color",
            palette: s => s.NeutralPalette,
            tone: s => s.NeutralPalette.KeyColor.Tone
        );

        public static readonly DynamicColor NeutralVariantPaletteKeyColor = DynamicColor.FromPalette(
            name: "neutral_variant_palette_key_color",
            palette: s => s.NeutralVariantPalette,
            tone: s => s.NeutralVariantPalette.KeyColor.Tone
        );

        public static readonly DynamicColor Background = DynamicColor.FromPalette(
            name: "background",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark ? 6.0 : 98.0,
            isBackground: true
        );

        public static readonly DynamicColor OnBackground = DynamicColor.FromPalette(
            name: "on_background",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark ? 90.0 : 10.0,
            background: _ => Background,
            contrastCurve: new ContrastCurve(3, 3, 4.5, 7)
        );

        public static readonly DynamicColor Surface = DynamicColor.FromPalette(
            name: "surface",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark ? 6.0 : 98.0,
            isBackground: true
        );

        public static readonly DynamicColor SurfaceDim = DynamicColor.FromPalette(
            name: "surface_dim",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark ? 6.0 : new ContrastCurve(87, 87, 80, 75).Get(s.ContrastLevel),
            isBackground: true
        );

        public static readonly DynamicColor SurfaceBright = DynamicColor.FromPalette(
            name: "surface_bright",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark ? new ContrastCurve(24, 24, 29, 34).Get(s.ContrastLevel) : 98.0,
            isBackground: true
        );

        public static readonly DynamicColor SurfaceContainerLowest = DynamicColor.FromPalette(
            name: "surface_container_lowest",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark ? new ContrastCurve(4, 4, 2, 0).Get(s.ContrastLevel) : 100.0,
            isBackground: true
        );

        public static readonly DynamicColor SurfaceContainerLow = DynamicColor.FromPalette(
            name: "surface_container_low",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark
                ? new ContrastCurve(10, 10, 11, 12).Get(s.ContrastLevel)
                : new ContrastCurve(96, 96, 96, 95).Get(s.ContrastLevel),
            isBackground: true
        );

        public static readonly DynamicColor SurfaceContainer = DynamicColor.FromPalette(
            name: "surface_container",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark
                ? new ContrastCurve(12, 12, 16, 20).Get(s.ContrastLevel)
                : new ContrastCurve(94, 94, 92, 90).Get(s.ContrastLevel),
            isBackground: true
        );

        public static readonly DynamicColor SurfaceContainerHigh = DynamicColor.FromPalette(
            name: "surface_container_high",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark
                ? new ContrastCurve(17, 17, 21, 25).Get(s.ContrastLevel)
                : new ContrastCurve(92, 92, 88, 85).Get(s.ContrastLevel),
            isBackground: true
        );

        public static readonly DynamicColor SurfaceContainerHighest = DynamicColor.FromPalette(
            name: "surface_container_highest",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark
                ? new ContrastCurve(22, 22, 26, 30).Get(s.ContrastLevel)
                : new ContrastCurve(90, 90, 84, 80).Get(s.ContrastLevel),
            isBackground: true
        );

        public static readonly DynamicColor OnSurface = DynamicColor.FromPalette(
            name: "on_surface",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark ? 90.0 : 10.0,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor SurfaceVariant = DynamicColor.FromPalette(
            name: "surface_variant",
            palette: s => s.NeutralVariantPalette,
            tone: s => s.IsDark ? 30.0 : 90.0,
            isBackground: true
        );

        public static readonly DynamicColor OnSurfaceVariant = DynamicColor.FromPalette(
            name: "on_surface_variant",
            palette: s => s.NeutralVariantPalette,
            tone: s => s.IsDark ? 80.0 : 30.0,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        public static readonly DynamicColor InverseSurface = DynamicColor.FromPalette(
            name: "inverse_surface",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark ? 90.0 : 20.0
        );

        public static readonly DynamicColor InverseOnSurface = DynamicColor.FromPalette(
            name: "inverse_on_surface",
            palette: s => s.NeutralPalette,
            tone: s => s.IsDark ? 20.0 : 95.0,
            background: _ => InverseSurface,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor Outline = DynamicColor.FromPalette(
            name: "outline",
            palette: s => s.NeutralVariantPalette,
            tone: s => s.IsDark ? 60.0 : 50.0,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1.5, 3, 4.5, 7)
        );

        public static readonly DynamicColor OutlineVariant = DynamicColor.FromPalette(
            name: "outline_variant",
            palette: s => s.NeutralVariantPalette,
            tone: s => s.IsDark ? 30.0 : 80.0,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5)
        );

        public static readonly DynamicColor Shadow = DynamicColor.FromPalette(
            name: "shadow",
            palette: s => s.NeutralPalette,
            tone: _ => 0.0
        );

        public static readonly DynamicColor Scrim = DynamicColor.FromPalette(
            name: "scrim",
            palette: s => s.NeutralPalette,
            tone: _ => 0.0
        );

        public static readonly DynamicColor SurfaceTint = DynamicColor.FromPalette(
            name: "surface_tint",
            palette: s => s.PrimaryPalette,
            tone: s => s.IsDark ? 80.0 : 40.0,
            isBackground: true
        );

        public static readonly DynamicColor Primary = DynamicColor.FromPalette(
            name: "primary",
            palette: s => s.PrimaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 100.0 : 0.0;
                return s.IsDark ? 80.0 : 40.0;
            },
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 7),
            toneDeltaPair: _ => new ToneDeltaPair(PrimaryContainer, Primary, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnPrimary = DynamicColor.FromPalette(
            name: "on_primary",
            palette: s => s.PrimaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 10.0 : 90.0;
                return s.IsDark ? 20.0 : 100.0;
            },
            background: _ => Primary,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor PrimaryContainer = DynamicColor.FromPalette(
            name: "primary_container",
            palette: s => s.PrimaryPalette,
            tone: s => {
                if (IsFidelity(s)) return s.SourceColorHct.Tone;
                if (IsMonochrome(s)) return s.IsDark ? 85.0 : 25.0;
                return s.IsDark ? 30.0 : 90.0;
            },
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: _ => new ToneDeltaPair(PrimaryContainer, Primary, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnPrimaryContainer = DynamicColor.FromPalette(
            name: "on_primary_container",
            palette: s => s.PrimaryPalette,
            tone: s => {
                if (IsFidelity(s)) return DynamicColor.ForegroundTone(PrimaryContainer.Tone(s), 4.5);

                if (IsMonochrome(s)) return s.IsDark ? 0.0 : 100.0;
                return s.IsDark ? 90.0 : 30.0;
            },
            background: _ => PrimaryContainer,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        public static readonly DynamicColor InversePrimary = DynamicColor.FromPalette(
            name: "inverse_primary",
            palette: s => s.PrimaryPalette,
            tone: s => s.IsDark ? 40.0 : 80.0,
            background: _ => InverseSurface,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 7)
        );

        public static readonly DynamicColor Secondary = DynamicColor.FromPalette(
            name: "secondary",
            palette: s => s.SecondaryPalette,
            tone: s => s.IsDark ? 80.0 : 40.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 7),
            toneDeltaPair: _ => new ToneDeltaPair(SecondaryContainer, Secondary, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnSecondary = DynamicColor.FromPalette(
            name: "on_secondary",
            palette: s => s.SecondaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 10.0 : 100.0;
                return s.IsDark ? 20.0 : 100.0;
            },
            background: _ => Secondary,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor SecondaryContainer = DynamicColor.FromPalette(
            name: "secondary_container",
            palette: s => s.SecondaryPalette,
            tone: s => {
                var initialTone = s.IsDark ? 30.0 : 90.0;
                if (IsMonochrome(s)) return s.IsDark ? 30.0 : 85.0;
                if (!IsFidelity(s)) return initialTone;
                return FindDesiredChromaByTone(
                    s.SecondaryPalette.Hue,
                    s.SecondaryPalette.Chroma,
                    initialTone,
                    s.IsDark ? false : true
                );
            },
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: _ => new ToneDeltaPair(SecondaryContainer, Secondary, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnSecondaryContainer = DynamicColor.FromPalette(
            name: "on_secondary_container",
            palette: s => s.SecondaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 90.0 : 10.0;
                if (!IsFidelity(s)) return s.IsDark ? 90.0 : 30.0;
                return DynamicColor.ForegroundTone(SecondaryContainer.Tone(s), 4.5);
            },
            background: _ => SecondaryContainer,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        public static readonly DynamicColor Tertiary = DynamicColor.FromPalette(
            name: "tertiary",
            palette: s => s.TertiaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 90.0 : 25.0;
                return s.IsDark ? 80.0 : 40.0;
            },
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 7),
            toneDeltaPair: _ => new ToneDeltaPair(TertiaryContainer, Tertiary, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnTertiary = DynamicColor.FromPalette(
            name: "on_tertiary",
            palette: s => s.TertiaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 10.0 : 90.0;
                return s.IsDark ? 20.0 : 100.0;
            },
            background: _ => Tertiary,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor TertiaryContainer = DynamicColor.FromPalette(
            name: "tertiary_container",
            palette: s => s.TertiaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 60.0 : 49.0;
                if (!IsFidelity(s)) return s.IsDark ? 30.0 : 90.0;
                var proposedHct = s.TertiaryPalette.GetHct(s.SourceColorHct.Tone);
                return DislikeAnalyzer.FixIfDisliked(proposedHct).Tone;
            },
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: _ => new ToneDeltaPair(TertiaryContainer, Tertiary, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnTertiaryContainer = DynamicColor.FromPalette(
            name: "on_tertiary_container",
            palette: s => s.TertiaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 0.0 : 100.0;
                if (!IsFidelity(s)) return s.IsDark ? 90.0 : 30.0;
                return DynamicColor.ForegroundTone(TertiaryContainer.Tone(s), 4.5);
            },
            background: _ => TertiaryContainer,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        public static readonly DynamicColor Error = DynamicColor.FromPalette(
            name: "error",
            palette: s => s.ErrorPalette,
            tone: s => s.IsDark ? 80.0 : 40.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 7),
            toneDeltaPair: _ => new ToneDeltaPair(ErrorContainer, Error, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnError = DynamicColor.FromPalette(
            name: "on_error",
            palette: s => s.ErrorPalette,
            tone: s => s.IsDark ? 20.0 : 100.0,
            background: _ => Error,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor ErrorContainer = DynamicColor.FromPalette(
            name: "error_container",
            palette: s => s.ErrorPalette,
            tone: s => s.IsDark ? 30.0 : 90.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: _ => new ToneDeltaPair(ErrorContainer, Error, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnErrorContainer = DynamicColor.FromPalette(
            name: "on_error_container",
            palette: s => s.ErrorPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 90.0 : 10.0;
                return s.IsDark ? 90.0 : 30.0;
            },
            background: _ => ErrorContainer,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        public static readonly DynamicColor PrimaryFixed = DynamicColor.FromPalette(
            name: "primary_fixed",
            palette: s => s.PrimaryPalette,
            tone: s => IsMonochrome(s) ? 40.0 : 90.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: _ => new ToneDeltaPair(PrimaryFixed, PrimaryFixedDim, 10, TonePolarity.Lighter, true)
        );

        public static readonly DynamicColor PrimaryFixedDim = DynamicColor.FromPalette(
            name: "primary_fixed_dim",
            palette: s => s.PrimaryPalette,
            tone: s => IsMonochrome(s) ? 30.0 : 80.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: _ => new ToneDeltaPair(PrimaryFixed, PrimaryFixedDim, 10, TonePolarity.Lighter, true)
        );

        public static readonly DynamicColor OnPrimaryFixed = DynamicColor.FromPalette(
            name: "on_primary_fixed",
            palette: s => s.PrimaryPalette,
            tone: s => IsMonochrome(s) ? 100.0 : 10.0,
            background: _ => PrimaryFixedDim,
            secondBackground: _ => PrimaryFixed,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor OnPrimaryFixedVariant = DynamicColor.FromPalette(
            name: "on_primary_fixed_variant",
            palette: s => s.PrimaryPalette,
            tone: s => IsMonochrome(s) ? 90.0 : 30.0,
            background: _ => PrimaryFixedDim,
            secondBackground: _ => PrimaryFixed,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        public static readonly DynamicColor SecondaryFixed = DynamicColor.FromPalette(
            name: "secondary_fixed",
            palette: s => s.SecondaryPalette,
            tone: s => IsMonochrome(s) ? 80.0 : 90.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: _ => new ToneDeltaPair(SecondaryFixed, SecondaryFixedDim, 10, TonePolarity.Lighter, true)
        );

        public static readonly DynamicColor SecondaryFixedDim = DynamicColor.FromPalette(
            name: "secondary_fixed_dim",
            palette: s => s.SecondaryPalette,
            tone: s => IsMonochrome(s) ? 70.0 : 80.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: _ => new ToneDeltaPair(SecondaryFixed, SecondaryFixedDim, 10, TonePolarity.Lighter, true)
        );

        public static readonly DynamicColor OnSecondaryFixed = DynamicColor.FromPalette(
            name: "on_secondary_fixed",
            palette: s => s.SecondaryPalette,
            tone: _ => 10.0,
            background: _ => SecondaryFixedDim,
            secondBackground: _ => SecondaryFixed,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor OnSecondaryFixedVariant = DynamicColor.FromPalette(
            name: "on_secondary_fixed_variant",
            palette: s => s.SecondaryPalette,
            tone: s => IsMonochrome(s) ? 25.0 : 30.0,
            background: _ => SecondaryFixedDim,
            secondBackground: _ => SecondaryFixed,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        public static readonly DynamicColor TertiaryFixed = DynamicColor.FromPalette(
            name: "tertiary_fixed",
            palette: s => s.TertiaryPalette,
            tone: s => IsMonochrome(s) ? 40.0 : 90.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: _ => new ToneDeltaPair(TertiaryFixed, TertiaryFixedDim, 10, TonePolarity.Lighter, true)
        );

        public static readonly DynamicColor TertiaryFixedDim = DynamicColor.FromPalette(
            name: "tertiary_fixed_dim",
            palette: s => s.TertiaryPalette,
            tone: s => IsMonochrome(s) ? 30.0 : 80.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: _ => new ToneDeltaPair(TertiaryFixed, TertiaryFixedDim, 10, TonePolarity.Lighter, true)
        );

        public static readonly DynamicColor OnTertiaryFixed = DynamicColor.FromPalette(
            name: "on_tertiary_fixed",
            palette: s => s.TertiaryPalette,
            tone: s => IsMonochrome(s) ? 100.0 : 10.0,
            background: _ => TertiaryFixedDim,
            secondBackground: _ => TertiaryFixed,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor OnTertiaryFixedVariant = DynamicColor.FromPalette(
            name: "on_tertiary_fixed_variant",
            palette: s => s.TertiaryPalette,
            tone: s => IsMonochrome(s) ? 90.0 : 30.0,
            background: _ => TertiaryFixedDim,
            secondBackground: _ => TertiaryFixed,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        private static bool IsFidelity(DynamicScheme scheme) {
            return scheme.Variant == Variant.Fidelity || scheme.Variant == Variant.Content;
        }

        private static bool IsMonochrome(DynamicScheme scheme) {
            return scheme.Variant == Variant.Monochrome;
        }

        public static DynamicColor HighestSurface(DynamicScheme s) {
            return s.IsDark ? SurfaceBright : SurfaceDim;
        }

        private static double FindDesiredChromaByTone(double hue, double chroma, double tone, bool byDecreasingTone) {
            var answer = tone;

            var closestToChroma = Hct.From(hue, chroma, tone);
            if (closestToChroma.Chroma < chroma) {
                var chromaPeak = closestToChroma.Chroma;
                while (closestToChroma.Chroma < chroma) {
                    answer += byDecreasingTone ? -1.0 : 1.0;
                    var potentialSolution = Hct.From(hue, chroma, answer);

                    if (chromaPeak > potentialSolution.Chroma) break;

                    if (math.abs(potentialSolution.Chroma - chroma) < 0.4) break;

                    var potentialDelta = math.abs(potentialSolution.Chroma - chroma);
                    var currentDelta = math.abs(closestToChroma.Chroma - chroma);
                    if (potentialDelta < currentDelta) closestToChroma = potentialSolution;

                    chromaPeak = math.max(chromaPeak, potentialSolution.Chroma);
                }
            }

            return answer;
        }
    }
}
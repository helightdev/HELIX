using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace MaterialColorUtilities {
    /// <summary>
    /// A class containing a value that changes with the contrast level.
    /// </summary>
    public sealed class ContrastCurve {
        public double Low { get; }
        public double Normal { get; }
        public double Medium { get; }
        public double High { get; }

        public ContrastCurve(double low, double normal, double medium, double high) {
            Low = low;
            Normal = normal;
            Medium = medium;
            High = high;
        }

        public double Get(double contrastLevel) {
            if (contrastLevel <= -1.0) { return Low; } else if (contrastLevel < 0.0) {
                return MathUtils.Lerp(Low, Normal, (contrastLevel - (-1.0)) / 1.0);
            } else if (contrastLevel < 0.5) { return MathUtils.Lerp(Normal, Medium, contrastLevel / 0.5); } else if
                (contrastLevel < 1.0) { return MathUtils.Lerp(Medium, High, (contrastLevel - 0.5) / 0.5); } else {
                return High;
            }
        }
    }

    /// <summary>
    /// Describes the difference in tone between colors.
    /// </summary>
    public enum TonePolarity {
        Darker,
        Lighter,
        Nearer,
        Farther
    }

    /// <summary>
    /// Documents a constraint between two DynamicColors, in which their tones must
    /// have a certain distance from each other.
    /// </summary>
    public sealed class ToneDeltaPair {
        public DynamicColor RoleA { get; }
        public DynamicColor RoleB { get; }
        public double Delta { get; }
        public TonePolarity Polarity { get; }
        public bool StayTogether { get; }

        public ToneDeltaPair(
            DynamicColor roleA,
            DynamicColor roleB,
            double delta,
            TonePolarity polarity,
            bool stayTogether
        ) {
            RoleA = roleA;
            RoleB = roleB;
            Delta = delta;
            Polarity = polarity;
            StayTogether = stayTogether;
        }
    }

    /// <summary>
    /// Set of themes supported by Dynamic Color.
    /// </summary>
    public enum Variant {
        Monochrome,
        Neutral,
        TonalSpot,
        Vibrant,
        Expressive,
        Content,
        Fidelity,
        Rainbow,
        FruitSalad,
    }

    public static class VariantExtensions {
        public static string Label(this Variant variant) {
            return variant switch {
                Variant.Monochrome => "monochrome",
                Variant.Neutral    => "neutral",
                Variant.TonalSpot  => "tonal spot",
                Variant.Vibrant    => "vibrant",
                Variant.Expressive => "expressive",
                Variant.Content    => "content",
                Variant.Fidelity   => "fidelity",
                Variant.Rainbow    => "rainbow",
                Variant.FruitSalad => "fruit salad",
                _                  => variant.ToString()
            };
        }

        public static string Description(this Variant variant) {
            return variant switch {
                Variant.Monochrome => "All colors are grayscale, no chroma.",
                Variant.Neutral    => "Close to grayscale, a hint of chroma.",
                Variant.TonalSpot =>
                    "Pastel tokens, low chroma palettes (32).\nDefault Material You theme at 2021 launch.",
                Variant.Vibrant =>
                    "Pastel colors, high chroma palettes. (max).\nThe primary palette's chroma is at maximum.\nUse Fidelity instead if tokens should alter their tone to match the palette vibrancy.",
                Variant.Expressive =>
                    "Pastel colors, medium chroma palettes.\nThe primary palette's hue is different from source color, for variety.",
                Variant.Content =>
                    "Almost identical to Fidelity.\nTokens and palettes match source color.\nPrimary Container is source color, adjusted to ensure contrast with surfaces.\n\nTertiary palette is analogue of source color.\nFound by dividing color wheel by 6, then finding the 2 colors adjacent to source.\nThe one that increases hue is used.",
                Variant.Fidelity =>
                    "Tokens and palettes match source color.\nPrimary Container is source color, adjusted to ensure contrast with surfaces.\nFor example, if source color is black, it is lightened so it doesn't match surfaces in dark mode.\n\nTertiary palette is complement of source color.",
                Variant.Rainbow    => "A playful theme - the source color's hue does not appear in the theme.",
                Variant.FruitSalad => "A playful theme - the source color's hue does not appear in the theme.",
                _                  => string.Empty
            };
        }
    }

    /// <summary>
    /// A color that adjusts itself based on UI state provided by DynamicScheme.
    /// </summary>
    public sealed class DynamicColor {
        public string Name { get; }
        public Func<DynamicScheme, TonalPalette> Palette { get; }
        public Func<DynamicScheme, double> Tone { get; }
        public bool IsBackground { get; }
        public Func<DynamicScheme, DynamicColor> Background { get; }
        public Func<DynamicScheme, DynamicColor> SecondBackground { get; }
        public ContrastCurve ContrastCurve { get; }
        public Func<DynamicScheme, ToneDeltaPair> ToneDeltaPair { get; }

        private readonly Dictionary<DynamicScheme, Hct> _hctCache = new Dictionary<DynamicScheme, Hct>();

        public DynamicColor(
            string name,
            Func<DynamicScheme, TonalPalette> palette,
            Func<DynamicScheme, double> tone,
            bool isBackground,
            Func<DynamicScheme, DynamicColor> background,
            Func<DynamicScheme, DynamicColor> secondBackground,
            ContrastCurve contrastCurve,
            Func<DynamicScheme, ToneDeltaPair> toneDeltaPair
        ) {
            Name = name;
            Palette = palette;
            Tone = tone;
            IsBackground = isBackground;
            Background = background;
            SecondBackground = secondBackground;
            ContrastCurve = contrastCurve;
            ToneDeltaPair = toneDeltaPair;
        }

        public static DynamicColor FromPalette(
            Func<DynamicScheme, TonalPalette> palette,
            Func<DynamicScheme, double> tone,
            string name = "",
            bool isBackground = false,
            Func<DynamicScheme, DynamicColor> background = null,
            Func<DynamicScheme, DynamicColor> secondBackground = null,
            ContrastCurve contrastCurve = null,
            Func<DynamicScheme, ToneDeltaPair> toneDeltaPair = null
        ) {
            return new DynamicColor(
                name,
                palette,
                tone,
                isBackground,
                background,
                secondBackground,
                contrastCurve,
                toneDeltaPair
            );
        }

        public int GetArgb(DynamicScheme scheme) {
            return GetHct(scheme).ToInt();
        }

        public Hct GetHct(DynamicScheme scheme) {
            if (_hctCache.TryGetValue(scheme, out Hct cached)) { return cached; }

            double tone = GetTone(scheme);
            Hct answer = Palette(scheme).GetHct(tone);

            if (_hctCache.Count > 4) { _hctCache.Clear(); }

            _hctCache[scheme] = answer;
            return answer;
        }

        public double GetTone(DynamicScheme scheme) {
            bool decreasingContrast = scheme.ContrastLevel < 0.0;

            if (ToneDeltaPair != null) {
                ToneDeltaPair pair = ToneDeltaPair(scheme);
                DynamicColor roleA = pair.RoleA;
                DynamicColor roleB = pair.RoleB;
                double delta = pair.Delta;
                TonePolarity polarity = pair.Polarity;
                bool stayTogether = pair.StayTogether;

                DynamicColor bg = Background(scheme);
                double bgTone = bg.GetTone(scheme);

                bool aIsNearer =
                    polarity == TonePolarity.Nearer
                 || (polarity == TonePolarity.Lighter && !scheme.IsDark)
                 || (polarity == TonePolarity.Darker && scheme.IsDark);

                DynamicColor nearer = aIsNearer ? roleA : roleB;
                DynamicColor farther = aIsNearer ? roleB : roleA;
                bool amNearer = Name == nearer.Name;
                double expansionDir = scheme.IsDark ? 1.0 : -1.0;

                double nContrast = nearer.ContrastCurve.Get(scheme.ContrastLevel);
                double fContrast = farther.ContrastCurve.Get(scheme.ContrastLevel);

                double nInitialTone = nearer.Tone(scheme);
                double nTone =
                    Contrast.RatioOfTones(bgTone, nInitialTone) >= nContrast
                        ? nInitialTone
                        : ForegroundTone(bgTone, nContrast);

                double fInitialTone = farther.Tone(scheme);
                double fTone =
                    Contrast.RatioOfTones(bgTone, fInitialTone) >= fContrast
                        ? fInitialTone
                        : ForegroundTone(bgTone, fContrast);

                if (decreasingContrast) {
                    nTone = ForegroundTone(bgTone, nContrast);
                    fTone = ForegroundTone(bgTone, fContrast);
                }

                if ((fTone - nTone) * expansionDir >= delta) {
                    // no-op
                } else {
                    fTone = MathUtils.ClampDouble(0.0, 100.0, nTone + delta * expansionDir);
                    if ((fTone - nTone) * expansionDir >= delta) {
                        // no-op
                    } else { nTone = MathUtils.ClampDouble(0.0, 100.0, fTone - delta * expansionDir); }
                }

                if (50.0 <= nTone && nTone < 60.0) {
                    if (expansionDir > 0.0) {
                        nTone = 60.0;
                        fTone = math.max(fTone, nTone + delta * expansionDir);
                    } else {
                        nTone = 49.0;
                        fTone = math.min(fTone, nTone + delta * expansionDir);
                    }
                } else if (50.0 <= fTone && fTone < 60.0) {
                    if (stayTogether) {
                        if (expansionDir > 0.0) {
                            nTone = 60.0;
                            fTone = math.max(fTone, nTone + delta * expansionDir);
                        } else {
                            nTone = 49.0;
                            fTone = math.min(fTone, nTone + delta * expansionDir);
                        }
                    } else { fTone = expansionDir > 0.0 ? 60.0 : 49.0; }
                }

                return amNearer ? nTone : fTone;
            } else {
                double answer = Tone(scheme);

                if (Background == null) { return answer; }

                double bgTone = Background(scheme).GetTone(scheme);
                double desiredRatio = ContrastCurve.Get(scheme.ContrastLevel);

                if (Contrast.RatioOfTones(bgTone, answer) >= desiredRatio) {
                    // no-op
                } else { answer = ForegroundTone(bgTone, desiredRatio); }

                if (decreasingContrast) { answer = ForegroundTone(bgTone, desiredRatio); }

                if (IsBackground && 50.0 <= answer && answer < 60.0) {
                    if (Contrast.RatioOfTones(49.0, bgTone) >= desiredRatio) { answer = 49.0; } else { answer = 60.0; }
                }

                if (SecondBackground != null) {
                    double bgTone1 = Background(scheme).GetTone(scheme);
                    double bgTone2 = SecondBackground(scheme).GetTone(scheme);

                    double upper = math.max(bgTone1, bgTone2);
                    double lower = math.min(bgTone1, bgTone2);

                    if (Contrast.RatioOfTones(upper, answer) >= desiredRatio &&
                        Contrast.RatioOfTones(lower, answer) >= desiredRatio) { return answer; }

                    double lightOption = Contrast.Lighter(upper, desiredRatio);
                    double darkOption = Contrast.Darker(lower, desiredRatio);

                    List<double> availables = new List<double>();
                    if (lightOption != -1.0) availables.Add(lightOption);
                    if (darkOption != -1.0) availables.Add(darkOption);

                    bool prefersLight =
                        TonePrefersLightForeground(bgTone1) ||
                        TonePrefersLightForeground(bgTone2);

                    if (prefersLight) { return lightOption < 0.0 ? 100.0 : lightOption; }

                    if (availables.Count == 1) { return availables[0]; }

                    return darkOption < 0.0 ? 0.0 : darkOption;
                }

                return answer;
            }
        }

        public static double ForegroundTone(double bgTone, double ratio) {
            double lighterTone = Contrast.LighterUnsafe(bgTone, ratio);
            double darkerTone = Contrast.DarkerUnsafe(bgTone, ratio);
            double lighterRatio = Contrast.RatioOfTones(lighterTone, bgTone);
            double darkerRatio = Contrast.RatioOfTones(darkerTone, bgTone);
            bool preferLighter = TonePrefersLightForeground(bgTone);

            if (preferLighter) {
                bool negligibleDifference =
                    math.abs(lighterRatio - darkerRatio) < 0.1 &&
                    lighterRatio < ratio &&
                    darkerRatio < ratio;

                return lighterRatio >= ratio ||
                       lighterRatio >= darkerRatio ||
                       negligibleDifference
                    ? lighterTone
                    : darkerTone;
            } else {
                return darkerRatio >= ratio || darkerRatio >= lighterRatio
                    ? darkerTone
                    : lighterTone;
            }
        }

        public static double EnableLightForeground(double tone) {
            if (TonePrefersLightForeground(tone) && !ToneAllowsLightForeground(tone)) { return 49.0; }

            return tone;
        }

        public static bool TonePrefersLightForeground(double tone) {
            return math.round(tone) < 60.0;
        }

        public static bool ToneAllowsLightForeground(double tone) {
            return math.round(tone) <= 49.0;
        }
    }

    /// <summary>
    /// Constructed by a set of values representing the current UI state and
    /// provides a set of TonalPalettes.
    /// </summary>
    public class DynamicScheme {
        public int SourceColorArgb { get; }
        public Hct SourceColorHct { get; }
        public Variant Variant { get; }
        public bool IsDark { get; }
        public double ContrastLevel { get; }

        public TonalPalette PrimaryPalette { get; }
        public TonalPalette SecondaryPalette { get; }
        public TonalPalette TertiaryPalette { get; }
        public TonalPalette NeutralPalette { get; }
        public TonalPalette NeutralVariantPalette { get; }
        public TonalPalette ErrorPalette { get; }

        public DynamicScheme(
            Hct sourceColorHct,
            Variant variant,
            bool isDark,
            TonalPalette primaryPalette,
            TonalPalette secondaryPalette,
            TonalPalette tertiaryPalette,
            TonalPalette neutralPalette,
            TonalPalette neutralVariantPalette,
            double contrastLevel = 0.0,
            TonalPalette errorPalette = null
        ) {
            SourceColorHct = sourceColorHct;
            SourceColorArgb = sourceColorHct.ToInt();
            Variant = variant;
            ContrastLevel = contrastLevel;
            IsDark = isDark;
            PrimaryPalette = primaryPalette;
            SecondaryPalette = secondaryPalette;
            TertiaryPalette = tertiaryPalette;
            NeutralPalette = neutralPalette;
            NeutralVariantPalette = neutralVariantPalette;
            ErrorPalette = errorPalette ?? TonalPalette.Of(25.0, 84.0);
        }

        public static double GetRotatedHue(
            Hct sourceColor,
            IReadOnlyList<double> hues,
            IReadOnlyList<double> rotations
        ) {
            double sourceHue = sourceColor.Hue;

            if (hues.Count != rotations.Count) {
                throw new ArgumentException("hues.length must equal rotations.length");
            }

            if (rotations.Count == 1) { return MathUtils.SanitizeDegreesDouble(sourceColor.Hue + rotations[0]); }

            int size = hues.Count;
            for (int i = 0; i <= size - 2; i++) {
                double thisHue = hues[i];
                double nextHue = hues[i + 1];
                if (thisHue < sourceHue && sourceHue < nextHue) {
                    return MathUtils.SanitizeDegreesDouble(sourceHue + rotations[i]);
                }
            }

            return sourceHue;
        }

        public Hct GetHct(DynamicColor dynamicColor) => dynamicColor.GetHct(this);
        public int GetArgb(DynamicColor dynamicColor) => dynamicColor.GetArgb(this);

        public int PrimaryPaletteKeyColor => GetArgb(MaterialDynamicColors.PrimaryPaletteKeyColor);
        public int SecondaryPaletteKeyColor => GetArgb(MaterialDynamicColors.SecondaryPaletteKeyColor);
        public int TertiaryPaletteKeyColor => GetArgb(MaterialDynamicColors.TertiaryPaletteKeyColor);
        public int NeutralPaletteKeyColor => GetArgb(MaterialDynamicColors.NeutralPaletteKeyColor);
        public int NeutralVariantPaletteKeyColor => GetArgb(MaterialDynamicColors.NeutralVariantPaletteKeyColor);
        public int Background => GetArgb(MaterialDynamicColors.Background);
        public int OnBackground => GetArgb(MaterialDynamicColors.OnBackground);
        public int Surface => GetArgb(MaterialDynamicColors.Surface);
        public int SurfaceDim => GetArgb(MaterialDynamicColors.SurfaceDim);
        public int SurfaceBright => GetArgb(MaterialDynamicColors.SurfaceBright);
        public int SurfaceContainerLowest => GetArgb(MaterialDynamicColors.SurfaceContainerLowest);
        public int SurfaceContainerLow => GetArgb(MaterialDynamicColors.SurfaceContainerLow);
        public int SurfaceContainer => GetArgb(MaterialDynamicColors.SurfaceContainer);
        public int SurfaceContainerHigh => GetArgb(MaterialDynamicColors.SurfaceContainerHigh);
        public int SurfaceContainerHighest => GetArgb(MaterialDynamicColors.SurfaceContainerHighest);
        public int OnSurface => GetArgb(MaterialDynamicColors.OnSurface);
        public int SurfaceVariant => GetArgb(MaterialDynamicColors.SurfaceVariant);
        public int OnSurfaceVariant => GetArgb(MaterialDynamicColors.OnSurfaceVariant);
        public int InverseSurface => GetArgb(MaterialDynamicColors.InverseSurface);
        public int InverseOnSurface => GetArgb(MaterialDynamicColors.InverseOnSurface);
        public int Outline => GetArgb(MaterialDynamicColors.Outline);
        public int OutlineVariant => GetArgb(MaterialDynamicColors.OutlineVariant);
        public int Shadow => GetArgb(MaterialDynamicColors.Shadow);
        public int Scrim => GetArgb(MaterialDynamicColors.Scrim);
        public int SurfaceTint => GetArgb(MaterialDynamicColors.SurfaceTint);
        public int Primary => GetArgb(MaterialDynamicColors.Primary);
        public int OnPrimary => GetArgb(MaterialDynamicColors.OnPrimary);
        public int PrimaryContainer => GetArgb(MaterialDynamicColors.PrimaryContainer);
        public int OnPrimaryContainer => GetArgb(MaterialDynamicColors.OnPrimaryContainer);
        public int InversePrimary => GetArgb(MaterialDynamicColors.InversePrimary);
        public int Secondary => GetArgb(MaterialDynamicColors.Secondary);
        public int OnSecondary => GetArgb(MaterialDynamicColors.OnSecondary);
        public int SecondaryContainer => GetArgb(MaterialDynamicColors.SecondaryContainer);
        public int OnSecondaryContainer => GetArgb(MaterialDynamicColors.OnSecondaryContainer);
        public int Tertiary => GetArgb(MaterialDynamicColors.Tertiary);
        public int OnTertiary => GetArgb(MaterialDynamicColors.OnTertiary);
        public int TertiaryContainer => GetArgb(MaterialDynamicColors.TertiaryContainer);
        public int OnTertiaryContainer => GetArgb(MaterialDynamicColors.OnTertiaryContainer);
        public int Error => GetArgb(MaterialDynamicColors.Error);
        public int OnError => GetArgb(MaterialDynamicColors.OnError);
        public int ErrorContainer => GetArgb(MaterialDynamicColors.ErrorContainer);
        public int OnErrorContainer => GetArgb(MaterialDynamicColors.OnErrorContainer);
        public int PrimaryFixed => GetArgb(MaterialDynamicColors.PrimaryFixed);
        public int PrimaryFixedDim => GetArgb(MaterialDynamicColors.PrimaryFixedDim);
        public int OnPrimaryFixed => GetArgb(MaterialDynamicColors.OnPrimaryFixed);
        public int OnPrimaryFixedVariant => GetArgb(MaterialDynamicColors.OnPrimaryFixedVariant);
        public int SecondaryFixed => GetArgb(MaterialDynamicColors.SecondaryFixed);
        public int SecondaryFixedDim => GetArgb(MaterialDynamicColors.SecondaryFixedDim);
        public int OnSecondaryFixed => GetArgb(MaterialDynamicColors.OnSecondaryFixed);
        public int OnSecondaryFixedVariant => GetArgb(MaterialDynamicColors.OnSecondaryFixedVariant);
        public int TertiaryFixed => GetArgb(MaterialDynamicColors.TertiaryFixed);
        public int TertiaryFixedDim => GetArgb(MaterialDynamicColors.TertiaryFixedDim);
        public int OnTertiaryFixed => GetArgb(MaterialDynamicColors.OnTertiaryFixed);
        public int OnTertiaryFixedVariant => GetArgb(MaterialDynamicColors.OnTertiaryFixedVariant);
    }

    /// <summary>
    /// Tokens, or named colors, in the Material Design system.
    /// </summary>
    public static class MaterialDynamicColors {
        public const double ContentAccentToneDelta = 15.0;

        private static bool IsFidelity(DynamicScheme scheme) =>
            scheme.Variant == Variant.Fidelity || scheme.Variant == Variant.Content;

        private static bool IsMonochrome(DynamicScheme scheme) => scheme.Variant == Variant.Monochrome;

        public static DynamicColor HighestSurface(DynamicScheme s) {
            return s.IsDark ? SurfaceBright : SurfaceDim;
        }

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
            background: s => Background,
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
            background: s => InverseSurface,
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
            tone: s => 0.0
        );

        public static readonly DynamicColor Scrim = DynamicColor.FromPalette(
            name: "scrim",
            palette: s => s.NeutralPalette,
            tone: s => 0.0
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
            toneDeltaPair: s => new ToneDeltaPair(PrimaryContainer, Primary, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnPrimary = DynamicColor.FromPalette(
            name: "on_primary",
            palette: s => s.PrimaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 10.0 : 90.0;
                return s.IsDark ? 20.0 : 100.0;
            },
            background: s => Primary,
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
            toneDeltaPair: s => new ToneDeltaPair(PrimaryContainer, Primary, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnPrimaryContainer = DynamicColor.FromPalette(
            name: "on_primary_container",
            palette: s => s.PrimaryPalette,
            tone: s => {
                if (IsFidelity(s)) { return DynamicColor.ForegroundTone(PrimaryContainer.Tone(s), 4.5); }

                if (IsMonochrome(s)) return s.IsDark ? 0.0 : 100.0;
                return s.IsDark ? 90.0 : 30.0;
            },
            background: s => PrimaryContainer,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        public static readonly DynamicColor InversePrimary = DynamicColor.FromPalette(
            name: "inverse_primary",
            palette: s => s.PrimaryPalette,
            tone: s => s.IsDark ? 40.0 : 80.0,
            background: s => InverseSurface,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 7)
        );

        public static readonly DynamicColor Secondary = DynamicColor.FromPalette(
            name: "secondary",
            palette: s => s.SecondaryPalette,
            tone: s => s.IsDark ? 80.0 : 40.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 7),
            toneDeltaPair: s => new ToneDeltaPair(SecondaryContainer, Secondary, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnSecondary = DynamicColor.FromPalette(
            name: "on_secondary",
            palette: s => s.SecondaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 10.0 : 100.0;
                return s.IsDark ? 20.0 : 100.0;
            },
            background: s => Secondary,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor SecondaryContainer = DynamicColor.FromPalette(
            name: "secondary_container",
            palette: s => s.SecondaryPalette,
            tone: s => {
                double initialTone = s.IsDark ? 30.0 : 90.0;
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
            toneDeltaPair: s => new ToneDeltaPair(SecondaryContainer, Secondary, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnSecondaryContainer = DynamicColor.FromPalette(
            name: "on_secondary_container",
            palette: s => s.SecondaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 90.0 : 10.0;
                if (!IsFidelity(s)) return s.IsDark ? 90.0 : 30.0;
                return DynamicColor.ForegroundTone(SecondaryContainer.Tone(s), 4.5);
            },
            background: s => SecondaryContainer,
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
            toneDeltaPair: s => new ToneDeltaPair(TertiaryContainer, Tertiary, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnTertiary = DynamicColor.FromPalette(
            name: "on_tertiary",
            palette: s => s.TertiaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 10.0 : 90.0;
                return s.IsDark ? 20.0 : 100.0;
            },
            background: s => Tertiary,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor TertiaryContainer = DynamicColor.FromPalette(
            name: "tertiary_container",
            palette: s => s.TertiaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 60.0 : 49.0;
                if (!IsFidelity(s)) return s.IsDark ? 30.0 : 90.0;
                Hct proposedHct = s.TertiaryPalette.GetHct(s.SourceColorHct.Tone);
                return DislikeAnalyzer.FixIfDisliked(proposedHct).Tone;
            },
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: s => new ToneDeltaPair(TertiaryContainer, Tertiary, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnTertiaryContainer = DynamicColor.FromPalette(
            name: "on_tertiary_container",
            palette: s => s.TertiaryPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 0.0 : 100.0;
                if (!IsFidelity(s)) return s.IsDark ? 90.0 : 30.0;
                return DynamicColor.ForegroundTone(TertiaryContainer.Tone(s), 4.5);
            },
            background: s => TertiaryContainer,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        public static readonly DynamicColor Error = DynamicColor.FromPalette(
            name: "error",
            palette: s => s.ErrorPalette,
            tone: s => s.IsDark ? 80.0 : 40.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 7),
            toneDeltaPair: s => new ToneDeltaPair(ErrorContainer, Error, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnError = DynamicColor.FromPalette(
            name: "on_error",
            palette: s => s.ErrorPalette,
            tone: s => s.IsDark ? 20.0 : 100.0,
            background: s => Error,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor ErrorContainer = DynamicColor.FromPalette(
            name: "error_container",
            palette: s => s.ErrorPalette,
            tone: s => s.IsDark ? 30.0 : 90.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: s => new ToneDeltaPair(ErrorContainer, Error, 10, TonePolarity.Nearer, false)
        );

        public static readonly DynamicColor OnErrorContainer = DynamicColor.FromPalette(
            name: "on_error_container",
            palette: s => s.ErrorPalette,
            tone: s => {
                if (IsMonochrome(s)) return s.IsDark ? 90.0 : 10.0;
                return s.IsDark ? 90.0 : 30.0;
            },
            background: s => ErrorContainer,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        public static readonly DynamicColor PrimaryFixed = DynamicColor.FromPalette(
            name: "primary_fixed",
            palette: s => s.PrimaryPalette,
            tone: s => IsMonochrome(s) ? 40.0 : 90.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: s => new ToneDeltaPair(PrimaryFixed, PrimaryFixedDim, 10, TonePolarity.Lighter, true)
        );

        public static readonly DynamicColor PrimaryFixedDim = DynamicColor.FromPalette(
            name: "primary_fixed_dim",
            palette: s => s.PrimaryPalette,
            tone: s => IsMonochrome(s) ? 30.0 : 80.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: s => new ToneDeltaPair(PrimaryFixed, PrimaryFixedDim, 10, TonePolarity.Lighter, true)
        );

        public static readonly DynamicColor OnPrimaryFixed = DynamicColor.FromPalette(
            name: "on_primary_fixed",
            palette: s => s.PrimaryPalette,
            tone: s => IsMonochrome(s) ? 100.0 : 10.0,
            background: s => PrimaryFixedDim,
            secondBackground: s => PrimaryFixed,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor OnPrimaryFixedVariant = DynamicColor.FromPalette(
            name: "on_primary_fixed_variant",
            palette: s => s.PrimaryPalette,
            tone: s => IsMonochrome(s) ? 90.0 : 30.0,
            background: s => PrimaryFixedDim,
            secondBackground: s => PrimaryFixed,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        public static readonly DynamicColor SecondaryFixed = DynamicColor.FromPalette(
            name: "secondary_fixed",
            palette: s => s.SecondaryPalette,
            tone: s => IsMonochrome(s) ? 80.0 : 90.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: s => new ToneDeltaPair(SecondaryFixed, SecondaryFixedDim, 10, TonePolarity.Lighter, true)
        );

        public static readonly DynamicColor SecondaryFixedDim = DynamicColor.FromPalette(
            name: "secondary_fixed_dim",
            palette: s => s.SecondaryPalette,
            tone: s => IsMonochrome(s) ? 70.0 : 80.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: s => new ToneDeltaPair(SecondaryFixed, SecondaryFixedDim, 10, TonePolarity.Lighter, true)
        );

        public static readonly DynamicColor OnSecondaryFixed = DynamicColor.FromPalette(
            name: "on_secondary_fixed",
            palette: s => s.SecondaryPalette,
            tone: s => 10.0,
            background: s => SecondaryFixedDim,
            secondBackground: s => SecondaryFixed,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor OnSecondaryFixedVariant = DynamicColor.FromPalette(
            name: "on_secondary_fixed_variant",
            palette: s => s.SecondaryPalette,
            tone: s => IsMonochrome(s) ? 25.0 : 30.0,
            background: s => SecondaryFixedDim,
            secondBackground: s => SecondaryFixed,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        public static readonly DynamicColor TertiaryFixed = DynamicColor.FromPalette(
            name: "tertiary_fixed",
            palette: s => s.TertiaryPalette,
            tone: s => IsMonochrome(s) ? 40.0 : 90.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: s => new ToneDeltaPair(TertiaryFixed, TertiaryFixedDim, 10, TonePolarity.Lighter, true)
        );

        public static readonly DynamicColor TertiaryFixedDim = DynamicColor.FromPalette(
            name: "tertiary_fixed_dim",
            palette: s => s.TertiaryPalette,
            tone: s => IsMonochrome(s) ? 30.0 : 80.0,
            isBackground: true,
            background: HighestSurface,
            contrastCurve: new ContrastCurve(1, 1, 3, 4.5),
            toneDeltaPair: s => new ToneDeltaPair(TertiaryFixed, TertiaryFixedDim, 10, TonePolarity.Lighter, true)
        );

        public static readonly DynamicColor OnTertiaryFixed = DynamicColor.FromPalette(
            name: "on_tertiary_fixed",
            palette: s => s.TertiaryPalette,
            tone: s => IsMonochrome(s) ? 100.0 : 10.0,
            background: s => TertiaryFixedDim,
            secondBackground: s => TertiaryFixed,
            contrastCurve: new ContrastCurve(4.5, 7, 11, 21)
        );

        public static readonly DynamicColor OnTertiaryFixedVariant = DynamicColor.FromPalette(
            name: "on_tertiary_fixed_variant",
            palette: s => s.TertiaryPalette,
            tone: s => IsMonochrome(s) ? 90.0 : 30.0,
            background: s => TertiaryFixedDim,
            secondBackground: s => TertiaryFixed,
            contrastCurve: new ContrastCurve(3, 4.5, 7, 11)
        );

        private static double FindDesiredChromaByTone(double hue, double chroma, double tone, bool byDecreasingTone) {
            double answer = tone;

            Hct closestToChroma = Hct.From(hue, chroma, tone);
            if (closestToChroma.Chroma < chroma) {
                double chromaPeak = closestToChroma.Chroma;
                while (closestToChroma.Chroma < chroma) {
                    answer += byDecreasingTone ? -1.0 : 1.0;
                    Hct potentialSolution = Hct.From(hue, chroma, answer);

                    if (chromaPeak > potentialSolution.Chroma) { break; }

                    if (math.abs(potentialSolution.Chroma - chroma) < 0.4) { break; }

                    double potentialDelta = math.abs(potentialSolution.Chroma - chroma);
                    double currentDelta = math.abs(closestToChroma.Chroma - chroma);
                    if (potentialDelta < currentDelta) { closestToChroma = potentialSolution; }

                    chromaPeak = math.max(chromaPeak, potentialSolution.Chroma);
                }
            }

            return answer;
        }
    }
}
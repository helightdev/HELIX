using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     A color that adjusts itself based on UI state provided by DynamicScheme.
    /// </summary>
    public sealed class DynamicColor {
        private readonly Dictionary<DynamicScheme, Hct> _hctCache = new();

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

        public string Name { get; }
        public Func<DynamicScheme, TonalPalette> Palette { get; }
        public Func<DynamicScheme, double> Tone { get; }
        public bool IsBackground { get; }
        public Func<DynamicScheme, DynamicColor> Background { get; }
        public Func<DynamicScheme, DynamicColor> SecondBackground { get; }
        public ContrastCurve ContrastCurve { get; }
        public Func<DynamicScheme, ToneDeltaPair> ToneDeltaPair { get; }

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
            if (_hctCache.TryGetValue(scheme, out var cached)) return cached;

            var tone = GetTone(scheme);
            var answer = Palette(scheme).GetHct(tone);

            if (_hctCache.Count > 4) _hctCache.Clear();

            _hctCache[scheme] = answer;
            return answer;
        }

        public double GetTone(DynamicScheme scheme) {
            var decreasingContrast = scheme.ContrastLevel < 0.0;

            if (ToneDeltaPair != null) {
                var pair = ToneDeltaPair(scheme);
                var roleA = pair.RoleA;
                var roleB = pair.RoleB;
                var delta = pair.Delta;
                var polarity = pair.Polarity;
                var stayTogether = pair.StayTogether;

                var bg = Background(scheme);
                var bgTone = bg.GetTone(scheme);

                var aIsNearer =
                    polarity == TonePolarity.Nearer
                 || (polarity == TonePolarity.Lighter && !scheme.IsDark)
                 || (polarity == TonePolarity.Darker && scheme.IsDark);

                var nearer = aIsNearer ? roleA : roleB;
                var farther = aIsNearer ? roleB : roleA;
                var amNearer = Name == nearer.Name;
                var expansionDir = scheme.IsDark ? 1.0 : -1.0;

                var nContrast = nearer.ContrastCurve.Get(scheme.ContrastLevel);
                var fContrast = farther.ContrastCurve.Get(scheme.ContrastLevel);

                var nInitialTone = nearer.Tone(scheme);
                var nTone =
                    Contrast.RatioOfTones(bgTone, nInitialTone) >= nContrast
                        ? nInitialTone
                        : ForegroundTone(bgTone, nContrast);

                var fInitialTone = farther.Tone(scheme);
                var fTone =
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
                    } else nTone = MathUtils.ClampDouble(0.0, 100.0, fTone - delta * expansionDir);
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
                    } else fTone = expansionDir > 0.0 ? 60.0 : 49.0;
                }

                return amNearer ? nTone : fTone;
            } else {
                var answer = Tone(scheme);

                if (Background == null) return answer;

                var bgTone = Background(scheme).GetTone(scheme);
                var desiredRatio = ContrastCurve.Get(scheme.ContrastLevel);

                if (Contrast.RatioOfTones(bgTone, answer) >= desiredRatio) {
                    // no-op
                } else answer = ForegroundTone(bgTone, desiredRatio);

                if (decreasingContrast) answer = ForegroundTone(bgTone, desiredRatio);

                if (IsBackground && 50.0 <= answer && answer < 60.0) {
                    if (Contrast.RatioOfTones(49.0, bgTone) >= desiredRatio) answer = 49.0;
                    else answer = 60.0;
                }

                if (SecondBackground != null) {
                    var bgTone1 = Background(scheme).GetTone(scheme);
                    var bgTone2 = SecondBackground(scheme).GetTone(scheme);

                    var upper = math.max(bgTone1, bgTone2);
                    var lower = math.min(bgTone1, bgTone2);

                    if (Contrast.RatioOfTones(upper, answer) >= desiredRatio &&
                        Contrast.RatioOfTones(lower, answer) >= desiredRatio) return answer;

                    var lightOption = Contrast.Lighter(upper, desiredRatio);
                    var darkOption = Contrast.Darker(lower, desiredRatio);

                    var availables = new List<double>();
                    if (!MathUtils.ApproximatelyEqual(lightOption, -1.0)) availables.Add(lightOption);
                    if (!MathUtils.ApproximatelyEqual(darkOption, -1.0)) availables.Add(darkOption);

                    var prefersLight =
                        TonePrefersLightForeground(bgTone1) ||
                        TonePrefersLightForeground(bgTone2);

                    if (prefersLight) return lightOption < 0.0 ? 100.0 : lightOption;

                    if (availables.Count == 1) return availables[0];

                    return darkOption < 0.0 ? 0.0 : darkOption;
                }

                return answer;
            }
        }

        public static double ForegroundTone(double bgTone, double ratio) {
            var lighterTone = Contrast.LighterUnsafe(bgTone, ratio);
            var darkerTone = Contrast.DarkerUnsafe(bgTone, ratio);
            var lighterRatio = Contrast.RatioOfTones(lighterTone, bgTone);
            var darkerRatio = Contrast.RatioOfTones(darkerTone, bgTone);
            var preferLighter = TonePrefersLightForeground(bgTone);

            if (preferLighter) {
                var negligibleDifference =
                    math.abs(lighterRatio - darkerRatio) < 0.1 &&
                    lighterRatio < ratio &&
                    darkerRatio < ratio;

                return lighterRatio >= ratio ||
                       lighterRatio >= darkerRatio ||
                       negligibleDifference
                    ? lighterTone
                    : darkerTone;
            }

            return darkerRatio >= ratio || darkerRatio >= lighterRatio
                ? darkerTone
                : lighterTone;
        }

        public static double EnableLightForeground(double tone) {
            if (TonePrefersLightForeground(tone) && !ToneAllowsLightForeground(tone)) return 49.0;

            return tone;
        }

        public static bool TonePrefersLightForeground(double tone) {
            return math.round(tone) < 60.0;
        }

        public static bool ToneAllowsLightForeground(double tone) {
            return math.round(tone) <= 49.0;
        }
    }
}
using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///     Design utilities using color temperature theory.
    ///     Analogous colors, complementary color, and cache to efficiently, lazily,
    ///     generate data for calculations when needed.
    /// </summary>
    public sealed class TemperatureCache {
        private Hct _complement;
        private List<Hct> _hctsByHue = new();

        private List<Hct> _hctsByTemp = new();
        private double _inputRelativeTemperature = -1.0;
        private Dictionary<Hct, double> _tempsByHct = new();

        public TemperatureCache(Hct input) {
            Input = input;
        }

        public Hct Input { get; }

        public Hct Warmest => HctsByTemp[HctsByTemp.Count - 1];
        public Hct Coldest => HctsByTemp[0];

        /// <summary>
        ///     A color that complements the input color aesthetically.
        /// </summary>
        public Hct Complement {
            get {
                if (_complement != null) return _complement;

                var coldestHue = Coldest.Hue;
                var coldestTemp = TempsByHct[Coldest];

                var warmestHue = Warmest.Hue;
                var warmestTemp = TempsByHct[Warmest];
                var range = warmestTemp - coldestTemp;

                var startHueIsColdestToWarmest = IsBetween(Input.Hue, coldestHue, warmestHue);
                var startHue = startHueIsColdestToWarmest ? warmestHue : coldestHue;
                var endHue = startHueIsColdestToWarmest ? coldestHue : warmestHue;

                const double directionOfRotation = 1.0;
                var smallestError = 1000.0;
                var answer = HctsByHue[(int)math.round(Input.Hue)];

                var complementRelativeTemp = 1.0 - InputRelativeTemperature;

                for (var hueAddend = 0.0; hueAddend <= 360.0; hueAddend += 1.0) {
                    var hue = MathUtils.SanitizeDegreesDouble(startHue + directionOfRotation * hueAddend);
                    if (!IsBetween(hue, startHue, endHue)) continue;

                    var possibleAnswer = HctsByHue[(int)math.round(hue)];
                    var relativeTemp = (_tempsByHct[possibleAnswer] - coldestTemp) / range;
                    var error = math.abs(complementRelativeTemp - relativeTemp);

                    if (error < smallestError) {
                        smallestError = error;
                        answer = possibleAnswer;
                    }
                }

                _complement = answer;
                return _complement;
            }
        }

        /// <summary>
        ///     Relative temperature of the input color.
        /// </summary>
        public double InputRelativeTemperature {
            get {
                if (_inputRelativeTemperature >= 0.0) return _inputRelativeTemperature;

                var coldestTemp = TempsByHct[Coldest];
                var range = TempsByHct[Warmest] - coldestTemp;
                var differenceFromColdest = TempsByHct[Input] - coldestTemp;

                _inputRelativeTemperature = range == 0.0 ? 0.5 : differenceFromColdest / range;
                return _inputRelativeTemperature;
            }
        }

        /// <summary>
        ///     HCTs for all hues, with the same chroma/tone as the input.
        ///     Sorted from coldest first to warmest last.
        /// </summary>
        public List<Hct> HctsByTemp {
            get {
                if (_hctsByTemp.Count > 0) return _hctsByTemp;

                var hcts = new List<Hct>(HctsByHue);
                hcts.Add(Input);

                var temperaturesByHct = TempsByHct;
                hcts.Sort((a, b) => temperaturesByHct[a].CompareTo(temperaturesByHct[b]));

                _hctsByTemp = hcts;
                return _hctsByTemp;
            }
        }

        /// <summary>
        ///     A map with keys of HCTs in HctsByTemp, values of raw temperature.
        /// </summary>
        public Dictionary<Hct, double> TempsByHct {
            get {
                if (_tempsByHct.Count > 0) return _tempsByHct;

                var allHcts = new List<Hct>(HctsByHue);
                allHcts.Add(Input);

                var temperaturesByHct = new Dictionary<Hct, double>();
                foreach (var hct in allHcts) temperaturesByHct[hct] = RawTemperature(hct);

                _tempsByHct = temperaturesByHct;
                return _tempsByHct;
            }
        }

        /// <summary>
        ///     HCTs for all hues, with the same chroma/tone as the input.
        ///     Sorted ascending, hue 0 to 360.
        /// </summary>
        public List<Hct> HctsByHue {
            get {
                if (_hctsByHue.Count > 0) return _hctsByHue;

                var hcts = new List<Hct>();
                for (var hue = 0.0; hue <= 360.0; hue += 1.0) hcts.Add(Hct.From(hue, Input.Chroma, Input.Tone));

                _hctsByHue = hcts;
                return _hctsByHue;
            }
        }

        /// <summary>
        ///     A set of colors with differing hues, equidistant in temperature.
        /// </summary>
        public List<Hct> Analogous(int count = 5, int divisions = 12) {
            var startHue = (int)math.round(Input.Hue);
            var startHct = HctsByHue[startHue];
            var lastTemp = RelativeTemperature(startHct);
            var allColors = new List<Hct> { startHct };

            var absoluteTotalTempDelta = 0.0;
            for (var i = 0; i < 360; i++) {
                var hue = MathUtils.SanitizeDegreesInt(startHue + i);
                var hct = HctsByHue[hue];
                var temp = RelativeTemperature(hct);
                var tempDelta = math.abs(temp - lastTemp);
                lastTemp = temp;
                absoluteTotalTempDelta += tempDelta;
            }

            var hueAddend = 1;
            var tempStep = absoluteTotalTempDelta / divisions;
            var totalTempDelta = 0.0;
            lastTemp = RelativeTemperature(startHct);

            while (allColors.Count < divisions) {
                var hue = MathUtils.SanitizeDegreesInt(startHue + hueAddend);
                var hct = HctsByHue[hue];
                var temp = RelativeTemperature(hct);
                var tempDelta = math.abs(temp - lastTemp);
                totalTempDelta += tempDelta;

                var desiredTotalTempDeltaForIndex = allColors.Count * tempStep;
                var indexSatisfied = totalTempDelta >= desiredTotalTempDeltaForIndex;
                var indexAddend = 1;

                while (indexSatisfied && allColors.Count < divisions) {
                    allColors.Add(hct);
                    desiredTotalTempDeltaForIndex = (allColors.Count + indexAddend) * tempStep;
                    indexSatisfied = totalTempDelta >= desiredTotalTempDeltaForIndex;
                    indexAddend++;
                }

                lastTemp = temp;
                hueAddend++;

                if (hueAddend > 360) {
                    while (allColors.Count < divisions) allColors.Add(hct);

                    break;
                }
            }

            var answers = new List<Hct> { Input };

            var increaseHueCount = (int)math.floor((count - 1) / 2.0);
            for (var i = 1; i < increaseHueCount + 1; i++) {
                var index = -i;
                while (index < 0) index = allColors.Count + index;

                if (index >= allColors.Count) index %= allColors.Count;

                answers.Insert(0, allColors[index]);
            }

            var decreaseHueCount = count - increaseHueCount - 1;
            for (var i = 1; i < decreaseHueCount + 1; i++) {
                var index = i;
                while (index < 0) index = allColors.Count + index;

                if (index >= allColors.Count) index %= allColors.Count;

                answers.Add(allColors[index]);
            }

            return answers;
        }

        /// <summary>
        ///     Temperature relative to all colors with the same chroma and tone.
        ///     Value on a scale from 0 to 1.
        /// </summary>
        public double RelativeTemperature(Hct hct) {
            var range = TempsByHct[Warmest] - TempsByHct[Coldest];
            var differenceFromColdest = TempsByHct[hct] - TempsByHct[Coldest];

            if (range == 0.0) return 0.5;

            return differenceFromColdest / range;
        }

        /// <summary>
        ///     Determines if an angle is between two other angles, rotating clockwise.
        /// </summary>
        public static bool IsBetween(double angle, double a, double b) {
            if (a < b) return a <= angle && angle <= b;

            return a <= angle || angle <= b;
        }

        /// <summary>
        ///     Value representing cool-warm factor of a color.
        ///     Values below 0 are considered cool, above 0 warm.
        /// </summary>
        public static double RawTemperature(Hct color) {
            var lab = ColorUtils.LabFromArgb(color.ToInt());

            var hue = MathUtils.SanitizeDegreesDouble(math.degrees(math.atan2(lab.z, lab.y)));

            var chroma = math.sqrt(lab.y * lab.y + lab.z * lab.z);

            return -0.5
                 + 0.02 * math.pow(chroma, 1.07)
                        * math.cos(math.radians(MathUtils.SanitizeDegreesDouble(hue - 50.0)));
        }
    }

    /// <summary>
    ///     Functions for blending in HCT and CAM16.
    /// </summary>
    public static class Blend {
        /// <summary>
        ///     Blend the design color's HCT hue towards the key color's HCT hue.
        /// </summary>
        public static int Harmonize(int designColor, int sourceColor) {
            var fromHct = Hct.FromInt(designColor);
            var toHct = Hct.FromInt(sourceColor);

            var differenceDegrees = MathUtils.DifferenceDegrees(fromHct.Hue, toHct.Hue);
            var rotationDegrees = math.min(differenceDegrees * 0.5, 15.0);

            var outputHue = MathUtils.SanitizeDegreesDouble(
                fromHct.Hue
              + rotationDegrees * MathUtils.RotationDirection(fromHct.Hue, toHct.Hue)
            );

            return Hct.From(outputHue, fromHct.Chroma, fromHct.Tone).ToInt();
        }

        /// <summary>
        ///     Blends hue from one color into another. Chroma and tone of the original are maintained.
        /// </summary>
        public static int HctHue(int from, int to, double amount) {
            var ucs = Cam16Ucs(from, to, amount);
            var ucsCam = Cam16.FromInt(ucs);
            var fromCam = Cam16.FromInt(from);

            var blended = Hct.From(
                ucsCam.Hue,
                fromCam.Chroma,
                ColorUtils.LstarFromArgb(from)
            );

            return blended.ToInt();
        }

        /// <summary>
        ///     Blend in CAM16-UCS space.
        /// </summary>
        public static int Cam16Ucs(int from, int to, double amount) {
            var fromCam = Cam16.FromInt(from);
            var toCam = Cam16.FromInt(to);

            var jstar = fromCam.Jstar + (toCam.Jstar - fromCam.Jstar) * amount;
            var astar = fromCam.Astar + (toCam.Astar - fromCam.Astar) * amount;
            var bstar = fromCam.Bstar + (toCam.Bstar - fromCam.Bstar) * amount;

            return Cam16.FromUcs(jstar, astar, bstar).ToInt();
        }
    }

    /// <summary>
    ///     Utility methods for calculating contrast given two colors,
    ///     or calculating a color given one color and a contrast ratio.
    /// </summary>
    public static class Contrast {
        /// <summary>
        ///     Returns a contrast ratio, which ranges from 1 to 21.
        /// </summary>
        public static double RatioOfTones(double toneA, double toneB) {
            toneA = MathUtils.ClampDouble(0.0, 100.0, toneA);
            toneB = MathUtils.ClampDouble(0.0, 100.0, toneB);

            return RatioOfYs(
                ColorUtils.YFromLstar(toneA),
                ColorUtils.YFromLstar(toneB)
            );
        }

        private static double RatioOfYs(double y1, double y2) {
            var lighter = y1 > y2 ? y1 : y2;
            var darker = MathUtils.ApproximatelyEqual(lighter, y2) ? y1 : y2;
            return (lighter + 5.0) / (darker + 5.0);
        }

        /// <summary>
        ///     Returns a tone &gt;= tone that ensures ratio.
        ///     Returns -1 if ratio cannot be achieved.
        /// </summary>
        public static double Lighter(double tone, double ratio) {
            if (tone < 0.0 || tone > 100.0) return -1.0;

            var darkY = ColorUtils.YFromLstar(tone);
            var lightY = ratio * (darkY + 5.0) - 5.0;
            var realContrast = RatioOfYs(lightY, darkY);
            var delta = math.abs(realContrast - ratio);

            if (realContrast < ratio && delta > 0.04) return -1.0;

            var returnValue = ColorUtils.LstarFromY(lightY) + 0.4;
            if (returnValue < 0.0 || returnValue > 100.0) return -1.0;

            return returnValue;
        }

        /// <summary>
        ///     Returns a tone &lt;= tone that ensures ratio.
        ///     Returns -1 if ratio cannot be achieved.
        /// </summary>
        public static double Darker(double tone, double ratio) {
            if (tone < 0.0 || tone > 100.0) return -1.0;

            var lightY = ColorUtils.YFromLstar(tone);
            var darkY = (lightY + 5.0) / ratio - 5.0;
            var realContrast = RatioOfYs(lightY, darkY);
            var delta = math.abs(realContrast - ratio);

            if (realContrast < ratio && delta > 0.04) return -1.0;

            var returnValue = ColorUtils.LstarFromY(darkY) - 0.4;
            if (returnValue < 0.0 || returnValue > 100.0) return -1.0;

            return returnValue;
        }

        /// <summary>
        ///     Unsafe lighter version. Returns 100 if ratio cannot be achieved.
        /// </summary>
        public static double LighterUnsafe(double tone, double ratio) {
            var lighterSafe = Lighter(tone, ratio);
            return lighterSafe < 0.0 ? 100.0 : lighterSafe;
        }

        /// <summary>
        ///     Unsafe darker version. Returns 0 if ratio cannot be achieved.
        /// </summary>
        public static double DarkerUnsafe(double tone, double ratio) {
            var darkerSafe = Darker(tone, ratio);
            return darkerSafe < 0.0 ? 0.0 : darkerSafe;
        }
    }

    /// <summary>
    ///     Check and/or fix universally disliked colors.
    /// </summary>
    public static class DislikeAnalyzer {
        /// <summary>
        ///     Returns true if hct is disliked.
        /// </summary>
        public static bool IsDisliked(Hct hct) {
            var huePasses = math.round(hct.Hue) >= 90.0 && math.round(hct.Hue) <= 111.0;
            var chromaPasses = math.round(hct.Chroma) > 16.0;
            var tonePasses = math.round(hct.Tone) < 65.0;

            return huePasses && chromaPasses && tonePasses;
        }

        /// <summary>
        ///     If hct is disliked, lighten it to make it likable.
        /// </summary>
        public static Hct FixIfDisliked(Hct hct) {
            if (IsDisliked(hct)) return Hct.From(hct.Hue, hct.Chroma, 70.0);

            return hct;
        }
    }

    /// <summary>
    ///     Comprises foundational palettes to build a color scheme.
    /// </summary>
    public sealed class CorePalettes {
        public CorePalettes(
            TonalPalette primary,
            TonalPalette secondary,
            TonalPalette tertiary,
            TonalPalette neutral,
            TonalPalette neutralVariant
        ) {
            Primary = primary;
            Secondary = secondary;
            Tertiary = tertiary;
            Neutral = neutral;
            NeutralVariant = neutralVariant;
        }

        public TonalPalette Primary { get; }
        public TonalPalette Secondary { get; }
        public TonalPalette Tertiary { get; }
        public TonalPalette Neutral { get; }
        public TonalPalette NeutralVariant { get; }
    }

    /// <summary>
    ///     A convenience class for retrieving colors that are constant in hue and
    ///     chroma, but vary in tone.
    /// </summary>
    public sealed class TonalPalette : IEquatable<TonalPalette> {
        public static readonly int[] CommonTones = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 95, 99, 100 };

        private readonly Dictionary<int, int> _cache;
        private readonly bool _isFromCache;

        private TonalPalette(Hct hct) {
            _cache = new Dictionary<int, int>();
            Hue = hct.Hue;
            Chroma = hct.Chroma;
            KeyColor = hct;
            _isFromCache = false;
        }

        private TonalPalette(double hue, double chroma) {
            _cache = new Dictionary<int, int>();
            Hue = hue;
            Chroma = chroma;
            KeyColor = new KeyColor(hue, chroma).Create();
            _isFromCache = false;
        }

        private TonalPalette(Dictionary<int, int> cache, double hue, double chroma) {
            _cache = cache;
            Hue = hue;
            Chroma = chroma;
            KeyColor = new KeyColor(hue, chroma).Create();
            _isFromCache = true;
        }

        public static int CommonSize => CommonTones.Length;

        public double Hue { get; }
        public double Chroma { get; }
        public Hct KeyColor { get; }

        public List<int> AsList {
            get {
                var result = new List<int>(CommonTones.Length);
                for (var i = 0; i < CommonTones.Length; i++) result.Add(Get(CommonTones[i]));

                return result;
            }
        }

        public bool Equals(TonalPalette other) {
            if (other is null) return false;

            if (!_isFromCache && !other._isFromCache)
                return MathUtils.ApproximatelyEqual(Hue, other.Hue) &&
                       MathUtils.ApproximatelyEqual(Chroma, other.Chroma);

            var thisList = AsList;
            var otherList = other.AsList;
            if (thisList.Count != otherList.Count) return false;

            for (var i = 0; i < thisList.Count; i++) {
                if (thisList[i] != otherList[i]) return false;
            }

            return true;
        }

        public static TonalPalette Of(double hue, double chroma) {
            return new TonalPalette(hue, chroma);
        }

        public static TonalPalette FromHct(Hct hct) {
            return new TonalPalette(hct);
        }

        public static TonalPalette FromList(IReadOnlyList<int> colors) {
            if (colors.Count != CommonSize)
                throw new ArgumentException($"colors must have length {CommonSize}", nameof(colors));

            var cache = new Dictionary<int, int>();
            for (var i = 0; i < CommonTones.Length; i++) cache[CommonTones[i]] = colors[i];

            var bestHue = 0.0;
            var bestChroma = 0.0;

            for (var i = 0; i < colors.Count; i++) {
                var hct = Hct.FromInt(colors[i]);

                if (hct.Tone > 98.0) continue;

                if (hct.Chroma > bestChroma) {
                    bestHue = hct.Hue;
                    bestChroma = hct.Chroma;
                }
            }

            return new TonalPalette(cache, bestHue, bestChroma);
        }

        /// <summary>
        ///     Returns the ARGB representation of an HCT color at the given tone.
        /// </summary>
        public int Get(int tone) {
            if (_cache.TryGetValue(tone, out var value)) return value;

            value = Hct.From(Hue, Chroma, tone).ToInt();
            _cache[tone] = value;
            return value;
        }

        /// <summary>
        ///     Returns the HCT color at the given tone.
        /// </summary>
        public Hct GetHct(double tone) {
            var roundedTone = (int)math.round(tone);
            if (_cache.TryGetValue(roundedTone, out var value)) return Hct.FromInt(value);

            return Hct.From(Hue, Chroma, tone);
        }

        public override bool Equals(object obj) {
            return obj is TonalPalette other && Equals(other);
        }

        public override int GetHashCode() {
            if (!_isFromCache) return HashCode.Combine(Hue, Chroma);

            var hc = new HashCode();
            var list = AsList;
            for (var i = 0; i < list.Count; i++) hc.Add(list[i]);

            return hc.ToHashCode();
        }

        public override string ToString() {
            return !_isFromCache
                ? $"TonalPalette.Of({Hue}, {Chroma})"
                : $"TonalPalette.FromList([{string.Join(", ", AsList)}])";
        }
    }

    /// <summary>
    ///     Key color is a color that represents the hue and chroma of a tonal palette.
    /// </summary>
    public sealed class KeyColor {
        private const double _maxChromaValue = 200.0;

        private readonly Dictionary<int, double> _chromaCache = new();

        public KeyColor(double hue, double requestedChroma) {
            Hue = hue;
            RequestedChroma = requestedChroma;
        }

        public double Hue { get; }
        public double RequestedChroma { get; }

        /// <summary>
        ///     Creates a key color from a hue and a requestedChroma.
        /// </summary>
        public Hct Create() {
            const int pivotTone = 50;
            const int toneStepSize = 1;
            const double epsilon = 0.01;

            var lowerTone = 0;
            var upperTone = 100;

            while (lowerTone < upperTone) {
                var midTone = (lowerTone + upperTone) / 2;
                var isAscending = MaxChroma(midTone) < MaxChroma(midTone + toneStepSize);
                var sufficientChroma = MaxChroma(midTone) >= RequestedChroma - epsilon;

                if (sufficientChroma) {
                    if (math.abs(lowerTone - pivotTone) < math.abs(upperTone - pivotTone)) upperTone = midTone;
                    else {
                        if (lowerTone == midTone) return Hct.From(Hue, RequestedChroma, lowerTone);

                        lowerTone = midTone;
                    }
                } else {
                    if (isAscending) lowerTone = midTone + toneStepSize;
                    else upperTone = midTone;
                }
            }

            return Hct.From(Hue, RequestedChroma, lowerTone);
        }

        private double MaxChroma(int tone) {
            if (_chromaCache.TryGetValue(tone, out var value)) return value;

            value = Hct.From(Hue, _maxChromaValue, tone).Chroma;
            _chromaCache[tone] = value;
            return value;
        }
    }
}
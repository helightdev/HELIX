using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace MaterialColorUtilities {
    /// <summary>
    /// Design utilities using color temperature theory.
    ///
    /// Analogous colors, complementary color, and cache to efficiently, lazily,
    /// generate data for calculations when needed.
    /// </summary>
    public sealed class TemperatureCache {
        public Hct Input { get; }

        private List<Hct> _hctsByTemp = new List<Hct>();
        private List<Hct> _hctsByHue = new List<Hct>();
        private Dictionary<Hct, double> _tempsByHct = new Dictionary<Hct, double>();
        private double _inputRelativeTemperature = -1.0;
        private Hct _complement;

        public TemperatureCache(Hct input) {
            Input = input;
        }

        public Hct Warmest => HctsByTemp[HctsByTemp.Count - 1];
        public Hct Coldest => HctsByTemp[0];

        /// <summary>
        /// A set of colors with differing hues, equidistant in temperature.
        /// </summary>
        public List<Hct> Analogous(int count = 5, int divisions = 12) {
            int startHue = (int)math.round(Input.Hue);
            Hct startHct = HctsByHue[startHue];
            double lastTemp = RelativeTemperature(startHct);
            List<Hct> allColors = new List<Hct> { startHct };

            double absoluteTotalTempDelta = 0.0;
            for (int i = 0; i < 360; i++) {
                int hue = MathUtils.SanitizeDegreesInt(startHue + i);
                Hct hct = HctsByHue[hue];
                double temp = RelativeTemperature(hct);
                double tempDelta = math.abs(temp - lastTemp);
                lastTemp = temp;
                absoluteTotalTempDelta += tempDelta;
            }

            int hueAddend = 1;
            double tempStep = absoluteTotalTempDelta / divisions;
            double totalTempDelta = 0.0;
            lastTemp = RelativeTemperature(startHct);

            while (allColors.Count < divisions) {
                int hue = MathUtils.SanitizeDegreesInt(startHue + hueAddend);
                Hct hct = HctsByHue[hue];
                double temp = RelativeTemperature(hct);
                double tempDelta = math.abs(temp - lastTemp);
                totalTempDelta += tempDelta;

                double desiredTotalTempDeltaForIndex = allColors.Count * tempStep;
                bool indexSatisfied = totalTempDelta >= desiredTotalTempDeltaForIndex;
                int indexAddend = 1;

                while (indexSatisfied && allColors.Count < divisions) {
                    allColors.Add(hct);
                    desiredTotalTempDeltaForIndex = (allColors.Count + indexAddend) * tempStep;
                    indexSatisfied = totalTempDelta >= desiredTotalTempDeltaForIndex;
                    indexAddend++;
                }

                lastTemp = temp;
                hueAddend++;

                if (hueAddend > 360) {
                    while (allColors.Count < divisions) { allColors.Add(hct); }

                    break;
                }
            }

            List<Hct> answers = new List<Hct> { Input };

            int increaseHueCount = (int)math.floor((count - 1) / 2.0);
            for (int i = 1; i < increaseHueCount + 1; i++) {
                int index = -i;
                while (index < 0) { index = allColors.Count + index; }

                if (index >= allColors.Count) { index %= allColors.Count; }

                answers.Insert(0, allColors[index]);
            }

            int decreaseHueCount = count - increaseHueCount - 1;
            for (int i = 1; i < decreaseHueCount + 1; i++) {
                int index = i;
                while (index < 0) { index = allColors.Count + index; }

                if (index >= allColors.Count) { index %= allColors.Count; }

                answers.Add(allColors[index]);
            }

            return answers;
        }

        /// <summary>
        /// A color that complements the input color aesthetically.
        /// </summary>
        public Hct Complement {
            get {
                if (_complement != null) { return _complement; }

                double coldestHue = Coldest.Hue;
                double coldestTemp = TempsByHct[Coldest];

                double warmestHue = Warmest.Hue;
                double warmestTemp = TempsByHct[Warmest];
                double range = warmestTemp - coldestTemp;

                bool startHueIsColdestToWarmest = IsBetween(Input.Hue, coldestHue, warmestHue);
                double startHue = startHueIsColdestToWarmest ? warmestHue : coldestHue;
                double endHue = startHueIsColdestToWarmest ? coldestHue : warmestHue;

                const double directionOfRotation = 1.0;
                double smallestError = 1000.0;
                Hct answer = HctsByHue[(int)math.round(Input.Hue)];

                double complementRelativeTemp = 1.0 - InputRelativeTemperature;

                for (double hueAddend = 0.0; hueAddend <= 360.0; hueAddend += 1.0) {
                    double hue = MathUtils.SanitizeDegreesDouble(startHue + (directionOfRotation * hueAddend));
                    if (!IsBetween(hue, startHue, endHue)) { continue; }

                    Hct possibleAnswer = HctsByHue[(int)math.round(hue)];
                    double relativeTemp = (_tempsByHct[possibleAnswer] - coldestTemp) / range;
                    double error = math.abs(complementRelativeTemp - relativeTemp);

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
        /// Temperature relative to all colors with the same chroma and tone.
        /// Value on a scale from 0 to 1.
        /// </summary>
        public double RelativeTemperature(Hct hct) {
            double range = TempsByHct[Warmest] - TempsByHct[Coldest];
            double differenceFromColdest = TempsByHct[hct] - TempsByHct[Coldest];

            if (range == 0.0) { return 0.5; }

            return differenceFromColdest / range;
        }

        /// <summary>
        /// Relative temperature of the input color.
        /// </summary>
        public double InputRelativeTemperature {
            get {
                if (_inputRelativeTemperature >= 0.0) { return _inputRelativeTemperature; }

                double coldestTemp = TempsByHct[Coldest];
                double range = TempsByHct[Warmest] - coldestTemp;
                double differenceFromColdest = TempsByHct[Input] - coldestTemp;

                _inputRelativeTemperature = range == 0.0 ? 0.5 : differenceFromColdest / range;
                return _inputRelativeTemperature;
            }
        }

        /// <summary>
        /// HCTs for all hues, with the same chroma/tone as the input.
        /// Sorted from coldest first to warmest last.
        /// </summary>
        public List<Hct> HctsByTemp {
            get {
                if (_hctsByTemp.Count > 0) { return _hctsByTemp; }

                List<Hct> hcts = new List<Hct>(HctsByHue);
                hcts.Add(Input);

                Dictionary<Hct, double> temperaturesByHct = TempsByHct;
                hcts.Sort((a, b) => temperaturesByHct[a].CompareTo(temperaturesByHct[b]));

                _hctsByTemp = hcts;
                return _hctsByTemp;
            }
        }

        /// <summary>
        /// A map with keys of HCTs in HctsByTemp, values of raw temperature.
        /// </summary>
        public Dictionary<Hct, double> TempsByHct {
            get {
                if (_tempsByHct.Count > 0) { return _tempsByHct; }

                List<Hct> allHcts = new List<Hct>(HctsByHue);
                allHcts.Add(Input);

                Dictionary<Hct, double> temperaturesByHct = new Dictionary<Hct, double>();
                foreach (Hct hct in allHcts) { temperaturesByHct[hct] = RawTemperature(hct); }

                _tempsByHct = temperaturesByHct;
                return _tempsByHct;
            }
        }

        /// <summary>
        /// HCTs for all hues, with the same chroma/tone as the input.
        /// Sorted ascending, hue 0 to 360.
        /// </summary>
        public List<Hct> HctsByHue {
            get {
                if (_hctsByHue.Count > 0) { return _hctsByHue; }

                List<Hct> hcts = new List<Hct>();
                for (double hue = 0.0; hue <= 360.0; hue += 1.0) { hcts.Add(Hct.From(hue, Input.Chroma, Input.Tone)); }

                _hctsByHue = hcts;
                return _hctsByHue;
            }
        }

        /// <summary>
        /// Determines if an angle is between two other angles, rotating clockwise.
        /// </summary>
        public static bool IsBetween(double angle, double a, double b) {
            if (a < b) { return a <= angle && angle <= b; }

            return a <= angle || angle <= b;
        }

        /// <summary>
        /// Value representing cool-warm factor of a color.
        /// Values below 0 are considered cool, above 0 warm.
        /// </summary>
        public static double RawTemperature(Hct color) {
            double3 lab = ColorUtils.LabFromArgb(color.ToInt());

            double hue = MathUtils.SanitizeDegreesDouble(math.degrees(math.atan2(lab.z, lab.y)));

            double chroma = math.sqrt((lab.y * lab.y) + (lab.z * lab.z));

            return -0.5
                 + 0.02 * math.pow(chroma, 1.07)
                        * math.cos(math.radians(MathUtils.SanitizeDegreesDouble(hue - 50.0)));
        }
    }

    /// <summary>
    /// Functions for blending in HCT and CAM16.
    /// </summary>
    public static class Blend {
        /// <summary>
        /// Blend the design color's HCT hue towards the key color's HCT hue.
        /// </summary>
        public static int Harmonize(int designColor, int sourceColor) {
            Hct fromHct = Hct.FromInt(designColor);
            Hct toHct = Hct.FromInt(sourceColor);

            double differenceDegrees = MathUtils.DifferenceDegrees(fromHct.Hue, toHct.Hue);
            double rotationDegrees = math.min(differenceDegrees * 0.5, 15.0);

            double outputHue = MathUtils.SanitizeDegreesDouble(
                fromHct.Hue
              + rotationDegrees * MathUtils.RotationDirection(fromHct.Hue, toHct.Hue)
            );

            return Hct.From(outputHue, fromHct.Chroma, fromHct.Tone).ToInt();
        }

        /// <summary>
        /// Blends hue from one color into another. Chroma and tone of the original are maintained.
        /// </summary>
        public static int HctHue(int from, int to, double amount) {
            int ucs = Cam16Ucs(from, to, amount);
            Cam16 ucsCam = Cam16.FromInt(ucs);
            Cam16 fromCam = Cam16.FromInt(from);

            Hct blended = Hct.From(
                ucsCam.Hue,
                fromCam.Chroma,
                ColorUtils.LstarFromArgb(from)
            );

            return blended.ToInt();
        }

        /// <summary>
        /// Blend in CAM16-UCS space.
        /// </summary>
        public static int Cam16Ucs(int from, int to, double amount) {
            Cam16 fromCam = Cam16.FromInt(from);
            Cam16 toCam = Cam16.FromInt(to);

            double jstar = fromCam.Jstar + ((toCam.Jstar - fromCam.Jstar) * amount);
            double astar = fromCam.Astar + ((toCam.Astar - fromCam.Astar) * amount);
            double bstar = fromCam.Bstar + ((toCam.Bstar - fromCam.Bstar) * amount);

            return Cam16.FromUcs(jstar, astar, bstar).ToInt();
        }
    }

    /// <summary>
    /// Utility methods for calculating contrast given two colors,
    /// or calculating a color given one color and a contrast ratio.
    /// </summary>
    public static class Contrast {
        /// <summary>
        /// Returns a contrast ratio, which ranges from 1 to 21.
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
            double lighter = y1 > y2 ? y1 : y2;
            double darker = lighter == y2 ? y1 : y2;
            return (lighter + 5.0) / (darker + 5.0);
        }

        /// <summary>
        /// Returns a tone &gt;= tone that ensures ratio.
        /// Returns -1 if ratio cannot be achieved.
        /// </summary>
        public static double Lighter(double tone, double ratio) {
            if (tone < 0.0 || tone > 100.0) { return -1.0; }

            double darkY = ColorUtils.YFromLstar(tone);
            double lightY = ratio * (darkY + 5.0) - 5.0;
            double realContrast = RatioOfYs(lightY, darkY);
            double delta = math.abs(realContrast - ratio);

            if (realContrast < ratio && delta > 0.04) { return -1.0; }

            double returnValue = ColorUtils.LstarFromY(lightY) + 0.4;
            if (returnValue < 0.0 || returnValue > 100.0) { return -1.0; }

            return returnValue;
        }

        /// <summary>
        /// Returns a tone &lt;= tone that ensures ratio.
        /// Returns -1 if ratio cannot be achieved.
        /// </summary>
        public static double Darker(double tone, double ratio) {
            if (tone < 0.0 || tone > 100.0) { return -1.0; }

            double lightY = ColorUtils.YFromLstar(tone);
            double darkY = ((lightY + 5.0) / ratio) - 5.0;
            double realContrast = RatioOfYs(lightY, darkY);
            double delta = math.abs(realContrast - ratio);

            if (realContrast < ratio && delta > 0.04) { return -1.0; }

            double returnValue = ColorUtils.LstarFromY(darkY) - 0.4;
            if (returnValue < 0.0 || returnValue > 100.0) { return -1.0; }

            return returnValue;
        }

        /// <summary>
        /// Unsafe lighter version. Returns 100 if ratio cannot be achieved.
        /// </summary>
        public static double LighterUnsafe(double tone, double ratio) {
            double lighterSafe = Lighter(tone, ratio);
            return lighterSafe < 0.0 ? 100.0 : lighterSafe;
        }

        /// <summary>
        /// Unsafe darker version. Returns 0 if ratio cannot be achieved.
        /// </summary>
        public static double DarkerUnsafe(double tone, double ratio) {
            double darkerSafe = Darker(tone, ratio);
            return darkerSafe < 0.0 ? 0.0 : darkerSafe;
        }
    }

    /// <summary>
    /// Check and/or fix universally disliked colors.
    /// </summary>
    public static class DislikeAnalyzer {
        /// <summary>
        /// Returns true if hct is disliked.
        /// </summary>
        public static bool IsDisliked(Hct hct) {
            bool huePasses = math.round(hct.Hue) >= 90.0 && math.round(hct.Hue) <= 111.0;
            bool chromaPasses = math.round(hct.Chroma) > 16.0;
            bool tonePasses = math.round(hct.Tone) < 65.0;

            return huePasses && chromaPasses && tonePasses;
        }

        /// <summary>
        /// If hct is disliked, lighten it to make it likable.
        /// </summary>
        public static Hct FixIfDisliked(Hct hct) {
            if (IsDisliked(hct)) { return Hct.From(hct.Hue, hct.Chroma, 70.0); }

            return hct;
        }
    }

    /// <summary>
    /// Comprises foundational palettes to build a color scheme.
    /// </summary>
    public sealed class CorePalettes {
        public TonalPalette Primary { get; }
        public TonalPalette Secondary { get; }
        public TonalPalette Tertiary { get; }
        public TonalPalette Neutral { get; }
        public TonalPalette NeutralVariant { get; }

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
    }

    /// <summary>
    /// A convenience class for retrieving colors that are constant in hue and
    /// chroma, but vary in tone.
    /// </summary>
    public sealed class TonalPalette : IEquatable<TonalPalette> {
        public static readonly int[] CommonTones = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 95, 99, 100 };

        public static int CommonSize => CommonTones.Length;

        public double Hue { get; }
        public double Chroma { get; }
        public Hct KeyColor { get; }

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

        public static TonalPalette Of(double hue, double chroma) {
            return new TonalPalette(hue, chroma);
        }

        public static TonalPalette FromHct(Hct hct) {
            return new TonalPalette(hct);
        }

        public static TonalPalette FromList(IReadOnlyList<int> colors) {
            if (colors.Count != CommonSize) {
                throw new ArgumentException($"colors must have length {CommonSize}", nameof(colors));
            }

            Dictionary<int, int> cache = new Dictionary<int, int>();
            for (int i = 0; i < CommonTones.Length; i++) { cache[CommonTones[i]] = colors[i]; }

            double bestHue = 0.0;
            double bestChroma = 0.0;

            for (int i = 0; i < colors.Count; i++) {
                Hct hct = Hct.FromInt(colors[i]);

                if (hct.Tone > 98.0) { continue; }

                if (hct.Chroma > bestChroma) {
                    bestHue = hct.Hue;
                    bestChroma = hct.Chroma;
                }
            }

            return new TonalPalette(cache, bestHue, bestChroma);
        }

        public List<int> AsList {
            get {
                List<int> result = new List<int>(CommonTones.Length);
                for (int i = 0; i < CommonTones.Length; i++) { result.Add(Get(CommonTones[i])); }

                return result;
            }
        }

        /// <summary>
        /// Returns the ARGB representation of an HCT color at the given tone.
        /// </summary>
        public int Get(int tone) {
            if (_cache.TryGetValue(tone, out int value)) { return value; }

            value = Hct.From(Hue, Chroma, tone).ToInt();
            _cache[tone] = value;
            return value;
        }

        /// <summary>
        /// Returns the HCT color at the given tone.
        /// </summary>
        public Hct GetHct(double tone) {
            int roundedTone = (int)math.round(tone);
            if (_cache.TryGetValue(roundedTone, out int value)) { return Hct.FromInt(value); }

            return Hct.From(Hue, Chroma, tone);
        }

        public bool Equals(TonalPalette other) {
            if (other is null) { return false; }

            if (!_isFromCache && !other._isFromCache) { return Hue == other.Hue && Chroma == other.Chroma; }

            List<int> thisList = AsList;
            List<int> otherList = other.AsList;
            if (thisList.Count != otherList.Count) { return false; }

            for (int i = 0; i < thisList.Count; i++) {
                if (thisList[i] != otherList[i]) { return false; }
            }

            return true;
        }

        public override bool Equals(object obj) {
            return obj is TonalPalette other && Equals(other);
        }

        public override int GetHashCode() {
            if (!_isFromCache) { return HashCode.Combine(Hue, Chroma); }

            HashCode hc = new HashCode();
            List<int> list = AsList;
            for (int i = 0; i < list.Count; i++) { hc.Add(list[i]); }

            return hc.ToHashCode();
        }

        public override string ToString() {
            return !_isFromCache
                ? $"TonalPalette.Of({Hue}, {Chroma})"
                : $"TonalPalette.FromList([{string.Join(", ", AsList)}])";
        }
    }

    /// <summary>
    /// Key color is a color that represents the hue and chroma of a tonal palette.
    /// </summary>
    public sealed class KeyColor {
        public double Hue { get; }
        public double RequestedChroma { get; }

        private readonly Dictionary<int, double> _chromaCache = new Dictionary<int, double>();
        private const double MaxChromaValue = 200.0;

        public KeyColor(double hue, double requestedChroma) {
            Hue = hue;
            RequestedChroma = requestedChroma;
        }

        /// <summary>
        /// Creates a key color from a hue and a requestedChroma.
        /// </summary>
        public Hct Create() {
            const int pivotTone = 50;
            const int toneStepSize = 1;
            const double epsilon = 0.01;

            int lowerTone = 0;
            int upperTone = 100;

            while (lowerTone < upperTone) {
                int midTone = (lowerTone + upperTone) / 2;
                bool isAscending = MaxChroma(midTone) < MaxChroma(midTone + toneStepSize);
                bool sufficientChroma = MaxChroma(midTone) >= RequestedChroma - epsilon;

                if (sufficientChroma) {
                    if (math.abs(lowerTone - pivotTone) < math.abs(upperTone - pivotTone)) {
                        upperTone = midTone;
                    } else {
                        if (lowerTone == midTone) { return Hct.From(Hue, RequestedChroma, lowerTone); }

                        lowerTone = midTone;
                    }
                } else {
                    if (isAscending) { lowerTone = midTone + toneStepSize; } else { upperTone = midTone; }
                }
            }

            return Hct.From(Hue, RequestedChroma, lowerTone);
        }

        private double MaxChroma(int tone) {
            if (_chromaCache.TryGetValue(tone, out double value)) { return value; }

            value = Hct.From(Hue, MaxChromaValue, tone).Chroma;
            _chromaCache[tone] = value;
            return value;
        }
    }
}
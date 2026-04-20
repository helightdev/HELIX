using System.Collections.Generic;
using Unity.Mathematics;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   Design utilities using color temperature theory.
    ///   Analogous colors, complementary color, and cache to efficiently, lazily,
    ///   generate data for calculations when needed.
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
        ///   A color that complements the input color aesthetically.
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
        ///   Relative temperature of the input color.
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
        ///   HCTs for all hues, with the same chroma/tone as the input.
        ///   Sorted from coldest first to warmest last.
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
        ///   A map with keys of HCTs in HctsByTemp, values of raw temperature.
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
        ///   HCTs for all hues, with the same chroma/tone as the input.
        ///   Sorted ascending, hue 0 to 360.
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
        ///   A set of colors with differing hues, equidistant in temperature.
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
        ///   Temperature relative to all colors with the same chroma and tone.
        ///   Value on a scale from 0 to 1.
        /// </summary>
        public double RelativeTemperature(Hct hct) {
      var range = TempsByHct[Warmest] - TempsByHct[Coldest];
      var differenceFromColdest = TempsByHct[hct] - TempsByHct[Coldest];

      if (range == 0.0) return 0.5;

      return differenceFromColdest / range;
    }

        /// <summary>
        ///   Determines if an angle is between two other angles, rotating clockwise.
        /// </summary>
        public static bool IsBetween(double angle, double a, double b) {
      if (a < b) return a <= angle && angle <= b;

      return a <= angle || angle <= b;
    }

        /// <summary>
        ///   Value representing cool-warm factor of a color.
        ///   Values below 0 are considered cool, above 0 warm.
        /// </summary>
        public static double RawTemperature(Hct color) {
      var lab = MaterialColorUtils.LabFromArgb(color.ToInt());

      var hue = MathUtils.SanitizeDegreesDouble(math.degrees(math.atan2(lab.z, lab.y)));

      var chroma = math.sqrt(lab.y * lab.y + lab.z * lab.z);

      return -0.5
             + 0.02 * math.pow(chroma, 1.07)
                    * math.cos(math.radians(MathUtils.SanitizeDegreesDouble(hue - 50.0)));
    }
  }
}
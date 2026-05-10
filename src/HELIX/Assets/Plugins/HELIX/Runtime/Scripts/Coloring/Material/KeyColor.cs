using System.Collections.Generic;
using Unity.Mathematics;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   Key color is a color that represents the hue and chroma of a tonal palette.
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
        ///   Creates a key color from a hue and a requestedChroma.
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
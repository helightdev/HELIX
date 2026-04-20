using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   A convenience class for retrieving colors that are constant in hue and
    ///   chroma, but vary in tone.
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

      if (!_isFromCache && !other._isFromCache) {
        return MathUtils.ApproximatelyEqual(Hue, other.Hue) &&
               MathUtils.ApproximatelyEqual(Chroma, other.Chroma);
      }

      var thisList = AsList;
      var otherList = other.AsList;
      if (thisList.Count != otherList.Count) return false;

      for (var i = 0; i < thisList.Count; i++)
        if (thisList[i] != otherList[i])
          return false;

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
        ///   Returns the ARGB representation of an HCT color at the given tone.
        /// </summary>
        public int Get(int tone) {
      if (_cache.TryGetValue(tone, out var value)) return value;

      value = Hct.From(Hue, Chroma, tone).ToInt();
      _cache[tone] = value;
      return value;
    }

        /// <summary>
        ///   Returns the HCT color at the given tone.
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
}
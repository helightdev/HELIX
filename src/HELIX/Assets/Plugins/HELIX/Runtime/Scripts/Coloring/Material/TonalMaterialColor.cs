using System;
using UnityEngine;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   Generated swatch backed by a tonal palette.
    ///   This replaces the old MaterialColor wrapper behavior.
    /// </summary>
    public sealed class TonalMaterialColor : MaterialColor {
    public TonalMaterialColor(int argb, string name = null)
      : this(Hct.FromInt(argb), name) { }

    public TonalMaterialColor(uint argb, string name = null)
      : this(unchecked((int)argb), name) { }

    public TonalMaterialColor(Hct hct, string name = null)
      : this(TonalPalette.FromHct(hct), hct, name) { }

    public TonalMaterialColor(TonalPalette tonalPalette, string name = null)
      : this(tonalPalette, tonalPalette?.KeyColor, name) { }

    private TonalMaterialColor(TonalPalette tonalPalette, Hct seed, string name = null)
      : base(tonalPalette?.Get(500) ?? 0, name) {
      Palette = tonalPalette ?? throw new ArgumentNullException(nameof(tonalPalette));
      Seed = seed ?? tonalPalette.KeyColor;
    }

    public TonalPalette Palette { get; }
    public Hct Seed { get; }

    public override Color this[int weight] => Palette.GetHct(WeightToTone(weight)).ToColor();

        /// <summary>
        ///   Returns a swatch for arbitrary HCT tone values.
        /// </summary>
        public Color[] ToToneSwatch(params double[] tones) {
      if (tones == null || tones.Length == 0) return ToSwatch();

      var colors = new Color[tones.Length];
      for (var i = 0; i < tones.Length; i++) colors[i] = Palette.GetHct(tones[i]).ToColor();
      return colors;
    }

    public static implicit operator Hct(TonalMaterialColor materialColor) {
      return materialColor.Seed;
    }

    public static implicit operator TonalPalette(TonalMaterialColor materialColor) {
      return materialColor.Palette;
    }

    private static double WeightToTone(int weight) {
      return weight switch {
        50 => 95.0,
        100 => 90.0,
        200 => 80.0,
        300 => 70.0,
        400 => 60.0,
        500 => 50.0,
        600 => 40.0,
        700 => 30.0,
        800 => 20.0,
        900 => 10.0,
        _ => (1000 - weight) / 10.0
      };
    }
  }
}
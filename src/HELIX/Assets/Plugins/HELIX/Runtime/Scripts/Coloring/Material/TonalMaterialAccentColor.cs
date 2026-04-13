using System;
using UnityEngine;

namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     Generated accent swatch backed by a tonal palette.
    ///     Useful if you want accent-style indexing but still generated from HCT.
    /// </summary>
    public sealed class TonalMaterialAccentColor : MaterialAccentColor {
        public TonalMaterialAccentColor(int argb, string name = null)
            : this(Hct.FromInt(argb), name) { }

        public TonalMaterialAccentColor(uint argb, string name = null)
            : this(unchecked((int)argb), name) { }

        public TonalMaterialAccentColor(Hct hct, string name = null)
            : this(TonalPalette.FromHct(hct), hct, name) { }

        public TonalMaterialAccentColor(TonalPalette tonalPalette, string name = null)
            : this(tonalPalette, tonalPalette?.KeyColor, name) { }

        private TonalMaterialAccentColor(TonalPalette tonalPalette, Hct seed, string name = null)
            : base(tonalPalette?.Get(80) ?? 0, name) {
            Palette = tonalPalette ?? throw new ArgumentNullException(nameof(tonalPalette));
            Seed = seed ?? tonalPalette.KeyColor;
        }

        public TonalPalette Palette { get; }
        public Hct Seed { get; }

        public override Color this[int weight] => Palette.GetHct(AccentWeightToTone(weight)).ToColor();

        public static implicit operator Hct(TonalMaterialAccentColor materialColor) {
            return materialColor.Seed;
        }

        public static implicit operator TonalPalette(TonalMaterialAccentColor materialColor) {
            return materialColor.Palette;
        }

        private static double AccentWeightToTone(int weight) {
            return weight switch {
                100 => 90.0,
                200 => 80.0,
                400 => 40.0,
                700 => 20.0,
                _   => throw new ArgumentOutOfRangeException(nameof(weight), $"Invalid accent weight {weight}. Expected 100, 200, 400, or 700.")
            };
        }
    }
}
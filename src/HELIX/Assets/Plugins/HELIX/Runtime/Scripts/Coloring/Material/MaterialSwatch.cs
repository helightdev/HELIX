using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   Base type for color swatches.
    ///   Supports a primary value, indexed shades, and conversion to Unity colors.
    /// </summary>
    public abstract class MaterialSwatch {
    public readonly string name;

    protected MaterialSwatch(int primaryArgb, string name = null) {
      PrimaryArgb = primaryArgb;
      this.name = name ?? string.Empty;
    }

    protected MaterialSwatch(uint primaryArgb, string name = null) : this(unchecked((int)primaryArgb), name) { }

        /// <summary>
        ///   Primary ARGB value of the swatch. For normal swatches this is typically shade 500.
        ///   For accent swatches this is typically shade 200.
        /// </summary>
        public int PrimaryArgb { get; }

    public Color Value => PrimaryArgb.ArgbToColor();
    public StyleColor StyleValue => Value;

        /// <summary>
        ///   Ordered list of valid shade weights for this swatch.
        /// </summary>
        public abstract ReadOnlySpan<int> Weights { get; }

        /// <summary>
        ///   Resolves a shade by weight.
        /// </summary>
        public abstract Color this[int weight] { get; }

        /// <summary>
        ///   Resolves a shade ARGB by weight.
        /// </summary>
        public virtual int GetArgb(int weight) {
      return this[weight].ToArgb();
    }

        /// <summary>
        ///   Returns all shades in the natural order of this swatch type.
        /// </summary>
        public Color[] ToSwatch() {
      var weights = Weights;
      var colors = new Color[weights.Length];
      for (var i = 0; i < weights.Length; i++) colors[i] = this[weights[i]];
      return colors;
    }

        /// <summary>
        ///   Returns shades for arbitrary weights.
        /// </summary>
        public Color[] ToSwatch(params int[] weights) {
      if (weights == null || weights.Length == 0) return ToSwatch();

      var colors = new Color[weights.Length];
      for (var i = 0; i < weights.Length; i++) colors[i] = this[weights[i]];
      return colors;
    }

    public override string ToString() {
      return string.IsNullOrEmpty(name)
        ? $"{GetType().Name}({PrimaryArgb.ToIntArgbHex()})"
        : $"{GetType().Name}(Name=\"{name}\", Value={PrimaryArgb.ToIntArgbHex()})";
    }

    public static implicit operator Color(MaterialSwatch swatch) {
      return swatch.Value;
    }

    public static implicit operator StyleColor(MaterialSwatch swatch) {
      return swatch.Value;
    }
  }
}
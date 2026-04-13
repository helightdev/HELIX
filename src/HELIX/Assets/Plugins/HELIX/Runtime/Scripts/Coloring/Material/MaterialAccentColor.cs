using System;
using UnityEngine;

namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     Base class for accent swatches with weights:
    ///     100, 200, 400, 700.
    /// </summary>
    public abstract class MaterialAccentColor : MaterialSwatch {
        private static readonly int[] _weights = { 100, 200, 400, 700 };

        protected MaterialAccentColor(int primaryArgb, string name = null) : base(primaryArgb, name) { }
        protected MaterialAccentColor(uint primaryArgb, string name = null) : base(primaryArgb, name) { }

        public override ReadOnlySpan<int> Weights => _weights;

        public Color Shade100 => this[100];
        public Color Shade200 => this[200];
        public Color Shade400 => this[400];
        public Color Shade700 => this[700];
    }
}
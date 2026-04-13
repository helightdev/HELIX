using System;
using UnityEngine;

namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     Base class for standard Material-style swatches with weights:
    ///     50, 100, 200, 300, 400, 500, 600, 700, 800, 900.
    /// </summary>
    public abstract class MaterialColor : MaterialSwatch {
        private static readonly int[] _weights = { 50, 100, 200, 300, 400, 500, 600, 700, 800, 900 };

        protected MaterialColor(int primaryArgb, string name = null) : base(primaryArgb, name) { }
        protected MaterialColor(uint primaryArgb, string name = null) : base(primaryArgb, name) { }

        public override ReadOnlySpan<int> Weights => _weights;

        public Color Shade50 => this[50];
        public Color Shade100 => this[100];
        public Color Shade200 => this[200];
        public Color Shade300 => this[300];
        public Color Shade400 => this[400];
        public Color Shade500 => this[500];
        public Color Shade600 => this[600];
        public Color Shade700 => this[700];
        public Color Shade800 => this[800];
        public Color Shade900 => this[900];
    }
}
using System;
using UnityEngine;

namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     Exact pre-created accent swatch.
    ///     Use this when you need to match Flutter accent tables exactly.
    /// </summary>
    public sealed class FixedMaterialAccentColor : MaterialAccentColor {
        private readonly int[] _argbs;

        public FixedMaterialAccentColor(
            int primaryArgb,
            int shade100,
            int shade200,
            int shade400,
            int shade700,
            string name = null
        ) : base(primaryArgb, name) {
            _argbs = new[] { shade100, shade200, shade400, shade700 };
        }

        public FixedMaterialAccentColor(
            uint primaryArgb,
            uint shade100,
            uint shade200,
            uint shade400,
            uint shade700,
            string name = null
        ) : this(
            unchecked((int)primaryArgb),
            unchecked((int)shade100),
            unchecked((int)shade200),
            unchecked((int)shade400),
            unchecked((int)shade700),
            name
        ) { }

        public override Color this[int weight] => GetArgb(weight).ArgbToColor();

        public override int GetArgb(int weight) {
            return weight switch {
                100 => _argbs[0],
                200 => _argbs[1],
                400 => _argbs[2],
                700 => _argbs[3],
                _   => throw new ArgumentOutOfRangeException(nameof(weight), $"Invalid accent swatch weight {weight}.")
            };
        }
    }
}
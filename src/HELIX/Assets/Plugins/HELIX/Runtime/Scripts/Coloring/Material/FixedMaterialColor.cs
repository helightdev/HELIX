using System;
using UnityEngine;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///   Exact pre-created standard swatch.
    ///   Use this when you need to match Flutter's hard-coded tables exactly.
    /// </summary>
    public sealed class FixedMaterialColor : MaterialColor {
    private readonly int[] _argbs;

    public FixedMaterialColor(
      int primaryArgb,
      int shade50,
      int shade100,
      int shade200,
      int shade300,
      int shade400,
      int shade500,
      int shade600,
      int shade700,
      int shade800,
      int shade900,
      string name = null
    ) : base(primaryArgb, name) {
      _argbs = new[] {
        shade50, shade100, shade200, shade300, shade400,
        shade500, shade600, shade700, shade800, shade900
      };
    }

    public FixedMaterialColor(
      uint primaryArgb,
      uint shade50,
      uint shade100,
      uint shade200,
      uint shade300,
      uint shade400,
      uint shade500,
      uint shade600,
      uint shade700,
      uint shade800,
      uint shade900,
      string name = null
    ) : this(
      unchecked((int)primaryArgb),
      unchecked((int)shade50),
      unchecked((int)shade100),
      unchecked((int)shade200),
      unchecked((int)shade300),
      unchecked((int)shade400),
      unchecked((int)shade500),
      unchecked((int)shade600),
      unchecked((int)shade700),
      unchecked((int)shade800),
      unchecked((int)shade900),
      name
    ) { }

    public override Color this[int weight] => GetArgb(weight).ArgbToColor();

    public override int GetArgb(int weight) {
      return weight switch {
        50 => _argbs[0],
        100 => _argbs[1],
        200 => _argbs[2],
        300 => _argbs[3],
        400 => _argbs[4],
        500 => _argbs[5],
        600 => _argbs[6],
        700 => _argbs[7],
        800 => _argbs[8],
        900 => _argbs[9],
        _ => throw new ArgumentOutOfRangeException(nameof(weight), $"Invalid swatch weight {weight}.")
      };
    }
  }
}
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
  public static class LayoutExtensions {
    public static float LayoutSimple(this StyleLength length, float available, Vector2 constraints) {
      return Mathf.Clamp(
        length.keyword switch {
          StyleKeyword.Auto or StyleKeyword.Initial => available,
          StyleKeyword.Undefined => length.value.unit switch {
            LengthUnit.Pixel => length.value.value,
            LengthUnit.Percent => length.value.value / 100f * available,
            _ => 0
          },
          _ => 0
        },
        Mathf.Approximately(constraints.x, -1) ? float.NegativeInfinity : constraints.x,
        Mathf.Approximately(constraints.y, -1) ? float.PositiveInfinity : constraints.y
      );
    }

    public static float LayoutSimple(this StyleLength length, float available) {
      return length.LayoutSimple(available, new Vector2(-1, -1));
    }

    public static Rect LayoutSimple(
      this Rect rect,
      Alignment alignment,
      StyleLength width,
      StyleLength height,
      Vector2 widthConstraints,
      Vector2 heightConstraints
    ) {
      var computedWidth = width.LayoutSimple(rect.width, widthConstraints);
      var computedHeight = height.LayoutSimple(rect.height, heightConstraints);
      var size = new Vector2(computedWidth, computedHeight);
      var leeway = rect.size - size;
      var coefficients = alignment.GetOffsetCoefficients();
      var offset = new Vector2(leeway.x * coefficients.x, leeway.y * coefficients.y);
      return new Rect(offset + rect.position, size);
    }

    public static Rect LayoutSimple(
      this Rect rect,
      Alignment alignment,
      StyleLength width,
      StyleLength height
    ) {
      return rect.LayoutSimple(alignment, width, height, new Vector2(-1, -1), new Vector2(-1, -1));
    }
  }
}
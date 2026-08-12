using System;
using UnityEngine.UIElements;

namespace HELIX.Types {
  public struct BorderRadius : IEquatable<BorderRadius>, IStyleLength4 {
    public StyleLength topLeft;
    public StyleLength topRight;
    public StyleLength bottomRight;
    public StyleLength bottomLeft;

    public BorderRadius(
      StyleLength topLeft,
      StyleLength topRight,
      StyleLength bottomRight,
      StyleLength bottomLeft
    ) {
      this.topLeft = topLeft;
      this.topRight = topRight;
      this.bottomRight = bottomRight;
      this.bottomLeft = bottomLeft;
    }

    public readonly void Apply(VisualElement element) {
      element.style.borderTopLeftRadius = topLeft;
      element.style.borderTopRightRadius = topRight;
      element.style.borderBottomRightRadius = bottomRight;
      element.style.borderBottomLeftRadius = bottomLeft;
    }

    public bool Equals(BorderRadius other) {
      return topLeft.Equals(other.topLeft) && topRight.Equals(other.topRight) &&
             bottomRight.Equals(other.bottomRight) && bottomLeft.Equals(other.bottomLeft);
    }

    public override bool Equals(object obj) {
      return obj is BorderRadius other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(topLeft, topRight, bottomRight, bottomLeft);
    }

    public override string ToString() {
      return HelixFormattingHelper.BuildQuadruple(
        "BorderRadius",
        topLeft.FormatStyleValue(),
        topRight.FormatStyleValue(),
        bottomRight.FormatStyleValue(),
        bottomLeft.FormatStyleValue(),
        showNames: false
      );
    }

    public StyleLength4 ToStyleLength4() {
      return this;
    }

    public static implicit operator BorderRadius(StyleLength4 sl4) {
      return new BorderRadius(sl4.l, sl4.t, sl4.r, sl4.b);
    }

    public static implicit operator StyleLength4(BorderRadius br) {
      return new StyleLength4(br.topLeft, br.topRight, br.bottomRight, br.bottomLeft);
    }

    public static implicit operator BorderRadius(float v) {
      return new BorderRadius(v, v, v, v);
    }

    public static BorderRadius All(StyleLength radius) {
      return new BorderRadius(radius, radius, radius, radius);
    }

    public static BorderRadius Horizontal(StyleLength left, StyleLength right) {
      return new BorderRadius(left, right, right, left);
    }

    public static BorderRadius Vertical(StyleLength top, StyleLength bottom) {
      return new BorderRadius(top, top, bottom, bottom);
    }

    public static BorderRadius Only(
      StyleLength? topLeft = null,
      StyleLength? topRight = null,
      StyleLength? bottomRight = null,
      StyleLength? bottomLeft = null
    ) {
      return new BorderRadius(
        topLeft.GetValueOrDefault(0),
        topRight.GetValueOrDefault(0),
        bottomRight.GetValueOrDefault(0),
        bottomLeft.GetValueOrDefault(0)
      );
    }

    public static readonly BorderRadius None = new(0, 0, 0, 0);

    public static readonly BorderRadius Initial = new(
      StyleKeyword.Initial,
      StyleKeyword.Initial,
      StyleKeyword.Initial,
      StyleKeyword.Initial
    );
  }
}
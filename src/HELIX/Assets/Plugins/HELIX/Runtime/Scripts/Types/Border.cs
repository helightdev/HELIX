using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Types {
  public struct Border : IEquatable<Border> {
    public BorderSide left;
    public BorderSide top;
    public BorderSide right;
    public BorderSide bottom;

    public Border(BorderSide left, BorderSide top, BorderSide right, BorderSide bottom) {
      this.left = left;
      this.top = top;
      this.right = right;
      this.bottom = bottom;
    }

    public readonly void Apply(VisualElement element) {
      element.style.borderLeftWidth = left.width;
      element.style.borderLeftColor = left.color;
      element.style.borderTopWidth = top.width;
      element.style.borderTopColor = top.color;
      element.style.borderRightWidth = right.width;
      element.style.borderRightColor = right.color;
      element.style.borderBottomWidth = bottom.width;
      element.style.borderBottomColor = bottom.color;
    }

    public bool Equals(Border other) {
      return left.Equals(other.left) && top.Equals(other.top) && right.Equals(other.right) &&
             bottom.Equals(other.bottom);
    }

    public override bool Equals(object obj) {
      return obj is Border other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(left, top, right, bottom);
    }

    public override string ToString() {
      return HelixFormattingHelper.BuildQuadruple("Border", left, top, right, bottom);
    }

    public static Border All(float width, Color color) {
      var side = new BorderSide(width, color);
      return new Border(side, side, side, side);
    }

    public static Border All(BorderSide side) {
      return new Border(side, side, side, side);
    }

    public static Border Symmetric(BorderSide horizontal, BorderSide vertical) {
      return new Border(horizontal, vertical, horizontal, vertical);
    }

    public static Border Only(
      BorderSide? left = null,
      BorderSide? top = null,
      BorderSide? right = null,
      BorderSide? bottom = null
    ) {
      return new Border(
        left.GetValueOrDefault(new BorderSide(0, default)),
        top.GetValueOrDefault(new BorderSide(0, default)),
        right.GetValueOrDefault(new BorderSide(0, default)),
        bottom.GetValueOrDefault(new BorderSide(0, default))
      );
    }

    public static readonly Border None = new(
      new BorderSide(0, default),
      new BorderSide(0, default),
      new BorderSide(0, default),
      new BorderSide(0, default)
    );
  }
}
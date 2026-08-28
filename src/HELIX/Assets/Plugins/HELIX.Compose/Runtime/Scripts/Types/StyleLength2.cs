using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  [Serializable]
  public struct StyleLength2 : IEquatable<StyleLength2> {
    public StyleLength w, h;

    public StyleLength2(StyleLength w, StyleLength h) {
      this.w = w;
      this.h = h;
    }

    public StyleLength2(StyleLength v) {
      w = v;
      h = v;
    }

    public static implicit operator StyleLength2(Vector2 v) {
      return new StyleLength2(new StyleLength(v.x), new StyleLength(v.y));
    }


    public StyleLength2 Abs() {
      return new StyleLength2(StyleLengths.Abs(w), StyleLengths.Abs(h));
    }

    public static StyleLength2 operator +(StyleLength2 a, StyleLength2 b) {
      return new StyleLength2(StyleLengths.Add(a.w, b.w), StyleLengths.Add(a.h, b.h));
    }

    public static StyleLength2 operator -(StyleLength2 a, StyleLength2 b) {
      return new StyleLength2(StyleLengths.Subtract(a.w, b.w), StyleLengths.Subtract(a.h, b.h));
    }

    public static StyleLength2 operator *(StyleLength2 a, StyleLength2 b) {
      return new StyleLength2(StyleLengths.Multiply(a.w, b.w), StyleLengths.Multiply(a.h, b.h));
    }

    public static StyleLength2 operator /(StyleLength2 a, StyleLength2 b) {
      return new StyleLength2(StyleLengths.Divide(a.w, b.w), StyleLengths.Divide(a.h, b.h));
    }

    public static StyleLength2 operator -(StyleLength2 a) {
      return new StyleLength2(StyleLengths.Negate(a.w), StyleLengths.Negate(a.h));
    }

    public static StyleLength2 Lerp(StyleLength2 from, StyleLength2 to, float t) {
      return new StyleLength2(
        StyleLengths.Interpolate(from.w, to.w, t),
        StyleLengths.Interpolate(from.h, to.h, t)
      );
    }

    public bool Equals(StyleLength2 other) {
      return w.Equals(other.w) && h.Equals(other.h);
    }

    public override bool Equals(object obj) {
      return obj is StyleLength2 other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(w, h);
    }

    public override string ToString() {
      return $"StyleLength2(width: {w.FormatStyleValue()}, height: {h.FormatStyleValue()})";
    }

    public static readonly StyleLength2 Initial = new(StyleKeyword.Initial);
    public static readonly StyleLength2 Null = new(StyleKeyword.Null);
    public static readonly StyleLength2 Auto = new(StyleKeyword.Auto);
  }
}
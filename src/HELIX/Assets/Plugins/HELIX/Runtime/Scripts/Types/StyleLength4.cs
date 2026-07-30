using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Types {
  [Serializable]
  public struct StyleLength4 : IEquatable<StyleLength4>, IStyleLength4 {
    public StyleLength l, t, r, b;

    public StyleLength4(StyleLength l, StyleLength t, StyleLength r, StyleLength b) {
      this.l = l;
      this.t = t;
      this.r = r;
      this.b = b;
    }

    public StyleLength4(StyleLength2 xy, StyleLength2 zw) {
      t = xy.w;
      r = xy.h;
      b = zw.w;
      l = zw.h;
    }

    public StyleLength4(StyleLength v) {
      t = v;
      r = v;
      b = v;
      l = v;
    }

    public static implicit operator StyleLength2(StyleLength4 sl4) {
      return new StyleLength2(sl4.t, sl4.r);
    }

    public static implicit operator StyleLength4(Vector4 v) {
      return new StyleLength4(v.x, v.y, v.z, v.w);
    }

    public static implicit operator StyleLength4(StyleLength2 xy) {
      return new StyleLength4(xy, xy);
    }

    public static implicit operator StyleLength4(StyleLength v) {
      return new StyleLength4(v);
    }

    public static implicit operator StyleLength4(float v) {
      return new StyleLength4(v);
    }

    public StyleLength4 Abs() =>
      new(StyleLengths.Abs(l), StyleLengths.Abs(t), StyleLengths.Abs(r), StyleLengths.Abs(b));

    public static StyleLength4 operator +(StyleLength4 a, StyleLength4 b) {
      return new StyleLength4(
        StyleLengths.Add(a.l, b.l),
        StyleLengths.Add(a.t, b.t),
        StyleLengths.Add(a.r, b.r),
        StyleLengths.Add(a.b, b.b)
      );
    }

    public static StyleLength4 operator -(StyleLength4 a, StyleLength4 b) {
      return new StyleLength4(
        StyleLengths.Subtract(a.l, b.l),
        StyleLengths.Subtract(a.t, b.t),
        StyleLengths.Subtract(a.r, b.r),
        StyleLengths.Subtract(a.b, b.b)
      );
    }

    public static StyleLength4 operator *(StyleLength4 a, StyleLength4 b) {
      return new StyleLength4(
        StyleLengths.Multiply(a.l, b.l),
        StyleLengths.Multiply(a.t, b.t),
        StyleLengths.Multiply(a.r, b.r),
        StyleLengths.Multiply(a.b, b.b)
      );
    }

    public static StyleLength4 operator /(StyleLength4 a, StyleLength4 b) {
      return new StyleLength4(
        StyleLengths.Divide(a.l, b.l),
        StyleLengths.Divide(a.t, b.t),
        StyleLengths.Divide(a.r, b.r),
        StyleLengths.Divide(a.b, b.b)
      );
    }

    public static StyleLength4 operator -(StyleLength4 a) {
      return new StyleLength4(
        StyleLengths.Negate(a.l),
        StyleLengths.Negate(a.t),
        StyleLengths.Negate(a.r),
        StyleLengths.Negate(a.b)
      );
    }

    public static StyleLength4 Lerp(StyleLength4 from, StyleLength4 to, float t) {
      return new StyleLength4(
        StyleLengths.Interpolate(from.l, to.l, t),
        StyleLengths.Interpolate(from.t, to.t, t),
        StyleLengths.Interpolate(from.r, to.r, t),
        StyleLengths.Interpolate(from.b, to.b, t)
      );
    }

    public bool Equals(StyleLength4 other) {
      return t.Equals(other.t) && r.Equals(other.r) && b.Equals(other.b) && l.Equals(other.l);
    }

    public override bool Equals(object obj) {
      return obj is StyleLength4 other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(l, t, r, b);
    }

    public override string ToString() {
      return HelixFormattingHelper.BuildQuadruple(
        "StyleLength4",
        l.FormatStyleValue(),
        t.FormatStyleValue(),
        r.FormatStyleValue(),
        b.FormatStyleValue()
      );
    }

    public StyleLength4 ToStyleLength4() {
      return this;
    }

    public static StyleLength4 Only(
      StyleLength? left = null,
      StyleLength? right = null,
      StyleLength? top = null,
      StyleLength? bottom = null
    ) {
      return new StyleLength4(
        left ?? StyleKeyword.Null,
        top ?? StyleKeyword.Null,
        right ?? StyleKeyword.Null,
        bottom ?? StyleKeyword.Null
      );
    }

    public static StyleLength4 Symmetric(StyleLength? horizontal = null, StyleLength? vertical = null) {
      return new StyleLength4(
        horizontal ?? StyleKeyword.Null,
        vertical ?? StyleKeyword.Null,
        horizontal ?? StyleKeyword.Null,
        vertical ?? StyleKeyword.Null
      );
    }

    public static StyleLength4 All(StyleLength value) {
      return new StyleLength4(value);
    }

    public static readonly StyleLength4 Zero = new(0);
    public static readonly StyleLength4 Initial = new(StyleKeyword.Initial);
    public static readonly StyleLength4 Null = new(StyleKeyword.Null);
    public static readonly StyleLength4 Auto = new(StyleKeyword.Auto);
  }

  public interface IStyleLength4 {
    StyleLength4 ToStyleLength4();
  }
}
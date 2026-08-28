using System;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public readonly struct Flex : IEquatable<Flex> {
    public readonly StyleFloat grow;
    public readonly StyleFloat shrink;
    public readonly StyleLength basis;
    public readonly StyleEnum<Align> align;

    public static readonly Flex Null = new(StyleKeyword.Null, StyleKeyword.Null, StyleKeyword.Null, StyleKeyword.Null);

    public Flex(StyleFloat grow, StyleFloat shrink, StyleLength basis, StyleEnum<Align> align) {
      this.grow = grow;
      this.shrink = shrink;
      this.basis = basis;
      this.align = align;
    }

    public static Flex Grow(StyleFloat factor) {
      return new Flex(factor, 0, StyleKeyword.Null, StyleKeyword.Null);
    }

    public static Flex Grow() {
      return new Flex(1f, 0, StyleKeyword.Null, StyleKeyword.Null);
    }

    public static Flex Shrink(StyleFloat factor) {
      return new Flex(0, factor, StyleKeyword.Null, StyleKeyword.Null);
    }

    public static Flex Shrink() {
      return new Flex(0, 1f, StyleKeyword.Null, StyleKeyword.Null);
    }

    public static Flex Flexible(StyleFloat grow) {
      return new Flex(grow, 1f, StyleKeyword.Null, StyleKeyword.Null);
    }

    public static Flex Flexible() {
      return new Flex(1f, 1f, StyleKeyword.Null, StyleKeyword.Null);
    }

    public static Flex Fill(StyleFloat grow) {
      return new Flex(grow, 0, StyleKeyword.Null, Align.Stretch);
    }

    public static Flex Fill() {
      return new Flex(1f, 0, StyleKeyword.Null, Align.Stretch);
    }

    public static Flex FillFlexible(StyleFloat grow) {
      return new Flex(grow, 1f, StyleKeyword.Null, Align.Stretch);
    }

    public static Flex FillFlexible() {
      return new Flex(1f, 1f, StyleKeyword.Null, Align.Stretch);
    }

    public static Flex Of(
      StyleFloat? grow = null,
      StyleFloat? shrink = null,
      StyleEnum<Align>? align = null,
      StyleLength? basis = null
    ) {
      return new Flex(
        grow.GetValueOrDefault(StyleKeyword.Null),
        shrink.GetValueOrDefault(StyleKeyword.Null),
        basis.GetValueOrDefault(StyleKeyword.Null),
        align.GetValueOrDefault(StyleKeyword.Null)
      );
    }

    public void Apply(VisualElement element) {
      element.style.flexGrow = grow;
      element.style.flexShrink = shrink;
      element.style.flexBasis = basis;
      element.style.alignSelf = align;
    }

    public bool Equals(Flex other) {
      return grow.Equals(other.grow) && shrink.Equals(other.shrink) && basis.Equals(other.basis) &&
        align.Equals(other.align);
    }

    public override bool Equals(object obj) {
      return obj is Flex other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(grow, shrink, basis, align);
    }
  }
}
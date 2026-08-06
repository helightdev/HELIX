using System;
using UnityEngine.UIElements;

namespace HELIX.Types {
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

    public static Flex Grow(StyleFloat factor) => new(factor, 0, StyleKeyword.Null, StyleKeyword.Null);
    public static Flex Grow() => new(1f, 0, StyleKeyword.Null, StyleKeyword.Null);
    public static Flex Shrink(StyleFloat factor) => new(0, factor, StyleKeyword.Null, StyleKeyword.Null);
    public static Flex Shrink() => new(0, 1f, StyleKeyword.Null, StyleKeyword.Null);
    public static Flex Flexible(StyleFloat grow) => new(grow, 1f, StyleKeyword.Null, StyleKeyword.Null);
    public static Flex Flexible() => new(1f, 1f, StyleKeyword.Null, StyleKeyword.Null);
    public static Flex Fill(StyleFloat grow) => new(grow, 0, StyleKeyword.Null, Align.Stretch);
    public static Flex Fill() => new(1f, 0, StyleKeyword.Null, Align.Stretch);
    public static Flex FillFlexible(StyleFloat grow) => new(grow, 1f, StyleKeyword.Null, Align.Stretch);
    public static Flex FillFlexible() => new(1f, 1f, StyleKeyword.Null, Align.Stretch);

    public static Flex Of(
      StyleFloat? grow = null, StyleFloat? shrink = null, StyleEnum<Align>? align = null, StyleLength? basis = null
    ) => new(
      grow.GetValueOrDefault(StyleKeyword.Null), shrink.GetValueOrDefault(StyleKeyword.Null),
      basis.GetValueOrDefault(StyleKeyword.Null), align.GetValueOrDefault(StyleKeyword.Null)
    );

    public void Apply(VisualElement element) {
      element.style.flexGrow = grow;
      element.style.flexShrink = shrink;
      element.style.flexBasis = basis;
      element.style.alignSelf = align;
    }

    public bool Equals(Flex other) {
      return grow.Equals(other.grow) && shrink.Equals(other.shrink) && basis.Equals(other.basis) && align.Equals(other.align);
    }

    public override bool Equals(object obj) {
      return obj is Flex other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(grow, shrink, basis, align);
    }
  }
}
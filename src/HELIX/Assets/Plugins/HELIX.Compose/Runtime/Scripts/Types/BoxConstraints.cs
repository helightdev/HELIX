using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public readonly struct AxisConstraint : IEquatable<AxisConstraint> {
    public readonly StyleLength preferred;
    public readonly StyleLength min;
    public readonly StyleLength max;

    public AxisConstraint(StyleLength preferred, StyleLength min, StyleLength max) {
      this.preferred = preferred;
      this.min = min;
      this.max = max;
    }

    public void Apply(VisualElement element, Axis axis) {
      if (axis == Axis.Horizontal) {
        element.style.width = preferred;
        element.style.minWidth = min;
        element.style.maxWidth = max;
      } else {
        element.style.height = preferred;
        element.style.minHeight = min;
        element.style.maxHeight = max;
      }
    }

    public float Resolve(float available) {
      return StyleLengths.ResolveConstraint(preferred, min, max, available);
    }

    public bool Equals(AxisConstraint other) {
      return preferred.Equals(other.preferred) && min.Equals(other.min) && max.Equals(other.max);
    }

    public override bool Equals(object obj) {
      return obj is AxisConstraint other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(preferred, min, max);
    }

    public override string ToString() {
      if ((min.keyword is StyleKeyword.Null or StyleKeyword.Auto &&
          max.keyword is StyleKeyword.Null or StyleKeyword.Auto) ||
        (min == max && min == preferred)) return preferred.FormatStyleValue();
      return min.FormatStyleValue() + " ≤ " + preferred.FormatStyleValue() + " ≤ " + max.FormatStyleValue();
    }

    public static AxisConstraint Only(
      StyleLength? preferred = null,
      StyleLength? min = null,
      StyleLength? max = null
    ) {
      return new AxisConstraint(
        preferred.GetValueOrDefault(StyleKeyword.Null),
        min.GetValueOrDefault(StyleKeyword.Null),
        max.GetValueOrDefault(StyleKeyword.Null)
      );
    }

    public static AxisConstraint Preferred(StyleLength preferred) {
      return new AxisConstraint(preferred, StyleKeyword.Null, StyleKeyword.Null);
    }

    public static AxisConstraint Tight(StyleLength size) {
      return new AxisConstraint(size, size, size);
    }

    public static AxisConstraint Loose(StyleLength max) {
      return new AxisConstraint(StyleKeyword.Null, StyleKeyword.Null, max);
    }

    public static AxisConstraint Min(StyleLength min) {
      return new AxisConstraint(StyleKeyword.Null, min, StyleKeyword.Null);
    }

    public static readonly AxisConstraint Initial = new(
      StyleKeyword.Initial,
      StyleKeyword.Initial,
      StyleKeyword.Initial
    );

    public static readonly AxisConstraint Null = new(
      StyleKeyword.Null,
      StyleKeyword.Null,
      StyleKeyword.Null
    );
  }

  public readonly struct BoxConstraints : IEquatable<BoxConstraints> {
    public readonly AxisConstraint width;
    public readonly AxisConstraint height;

    public BoxConstraints(AxisConstraint width, AxisConstraint height) {
      this.width = width;
      this.height = height;
    }

    public BoxConstraints(StyleLength2 preferred, StyleLength2 min, StyleLength2 max) : this(
      new AxisConstraint(preferred.w, min.w, max.w),
      new AxisConstraint(preferred.h, min.h, max.h)
    ) { }

    public void Apply(VisualElement element) {
      width.Apply(element, Axis.Horizontal);
      height.Apply(element, Axis.Vertical);
    }

    public Vector2 ResolveSize(Vector2 available) {
      return new Vector2(
        width.Resolve(available.x),
        height.Resolve(available.y)
      );
    }

    public bool Equals(BoxConstraints other) {
      return width.Equals(other.width) && height.Equals(other.height);
    }

    public override bool Equals(object obj) {
      return obj is BoxConstraints other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(width, height);
    }

    public override string ToString() {
      return $"BoxConstraints(width: {width}, height: {height})";
    }

    public static BoxConstraints Only(
      StyleLength2? preferred = null,
      StyleLength2? min = null,
      StyleLength2? max = null
    ) {
      return new BoxConstraints(
        preferred.GetValueOrDefault(StyleLength2.Null),
        min.GetValueOrDefault(StyleLength2.Null),
        max.GetValueOrDefault(StyleLength2.Null)
      );
    }

    public static BoxConstraints Preferred(StyleLength2 preferred) {
      return new BoxConstraints(AxisConstraint.Preferred(preferred.w), AxisConstraint.Preferred(preferred.h));
    }

    public static BoxConstraints Preferred(StyleLength width, StyleLength height) {
      return new BoxConstraints(AxisConstraint.Preferred(width), AxisConstraint.Preferred(height));
    }

    public static BoxConstraints Tight(StyleLength2 size) {
      return new BoxConstraints(AxisConstraint.Tight(size.w), AxisConstraint.Tight(size.h));
    }

    public static BoxConstraints Tight(StyleLength width, StyleLength height) {
      return new BoxConstraints(AxisConstraint.Tight(width), AxisConstraint.Tight(height));
    }

    public static BoxConstraints Loose(StyleLength2 max) {
      return new BoxConstraints(AxisConstraint.Loose(max.w), AxisConstraint.Loose(max.h));
    }

    public static BoxConstraints Min(StyleLength2 min) {
      return new BoxConstraints(AxisConstraint.Min(min.w), AxisConstraint.Min(min.h));
    }

    public static readonly BoxConstraints Initial = new(AxisConstraint.Initial, AxisConstraint.Initial);
    public static readonly BoxConstraints Null = new(AxisConstraint.Null, AxisConstraint.Null);
  }
}
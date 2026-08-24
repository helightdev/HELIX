using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Types {
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

    public float Resolve(float available) => StyleLengths.ResolveConstraint(preferred, min, max, available);

    public bool Equals(AxisConstraint other) =>
      preferred.Equals(other.preferred) && min.Equals(other.min) && max.Equals(other.max);

    public override bool Equals(object obj) => obj is AxisConstraint other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(preferred, min, max);

    public override string ToString() {
      if ((min.keyword is StyleKeyword.Null or StyleKeyword.Auto &&
           max.keyword is StyleKeyword.Null or StyleKeyword.Auto) ||
          min == max && min == preferred) return preferred.FormatStyleValue();
      return min.FormatStyleValue() + " ≤ " + preferred.FormatStyleValue() + " ≤ " + max.FormatStyleValue();
    }

    public static AxisConstraint Only(
      StyleLength? preferred = null, StyleLength? min = null, StyleLength? max = null
    ) => new(
      preferred.GetValueOrDefault(StyleKeyword.Null),
      min.GetValueOrDefault(StyleKeyword.Null),
      max.GetValueOrDefault(StyleKeyword.Null)
    );

    public static AxisConstraint Preferred(StyleLength preferred) =>
      new(preferred, StyleKeyword.Null, StyleKeyword.Null);

    public static AxisConstraint Tight(StyleLength size) => new(size, size, size);

    public static AxisConstraint Loose(StyleLength max) =>
      new(StyleKeyword.Null, StyleKeyword.Null, max);

    public static AxisConstraint Min(StyleLength min) =>
      new(StyleKeyword.Null, min, StyleKeyword.Null);

    public static readonly AxisConstraint Initial = new(
      StyleKeyword.Initial, StyleKeyword.Initial, StyleKeyword.Initial
    );

    public static readonly AxisConstraint Null = new(
      StyleKeyword.Null, StyleKeyword.Null, StyleKeyword.Null
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

    public Vector2 ResolveSize(Vector2 available) => new(
      width.Resolve(available.x), height.Resolve(available.y)
    );

    public bool Equals(BoxConstraints other) => width.Equals(other.width) && height.Equals(other.height);
    public override bool Equals(object obj) => obj is BoxConstraints other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(width, height);
    public override string ToString() => $"BoxConstraints(width: {width}, height: {height})";

    public static BoxConstraints Only(
      StyleLength2? preferred = null,
      StyleLength2? min = null,
      StyleLength2? max = null
    ) => new(
      preferred.GetValueOrDefault(StyleLength2.Null),
      min.GetValueOrDefault(StyleLength2.Null),
      max.GetValueOrDefault(StyleLength2.Null)
    );

    public static BoxConstraints Preferred(StyleLength2 preferred) =>
      new(AxisConstraint.Preferred(preferred.w), AxisConstraint.Preferred(preferred.h));

    public static BoxConstraints Preferred(StyleLength width, StyleLength height) =>
      new(AxisConstraint.Preferred(width), AxisConstraint.Preferred(height));

    public static BoxConstraints Tight(StyleLength2 size) =>
      new(AxisConstraint.Tight(size.w), AxisConstraint.Tight(size.h));

    public static BoxConstraints Tight(StyleLength width, StyleLength height) =>
      new(AxisConstraint.Tight(width), AxisConstraint.Tight(height));

    public static BoxConstraints Loose(StyleLength2 max) =>
      new(AxisConstraint.Loose(max.w), AxisConstraint.Loose(max.h));

    public static BoxConstraints Min(StyleLength2 min) =>
      new(AxisConstraint.Min(min.w), AxisConstraint.Min(min.h));

    public static readonly BoxConstraints Initial = new(AxisConstraint.Initial, AxisConstraint.Initial);
    public static readonly BoxConstraints Null = new(AxisConstraint.Null, AxisConstraint.Null);
  }
}

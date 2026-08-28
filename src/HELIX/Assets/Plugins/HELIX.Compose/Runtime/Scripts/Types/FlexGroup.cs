using System;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public readonly struct FlexGroup : IEquatable<FlexGroup> {
    public readonly StyleEnum<Justify> main;
    public readonly StyleEnum<Align> cross;
    public readonly StyleEnum<Align> wrapCross;
    public readonly StyleEnum<FlexDirection> direction;
    public readonly StyleEnum<Wrap> wrap;

    public static readonly FlexGroup Null = new(
      StyleKeyword.Null,
      StyleKeyword.Null,
      StyleKeyword.Null,
      StyleKeyword.Null,
      StyleKeyword.Null
    );

    public FlexGroup(
      StyleEnum<Justify> main,
      StyleEnum<Align> cross,
      StyleEnum<Align> wrapCross,
      StyleEnum<FlexDirection> direction,
      StyleEnum<Wrap> wrap
    ) {
      this.main = main;
      this.cross = cross;
      this.wrapCross = wrapCross;
      this.direction = direction;
      this.wrap = wrap;
    }

    public static FlexGroup Of(
      Axis axis,
      Justify main,
      Align cross,
      Align? wrapCross = null,
      Wrap? wrap = null,
      bool reverse = false
    ) {
      return new FlexGroup(main, cross, wrapCross ?? cross, axis.ToFlexDirection(reverse), wrap ?? Wrap.NoWrap);
    }

    public static FlexGroup Row(
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      bool reverse = false
    ) {
      return new FlexGroup(main, cross, cross, reverse ? FlexDirection.RowReverse : FlexDirection.Row, Wrap.NoWrap);
    }

    public static FlexGroup Column(
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      bool reverse = false
    ) {
      return new FlexGroup(
        main,
        cross,
        cross,
        reverse ? FlexDirection.ColumnReverse : FlexDirection.Column,
        Wrap.NoWrap
      );
    }

    public static FlexGroup Wrapping(
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      Align? wrapCross = null,
      Axis axis = Axis.Horizontal,
      bool reverse = false,
      bool wrapReverse = false
    ) {
      return new FlexGroup(
        main,
        cross,
        wrapCross ?? cross,
        axis.ToFlexDirection(reverse),
        wrapReverse ? Wrap.WrapReverse : Wrap.Wrap
      );
    }

    public static FlexGroup Center(
      Axis axis = Axis.Vertical,
      bool reverse = false
    ) {
      return new FlexGroup(Justify.Center, Align.Center, Align.Center, axis.ToFlexDirection(reverse), Wrap.NoWrap);
    }

    public void Apply(VisualElement element) {
      element.style.justifyContent = main;
      element.style.alignItems = cross;
      element.style.alignContent = wrapCross;
      element.style.flexDirection = direction;
      element.style.flexWrap = wrap;
    }

    public bool Equals(FlexGroup other) {
      return main.Equals(other.main) && cross.Equals(other.cross) &&
        wrapCross.Equals(other.wrapCross) && direction.Equals(other.direction) &&
        wrap.Equals(other.wrap);
    }

    public override bool Equals(object obj) {
      return obj is FlexGroup other && Equals(other);
    }

    public override int GetHashCode() {
      return HashCode.Combine(main, cross, wrapCross, direction, wrap);
    }
  }
}
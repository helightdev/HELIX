using UnityEngine.UIElements;

namespace HELIX.Types {
    public enum Axis : byte {
        Horizontal = 0,
        Vertical = 1
    }

    public static class AxisExtensions {
        public static Axis Opposite(this Axis axis) {
            return axis == Axis.Horizontal ? Axis.Vertical : Axis.Horizontal;
        }

        public static FlexDirection ToFlexDirection(this Axis axis, bool reverse = false) {
            return axis == Axis.Horizontal
                ? reverse ? FlexDirection.RowReverse : FlexDirection.Row
                : reverse ? FlexDirection.ColumnReverse : FlexDirection.Column;
        }
    }
}
using UnityEngine;
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
                : reverse
                    ? FlexDirection.ColumnReverse
                    : FlexDirection.Column;
        }

        public static float GetLayoutSize(this Axis axis, VisualElement element) {
            return axis == Axis.Horizontal ? element.layout.width : element.layout.height;
        }

        public static float GetRectSize(this Axis axis, Rect rect) {
            return axis == Axis.Horizontal ? rect.width : rect.height;
        }

        public static float GetVectorComponent(this Axis axis, Vector2 vector) {
            return axis == Axis.Horizontal ? vector.x : vector.y;
        }

        public static void SetSize(this Axis axis, VisualElement element, Length length) {
            if (axis == Axis.Horizontal) element.style.width = length;
            else element.style.height = length;
        }

        public static void SetBegin(this Axis axis, VisualElement element, Length length) {
            if (axis == Axis.Horizontal) element.style.left = length;
            else element.style.top = length;
        }

        public static void SetEnd(this Axis axis, VisualElement element, Length length) {
            if (axis == Axis.Horizontal) element.style.right = length;
            else element.style.bottom = length;
        }

        public static Vector2 ToCoefficient(this Axis axis) {
            return axis == Axis.Horizontal ? Vector2.right : Vector2.up;
        }
    }
}
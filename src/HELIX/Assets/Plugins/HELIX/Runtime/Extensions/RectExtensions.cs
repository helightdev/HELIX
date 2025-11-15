using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Extensions {
    public static class RectExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 TopLeft(this Rect rect) {
            return new Vector2(rect.x, rect.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 TopRight(this Rect rect) {
            return new Vector2(rect.x + rect.width, rect.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 BottomLeft(this Rect rect) {
            return new Vector2(rect.x, rect.y + rect.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 BottomRight(this Rect rect) {
            return new Vector2(rect.x + rect.width, rect.y + rect.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Center(this Rect rect) {
            return new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect Inflate(this Rect rect, float value) {
            return new Rect(rect.x - value, rect.y - value, rect.width + value * 2, rect.height + value * 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect Deflate(this Rect rect, float value) {
            return new Rect(rect.x + value, rect.y + value, rect.width - value * 2, rect.height - value * 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect Move(this Rect rect, Vector2 value) {
            return new Rect(rect.x + value.x, rect.y + value.y, rect.width, rect.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect Move(this Rect rect, float x, float y) {
            return new Rect(rect.x + x, rect.y + y, rect.width, rect.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect Move(this Rect rect, float value) {
            return new Rect(rect.x + value, rect.y + value, rect.width, rect.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect WithX(this Rect rect, float x) {
            return new Rect(x, rect.y, rect.width, rect.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect WithY(this Rect rect, float y) {
            return new Rect(rect.x, y, rect.width, rect.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect WithPosition(this Rect rect, Vector2 position) {
            return new Rect(position.x, position.y, rect.width, rect.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect ClampPosition(this Rect rect, Vector2 min, Vector2 max) {
            return new Rect(Mathf.Clamp(rect.x, min.x, max.x), Mathf.Clamp(rect.y, min.y, max.y), rect.width,
                rect.height);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect WithWidth(this Rect rect, float width) {
            return new Rect(rect.x, rect.y, width, rect.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect WithHeight(this Rect rect, float height) {
            return new Rect(rect.x, rect.y, rect.width, height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect Scale(this Rect rect, float value) {
            return new Rect(rect.x, rect.y, rect.width * value, rect.height * value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect Scale(this Rect rect, float x, float y) {
            return new Rect(rect.x, rect.y, rect.width * x, rect.height * y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect Scale(this Rect rect, Vector2 vector2) {
            return new Rect(rect.x, rect.y, rect.width * vector2.x, rect.height * vector2.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect WithSize(this Rect rect, Vector2 size) {
            return new Rect(rect.x, rect.y, size.x, size.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect ClampSize(this Rect rect, Vector2 min, Vector2 max) {
            return new Rect(rect.x, rect.y, Mathf.Clamp(rect.width, min.x, max.x),
                Mathf.Clamp(rect.height, min.y, max.y));
        }

        public static void PathRect(this Painter2D painter, Rect rect) {
            painter.BeginPath();
            painter.MoveTo(rect.position);
            painter.LineTo(rect.position + new Vector2(rect.width, 0));
            painter.LineTo(rect.position + new Vector2(rect.width, rect.height));
            painter.LineTo(rect.position + new Vector2(0, rect.height));
            painter.ClosePath();
        }

        public static void PathRRect(this Painter2D painter, Rect rect, float radius) {
            radius = Mathf.Min(
                Mathf.Max(radius, 0f),
                rect.width * 0.5f,
                rect.height * 0.5f
            );
            
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.x + radius, rect.y));
            painter.LineTo(new Vector2(rect.x + rect.width - radius, rect.y));
            painter.ArcTo(new Vector2(rect.x + rect.width, rect.y), new Vector2(rect.x + rect.width, rect.y + radius),
                radius);
            painter.LineTo(new Vector2(rect.x + rect.width, rect.y + rect.height - radius));
            painter.ArcTo(new Vector2(rect.x + rect.width, rect.y + rect.height),
                new Vector2(rect.x + rect.width - radius, rect.y + rect.height), radius);
            painter.LineTo(new Vector2(rect.x + radius, rect.y + rect.height));
            painter.ArcTo(new Vector2(rect.x, rect.y + rect.height), new Vector2(rect.x, rect.y + rect.height - radius),
                radius);
            painter.LineTo(new Vector2(rect.x, rect.y + radius));
            painter.ArcTo(new Vector2(rect.x, rect.y), new Vector2(rect.x + radius, rect.y), radius);
            painter.ClosePath();
        }
        
        public static void PathRRect(this Painter2D painter, Rect rect, Vector4 radius) {
            radius = new Vector4(
                Mathf.Max(radius.x, 0f),
                Mathf.Max(radius.y, 0f),
                Mathf.Max(radius.z, 0f),
                Mathf.Max(radius.w, 0f)
            );

            var scale = 1f;
            var topSum    = radius.x + radius.y;
            var bottomSum = radius.w + radius.z;
            var leftSum   = radius.x + radius.w;
            var rightSum  = radius.y + radius.z;

            if (topSum > 0f) scale = Mathf.Min(scale, rect.width / topSum);
            if (bottomSum > 0f) scale = Mathf.Min(scale, rect.width / bottomSum);
            if (leftSum > 0f) scale = Mathf.Min(scale, rect.height / leftSum);
            if (rightSum > 0f) scale = Mathf.Min(scale, rect.height / rightSum);

            scale = Mathf.Clamp01(scale);
            radius *= scale;
            
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.x + radius.x, rect.y));
            painter.LineTo(new Vector2(rect.x + rect.width - radius.y, rect.y));
            painter.ArcTo(new Vector2(rect.x + rect.width, rect.y), new Vector2(rect.x + rect.width, rect.y + radius.y),
                radius.y);
            painter.LineTo(new Vector2(rect.x + rect.width, rect.y + rect.height - radius.z));
            painter.ArcTo(new Vector2(rect.x + rect.width, rect.y + rect.height),
                new Vector2(rect.x + rect.width - radius.z, rect.y + rect.height), radius.z);
            painter.LineTo(new Vector2(rect.x + radius.w, rect.y + rect.height));
            painter.ArcTo(new Vector2(rect.x, rect.y + rect.height), new Vector2(rect.x, rect.y + rect.height - radius.w),
                radius.w);
            painter.LineTo(new Vector2(rect.x, rect.y + radius.x));
            painter.ArcTo(new Vector2(rect.x, rect.y), new Vector2(rect.x + radius.x, rect.y), radius.x);
            painter.ClosePath();
        }
    }
}
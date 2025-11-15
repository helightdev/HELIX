using HELIX.Types;
using UnityEngine;

namespace HELIX.Painting.Paths {
    public static class BasicShapes {
        public static void Rect(this IPathBuilder builder, Rect rect) {
            builder.MoveTo(rect.position);
            builder.LineTo(rect.position + new Vector2(rect.width, 0));
            builder.LineTo(rect.position + new Vector2(rect.width, rect.height));
            builder.LineTo(rect.position + new Vector2(0, rect.height));
            builder.ClosePath();
        }

        public static void RRect(this IPathBuilder builder, RRect roundedRect) {
            var radius = roundedRect.radii;
            var rect = roundedRect.rect;
            
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (radius.x == radius.y && radius.y == radius.z && radius.z == radius.w) {
                var r = Mathf.Min(
                    Mathf.Max(radius.x, 0f),
                    rect.width * 0.5f,
                    rect.height * 0.5f
                );
                radius = new Vector4(r, r, r, r);
            } else {
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
            }
            
            builder.MoveTo(new Vector2(rect.x + radius.x, rect.y));
            builder.LineTo(new Vector2(rect.x + rect.width - radius.y, rect.y));
            builder.ArcTo(new Vector2(rect.x + rect.width, rect.y), new Vector2(rect.x + rect.width, rect.y + radius.y),
                radius.y);
            builder.LineTo(new Vector2(rect.x + rect.width, rect.y + rect.height - radius.z));
            builder.ArcTo(new Vector2(rect.x + rect.width, rect.y + rect.height),
                new Vector2(rect.x + rect.width - radius.z, rect.y + rect.height), radius.z);
            builder.LineTo(new Vector2(rect.x + radius.w, rect.y + rect.height));
            builder.ArcTo(new Vector2(rect.x, rect.y + rect.height), new Vector2(rect.x, rect.y + rect.height - radius.w),
                radius.w);
            builder.LineTo(new Vector2(rect.x, rect.y + radius.x));
            builder.ArcTo(new Vector2(rect.x, rect.y), new Vector2(rect.x + radius.x, rect.y), radius.x);
            builder.ClosePath();
        }
    }
}
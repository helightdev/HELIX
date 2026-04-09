using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Types {
    [Serializable]
    public struct Alignment {
        [Range(-1f, 1f)]
        public float x;

        [Range(-1f, 1f)]
        public float y;

        public Alignment(float x, float y) {
            this.x = Mathf.Clamp(x, -1f, 1f);
            this.y = Mathf.Clamp(y, -1f, 1f);
        }

        public Vector2 GetOffsetCoefficients() {
            var horizontal = math.remap(-1f, 1f, 0f, 1f, x);
            var vertical = math.remap(-1f, 1f, 0f, 1f, y);
            return new Vector2(horizontal, vertical);
        }

        public static readonly Alignment TopCenter = new(0f, -1f);
        public static readonly Alignment TopRight = new(1f, -1f);
        public static readonly Alignment TopLeft = new(-1f, -1f);
        public static readonly Alignment CenterLeft = new(-1f, 0f);
        public static readonly Alignment Center = new(0f, 0f);
        public static readonly Alignment CenterRight = new(1f, 0f);
        public static readonly Alignment BottomLeft = new(-1f, 1f);
        public static readonly Alignment BottomCenter = new(0f, 1f);
        public static readonly Alignment BottomRight = new(1f, 1f);

        public static implicit operator Alignment(Vector2 vec) {
            return new Alignment(vec.x, vec.y);
        }

        public static implicit operator Vector2(Alignment alignment) {
            return new Vector2(alignment.x, alignment.y);
        }

        public static implicit operator Alignment(TextAnchor anchor) {
            return AlignmentHelper.FromTextAnchor(anchor);
        }

        public TextAnchor Quantize() {
            var horizontalMiddle = Mathf.Approximately(x, 0f);
            var verticalMiddle = Mathf.Approximately(y, 0f);

            if (horizontalMiddle) {
                if (verticalMiddle) return TextAnchor.MiddleCenter;
                return y < 0f ? TextAnchor.UpperCenter : TextAnchor.LowerCenter;
            }

            if (x < 0f) {
                if (verticalMiddle) return TextAnchor.MiddleLeft;
                return y < 0f ? TextAnchor.UpperLeft : TextAnchor.LowerLeft;
            }

            if (verticalMiddle) return TextAnchor.MiddleRight;
            return y < 0f ? TextAnchor.UpperRight : TextAnchor.LowerRight;
        }

        public bool IsLosslessQuantizable() {
            var matchX = Mathf.Approximately(math.round(x), x);
            var matchY = Mathf.Approximately(math.round(y), y);
            return matchX && matchY;
        }

        public void AlignAsColumn(VisualElement element) {
            AlignmentHelper.ToColumnAlignment(Quantize(), out var mainAxis, out var crossAxis);
            element.style.flexDirection = FlexDirection.Column;
            element.style.flexWrap = Wrap.NoWrap;
            element.style.justifyContent = mainAxis;
            element.style.alignItems = crossAxis;
        }

        public override string ToString() {
            if (IsLosslessQuantizable()) {
                var quantized = Quantize();
                return $"Alignment({AlignmentHelper.MapName(quantized)})";
            }
            return $"Alignment(x: {x}, y: {y})";
        }

        public static class AlignmentHelper {
            public static Alignment FromTextAnchor(TextAnchor anchor) {
                return anchor switch {
                    TextAnchor.UpperLeft    => TopLeft,
                    TextAnchor.UpperCenter  => TopCenter,
                    TextAnchor.UpperRight   => TopRight,
                    TextAnchor.MiddleLeft   => CenterLeft,
                    TextAnchor.MiddleCenter => Center,
                    TextAnchor.MiddleRight  => CenterRight,
                    TextAnchor.LowerLeft    => BottomLeft,
                    TextAnchor.LowerCenter  => BottomCenter,
                    TextAnchor.LowerRight   => BottomRight,
                    _                       => throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null)
                };
            }

            public static string MapName(TextAnchor anchor) {
                return anchor switch {
                    TextAnchor.UpperLeft    => "TopLeft",
                    TextAnchor.UpperCenter  => "TopCenter",
                    TextAnchor.UpperRight   => "TopRight",
                    TextAnchor.MiddleLeft   => "CenterLeft",
                    TextAnchor.MiddleCenter => "Center",
                    TextAnchor.MiddleRight  => "CenterRight",
                    TextAnchor.LowerLeft    => "BottomLeft",
                    TextAnchor.LowerCenter  => "BottomCenter",
                    TextAnchor.LowerRight   => "BottomRight",
                    _                       => throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null)
                };
            }

            public static void ToColumnAlignment(TextAnchor anchor, out Justify mainAxis, out Align crossAxis) {
                switch (anchor) {
                    case TextAnchor.UpperLeft:
                        mainAxis = Justify.FlexStart;
                        crossAxis = Align.FlexStart;
                        break;
                    case TextAnchor.UpperCenter:
                        mainAxis = Justify.FlexStart;
                        crossAxis = Align.Center;
                        break;
                    case TextAnchor.UpperRight:
                        mainAxis = Justify.FlexStart;
                        crossAxis = Align.FlexEnd;
                        break;
                    case TextAnchor.MiddleLeft:
                        mainAxis = Justify.Center;
                        crossAxis = Align.FlexStart;
                        break;
                    case TextAnchor.MiddleCenter:
                        mainAxis = Justify.Center;
                        crossAxis = Align.Center;
                        break;
                    case TextAnchor.MiddleRight:
                        mainAxis = Justify.Center;
                        crossAxis = Align.FlexEnd;
                        break;
                    case TextAnchor.LowerLeft:
                        mainAxis = Justify.FlexEnd;
                        crossAxis = Align.FlexStart;
                        break;
                    case TextAnchor.LowerCenter:
                        mainAxis = Justify.FlexEnd;
                        crossAxis = Align.Center;
                        break;
                    case TextAnchor.LowerRight:
                        mainAxis = Justify.FlexEnd;
                        crossAxis = Align.FlexEnd;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(anchor), anchor, null);
                }
            }
        }
    }
}
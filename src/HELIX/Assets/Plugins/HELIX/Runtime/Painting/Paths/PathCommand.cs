using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Painting.Paths {
    [StructLayout(LayoutKind.Sequential)]
    public struct PathCommand {
        public PathCommandType type;
        public Vector2 p0; // MoveTo / LineTo / Arc center / Bezier control1
        public Vector2 p1; // ArcTo control / Bezier control2
        public Vector2 p2; // ArcTo end / Bezier end
        public float f0; // radius for Arc/ArcTo
        public float f1; // startAngle for Arc (radians)
        public float f2; // endAngle for Arc (radians)
        public bool flag; // Arc direction for Arc

        public static PathCommand MoveTo(Vector2 pos) => new() {
            type = PathCommandType.MoveTo,
            p0 = pos
        };

        public static PathCommand LineTo(Vector2 pos) => new() {
            type = PathCommandType.LineTo,
            p0 = pos
        };

        public static PathCommand ArcTo(Vector2 control, Vector2 end, float radius) => new() {
            type = PathCommandType.ArcTo,
            p0 = control,
            p1 = end,
            f0 = radius
        };

        public static PathCommand Arc(Vector2 center, float radius, Angle startRad, Angle endRad, ArcDirection dir) =>
            new() {
                type = PathCommandType.Arc,
                p0 = center,
                f0 = radius,
                f1 = startRad.ToRadians(),
                f2 = endRad.ToRadians(),
                flag = dir == ArcDirection.Clockwise
            };

        public static PathCommand BezierCurveTo(Vector2 c1, Vector2 c2, Vector2 end) => new() {
            type = PathCommandType.BezierCurveTo,
            p0 = c1,
            p1 = c2,
            p2 = end
        };

        public static PathCommand QuadraticCurveTo(Vector2 control, Vector2 end) => new() {
            type = PathCommandType.QuadraticCurveTo,
            p0 = control,
            p1 = end
        };

        public static PathCommand ClosePath() => new() { type = PathCommandType.ClosePath };

        public void Apply(Painter2D painter) {
            switch (type) {
                case PathCommandType.MoveTo:
                    painter.MoveTo(p0);
                    break;
                case PathCommandType.LineTo:
                    painter.LineTo(p0);
                    break;
                case PathCommandType.ArcTo:
                    painter.ArcTo(p0, p1, f0);
                    break;
                case PathCommandType.Arc:
                    painter.Arc(p0, f0, Angle.Radians(f1), Angle.Radians(f2),
                        flag ? ArcDirection.Clockwise : ArcDirection.CounterClockwise);
                    break;
                case PathCommandType.BezierCurveTo:
                    painter.BezierCurveTo(p0, p1, p2);
                    break;
                case PathCommandType.QuadraticCurveTo:
                    painter.QuadraticCurveTo(p0, p1);
                    break;
                case PathCommandType.ClosePath:
                    painter.ClosePath();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum PathCommandType : byte {
        MoveTo = 0,
        LineTo = 1,
        ArcTo = 2,
        Arc = 3,
        BezierCurveTo = 4,
        QuadraticCurveTo = 5,
        ClosePath = 6
    }
}
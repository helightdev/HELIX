using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Painting.Paths {
    public interface IPathBuilder {
        void MoveTo(Vector2 pos);
        void LineTo(Vector2 pos);
        void ArcTo(Vector2 control, Vector2 end, float radius);
        void Arc(Vector2 center, float radius, Angle startRad, Angle endRad, ArcDirection dir);
        void BezierCurveTo(Vector2 c1, Vector2 c2, Vector2 end);
        void QuadraticCurveTo(Vector2 control, Vector2 end);
        void ClosePath();

        static Path Record(Action<IPathBuilder> func) {
            var builder = new PathBuilder();
            func(builder);
            return builder.Build();
        }

        static void Draw(Painter2D painter, Action<IPathBuilder> func) {
            var drawingBuilder = new DrawingPathBuilder(painter);
            painter.BeginPath();
            func(drawingBuilder);
        }
    }
}
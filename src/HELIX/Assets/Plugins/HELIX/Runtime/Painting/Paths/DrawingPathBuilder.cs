using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Painting.Paths {
    public class DrawingPathBuilder : IPathBuilder {
        private readonly Painter2D _painter;

        public DrawingPathBuilder(Painter2D painter) {
            _painter = painter;
        }

        public void MoveTo(Vector2 pos) {
            _painter.MoveTo(pos);
        }

        public void LineTo(Vector2 pos) {
            _painter.LineTo(pos);
        }

        public void ArcTo(Vector2 control, Vector2 end, float radius) {
            _painter.ArcTo(control, end, radius);
        }

        public void Arc(Vector2 center, float radius, Angle startRad, Angle endRad, ArcDirection dir) {
            _painter.Arc(center, radius, startRad, endRad, dir);
        }

        public void BezierCurveTo(Vector2 c1, Vector2 c2, Vector2 end) {
            _painter.BezierCurveTo(c1, c2, end);
        }

        public void QuadraticCurveTo(Vector2 control, Vector2 end) {
            _painter.QuadraticCurveTo(control, end);
        }

        public void ClosePath() {
            _painter.ClosePath();
        }
    }
}
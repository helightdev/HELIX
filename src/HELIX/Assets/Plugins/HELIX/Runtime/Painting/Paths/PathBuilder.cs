using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Painting.Paths {
    public class PathBuilder : IPathBuilder {
        private readonly System.Collections.Generic.List<PathCommand> _commands = new();

        public void MoveTo(Vector2 pos) => _commands.Add(PathCommand.MoveTo(pos));

        public void LineTo(Vector2 pos) => _commands.Add(PathCommand.LineTo(pos));

        public void ArcTo(Vector2 control, Vector2 end, float radius) =>
            _commands.Add(PathCommand.ArcTo(control, end, radius));

        public void Arc(Vector2 center, float radius, Angle startRad, Angle endRad, ArcDirection dir) =>
            _commands.Add(PathCommand.Arc(center, radius, startRad, endRad, dir));

        public void BezierCurveTo(Vector2 c1, Vector2 c2, Vector2 end) =>
            _commands.Add(PathCommand.BezierCurveTo(c1, c2, end));

        public void QuadraticCurveTo(Vector2 control, Vector2 end) =>
            _commands.Add(PathCommand.QuadraticCurveTo(control, end));

        public void ClosePath() {
            _commands.Add(PathCommand.ClosePath());
        }

        public Path Build() => new(_commands.ToArray());
    }
}
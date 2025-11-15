using UnityEngine.UIElements;

namespace HELIX.Painting.Paths {
    public class Path {
        public PathCommand[] commands;

        public Path(PathCommand[] commands) {
            this.commands = commands;
        }

        public void Apply(Painter2D painter) {
            painter.BeginPath();
            foreach (var command in commands) {
                command.Apply(painter);
            }
        }
    }
}
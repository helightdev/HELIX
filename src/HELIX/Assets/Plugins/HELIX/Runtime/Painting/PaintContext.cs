using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Painting {
    public class PaintCanvas {
        public MeshGenerationContext mgc;
        public Painter2D painter;
        public Vector2 size;
        public Rect canvasRect;

        public PaintCanvas(MeshGenerationContext context) {
            mgc = context;
            painter = context.painter2D;
            var layout = context.visualElement.layout;
            size = new Vector2(layout.width, layout.height);
            canvasRect = new Rect(0, 0, size.x, size.y);
        }
    }
}
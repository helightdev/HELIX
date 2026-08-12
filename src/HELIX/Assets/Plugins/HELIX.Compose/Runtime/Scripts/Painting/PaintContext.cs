using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Painting {
  public readonly struct PaintCanvas {
    public readonly Rect canvasRect;
    public readonly MeshGenerationContext mgc;
    public readonly Painter2D painter;
    public readonly Vector2 size;

    public PaintCanvas(MeshGenerationContext context) {
      mgc = context;
      painter = context.painter2D;
      var layout = context.visualElement.layout;
      size = new Vector2(layout.width, layout.height);
      canvasRect = new Rect(0, 0, size.x, size.y);
    }
  }
}
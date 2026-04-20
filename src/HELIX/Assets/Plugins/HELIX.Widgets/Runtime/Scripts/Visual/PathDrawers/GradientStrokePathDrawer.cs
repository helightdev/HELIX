using HELIX.Painting;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual.PathDrawers {
  [UxmlObject]
  public partial class GradientStrokePathDrawer : ScriptablePathDrawer {
    [Header("Gradient Stroke")]
    [UxmlAttribute]
    public Color StartColor { get; set; } = Color.white;

    [UxmlAttribute] public Color EndColor { get; set; } = Color.black;

    [UxmlAttribute] public float Width { get; set; } = 1f;

    [UxmlAttribute] public float MiterLimit { get; set; } = 4f;

    [UxmlAttribute] public LineJoin LineJoin { get; set; } = LineJoin.Miter;

    [UxmlAttribute] public LineCap LineCap { get; set; } = LineCap.Butt;

    [UxmlAttribute] public float DashLength { get; set; } = 0f;

    [UxmlAttribute] public float DashGap { get; set; } = 0f;

    [UxmlAttribute] public float DashOffset { get; set; } = 0f;

    public override void Draw(PaintCanvas canvas) {
      var gradient = new Gradient {
        colorKeys = new[] { new GradientColorKey(StartColor, 0f), new GradientColorKey(EndColor, 1f) },
        alphaKeys = new[] { new GradientAlphaKey(StartColor.a, 0f), new GradientAlphaKey(EndColor.a, 1f) }
      };
      canvas.painter.lineWidth = Width;
      canvas.painter.miterLimit = MiterLimit;
      canvas.painter.lineJoin = LineJoin;
      canvas.painter.lineCap = LineCap;
      canvas.painter.strokeGradient = gradient;
      if (!Mathf.Approximately(DashGap, 0)) {
        canvas.painter.dashOffset = DashOffset;
        canvas.painter.SetDashPattern(DashLength, DashGap);
      }

      canvas.painter.Stroke();
    }
  }
}
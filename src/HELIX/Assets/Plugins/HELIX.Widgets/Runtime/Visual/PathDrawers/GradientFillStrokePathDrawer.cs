using HELIX.Painting;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual.PathDrawers {
    [UxmlObject]
    public partial class GradientFillStrokePathDrawer : ScriptablePathDrawer {
        [Header("Gradient Fill Stroke")]
        [UxmlObjectReference]
        public FillGradientGenerator GradientGenerator { get; set; } = new DirectionalLinearGradientGenerator();

        [UxmlAttribute] public float Width { get; set; } = 1f;
        [UxmlAttribute] public float MiterLimit { get; set; } = 4f;
        [UxmlAttribute] public LineJoin LineJoin { get; set; } = LineJoin.Miter;
        [UxmlAttribute] public LineCap LineCap { get; set; } = LineCap.Butt;
        [UxmlAttribute] public float DashLength { get; set; } = 0f;
        [UxmlAttribute] public float DashGap { get; set; } = 0f;
        [UxmlAttribute] public float DashOffset { get; set; } = 0f;

        public override void Draw(PaintCanvas canvas) {
            if (GradientGenerator == null) return;
            var fillGradient = GradientGenerator.Generate(canvas);
            canvas.painter.strokeFillGradient = fillGradient;
            canvas.painter.lineWidth = Width;
            canvas.painter.miterLimit = MiterLimit;
            canvas.painter.lineJoin = LineJoin;
            canvas.painter.lineCap = LineCap;

            if (!Mathf.Approximately(DashGap, 0)) {
                canvas.painter.dashOffset = DashOffset;
                canvas.painter.SetDashPattern(DashLength, DashGap);
            }

            canvas.painter.Stroke();
        }
    }
}
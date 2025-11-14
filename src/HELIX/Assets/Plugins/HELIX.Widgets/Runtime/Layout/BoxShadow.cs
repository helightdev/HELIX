using HELIX.Extensions;
using HELIX.Painting;
using HELIX.Widgets.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Layout {
    [UxmlElement]
    public partial class BoxShadow : PaintingWidget {

        public override void Paint(PaintCanvas canvas, Rect bounds) {
            canvas.painter.PathRect(bounds);
            canvas.painter.fillColor = Color.black;
            canvas.painter.Fill();
            canvas.painter.strokeColor = Color.white;
            canvas.painter.Stroke();
        }
    }
}
using HELIX.Painting;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual.PathDrawers {
    [UxmlObject]
    public partial class SolidFillPathDrawer : ScriptablePathDrawer {
        [Header("Solid Fill"), UxmlAttribute]
        public Color Color { get; set; } = Color.white;

        public override void Draw(PaintCanvas canvas) {
            canvas.painter.fillColor = Color;
            canvas.painter.Fill();
        }
    }
}
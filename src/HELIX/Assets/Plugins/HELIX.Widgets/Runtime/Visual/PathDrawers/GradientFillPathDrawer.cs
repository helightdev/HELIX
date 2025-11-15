using HELIX.Painting;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual.PathDrawers {
    [UxmlObject]
    public partial class GradientFillPathDrawer : ScriptablePathDrawer {
        [Header("Gradient Fill")]
        [UxmlObjectReference]
        public FillGradientGenerator GradientGenerator { get; set; } = new DirectionalLinearGradientGenerator();
        
        public override void Draw(PaintCanvas canvas) {
            if (GradientGenerator == null) return;
            var gradient = GradientGenerator.Generate(canvas);
            canvas.painter.fillGradient = gradient;
            canvas.painter.Fill();
        }
    }
}
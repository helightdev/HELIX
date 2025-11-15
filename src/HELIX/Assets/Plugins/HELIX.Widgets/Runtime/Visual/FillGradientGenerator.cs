using HELIX.Painting;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual {
    
    [UxmlObject]
    public abstract partial class FillGradientGenerator {
        public abstract FillGradient Generate(PaintCanvas canvas);
    }
}
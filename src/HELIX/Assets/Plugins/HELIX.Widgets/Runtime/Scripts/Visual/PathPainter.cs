using System.Collections.Generic;
using HELIX.Painting;
using HELIX.Painting.Paths;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual {
    [UxmlElement]
    public partial class PathPainter : PaintingWidget {
        
        [UxmlObjectReference("paint")]
        public ScriptablePaint Painter { get; set; }

        public override void Paint(PaintCanvas canvas, Rect bounds) {
            Painter.Draw(canvas, bounds);
        }
    }

    [UxmlObject]
    public abstract partial class ScriptablePathBuilder {
        public abstract void Build(IPathBuilder builder, Rect bounds);
    }

    [UxmlObject]
    public abstract partial class ScriptablePathDrawer {
        public abstract void Draw(PaintCanvas canvas);
    }
}
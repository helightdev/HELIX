using System.Collections.Generic;
using HELIX.Painting;
using HELIX.Painting.Paths;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual {
    [UxmlObject]
    public abstract partial class ScriptablePaint {
        public abstract void Draw(PaintCanvas canvas, Rect bounds);

        public void Draw(MeshGenerationContext context) {
            Draw(new PaintCanvas(context));
        }

        public void Draw(PaintCanvas canvas) {
            Draw(canvas, canvas.canvasRect);
        }
    }

    [UxmlObject]
    public partial class HiddenPaint : ScriptablePaint {
        public override void Draw(PaintCanvas canvas, Rect bounds) { }
    }

    [UxmlObject]
    public partial class ScriptablePainter : ScriptablePaint {
        public ScriptablePainter() { }

        public ScriptablePainter(
            List<ScriptablePathBuilder> builders,
            List<ScriptablePathDrawer> drawers,
            ScriptablePaint then = null
        ) {
            PathBuilders = builders;
            PathDrawers = drawers;
            Then = then;
        }

        public ScriptablePainter(
            ScriptablePathBuilder builder,
            ScriptablePathDrawer drawer,
            ScriptablePaint then = null
        ) {
            PathBuilders = new List<ScriptablePathBuilder> { builder };
            PathDrawers = new List<ScriptablePathDrawer> { drawer };
            Then = then;
        }

        [UxmlObjectReference("builders")]
        public List<ScriptablePathBuilder> PathBuilders { get; set; } = new();

        [UxmlObjectReference("drawers")]
        public List<ScriptablePathDrawer> PathDrawers { get; set; } = new();

        [UxmlObjectReference("then")]
        public ScriptablePaint Then { get; set; }

        public override void Draw(PaintCanvas canvas, Rect bounds) {
            if (PathBuilders?.Count == 0 || PathDrawers?.Count == 0) return;
            IPathBuilder.Draw(
                canvas.painter,
                builder => {
                    foreach (var pathBuilder in PathBuilders) pathBuilder?.Build(builder, bounds);
                    foreach (var drawer in PathDrawers) drawer?.Draw(canvas);
                }
            );
            Then?.Draw(canvas, bounds);
        }
    }
}
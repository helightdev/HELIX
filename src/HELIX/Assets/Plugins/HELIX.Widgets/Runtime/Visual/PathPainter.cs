using System;
using System.Collections.Generic;
using System.Numerics;
using HELIX.Painting;
using HELIX.Painting.Paths;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual {
    [UxmlElement]
    public partial class PathPainter : PaintingWidget {
        [UxmlObjectReference] public List<ScriptablePathBuilder> PathBuilders { get; set; } = new();
        [UxmlObjectReference] public List<ScriptablePathDrawer> PathDrawers { get; set; } = new();

        public override void Paint(PaintCanvas canvas, Rect bounds) {
            if (PathBuilders?.Count == 0 || PathDrawers?.Count == 0) return;
            IPathBuilder.Draw(canvas.painter, builder => {
                foreach (var pathBuilder in PathBuilders) pathBuilder?.Build(builder, bounds);
                foreach (var drawer in PathDrawers) drawer?.Draw(canvas);
            });
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
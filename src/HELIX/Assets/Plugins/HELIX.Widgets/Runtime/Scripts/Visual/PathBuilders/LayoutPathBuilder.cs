using HELIX.Extensions;
using HELIX.Painting.Paths;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual.PathBuilders {
    [UxmlObject]
    public partial class LayoutPathBuilder : ScriptablePathBuilder {
        [UxmlAttribute]
        public StyleLength Height { get; set; } = Length.Auto();

        [UxmlAttribute]
        public StyleLength Width { get; set; } = Length.Auto();

        [UxmlAttribute]
        public Vector2 WidthConstraints { get; set; } = new(-1, -1);

        [UxmlAttribute]
        public Vector2 HeightConstraints { get; set; } = new(-1, -1);

        [UxmlAttribute]
        public Alignment Alignment { get; set; } = Alignment.Center;

        [UxmlObjectReference]
        public ScriptablePathBuilder Builder { get; set; }

        public override void Build(IPathBuilder builder, Rect bounds) {
            bounds = bounds.LayoutSimple(Alignment, Width, Height, WidthConstraints, HeightConstraints);
            Builder?.Build(builder, bounds);
        }
    }
}
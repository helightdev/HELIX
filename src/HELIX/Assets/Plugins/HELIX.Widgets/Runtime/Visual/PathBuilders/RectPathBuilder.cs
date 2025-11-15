using HELIX.Painting.Paths;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual.PathBuilders {
    [UxmlObject]
    public partial class RectPathBuilder : ScriptablePathBuilder {
        [Header("Rectangle"), UxmlAttribute] public Rect Rect { get; set; } = default;

        [UxmlAttribute] public RectConstructionMode Mode { get; set; } = RectConstructionMode.Insets;

        public override void Build(IPathBuilder builder, Rect bounds) {
            var rect = Rect;
            switch (Mode) {
                case RectConstructionMode.Absolute:
                    break;
                case RectConstructionMode.Normalized:
                    rect.x *= bounds.width;
                    rect.y *= bounds.height;
                    rect.width *= bounds.width;
                    rect.height *= bounds.height;
                    break;
                case RectConstructionMode.Insets:
                    rect.width = bounds.width - rect.x - rect.width;
                    rect.height = bounds.height - rect.y - rect.height;
                    break;
            }
            builder.Rect(rect);
        }
    }
}
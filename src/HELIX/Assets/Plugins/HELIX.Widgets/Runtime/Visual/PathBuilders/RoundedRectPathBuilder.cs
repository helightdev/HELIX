using HELIX.Painting.Paths;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual.PathBuilders {
    [UxmlObject]
    public partial class RoundedRectPathBuilder : ScriptablePathBuilder {
        [Header("Rounded Rectangle"), UxmlAttribute] public Rect Rect { get; set; } = default;

        [UxmlAttribute] public RectConstructionMode Mode { get; set; } = RectConstructionMode.Insets;

        [UxmlAttribute]
        public Rect Corners {
            get => EditorUtilities.SwizzleToRect(Radii);
            set => Radii = EditorUtilities.SwizzleToVector4(value);
        }

        public Vector4 Radii { get; set; } = default;


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

            if (Corners == default) {
                builder.Rect(rect);
            } else {
                builder.RRect(new RRect(rect, Radii));
            }
        }
    }

    public enum RectConstructionMode {
        Insets = 0,
        Normalized = 1,
        Absolute = 2
    }
}
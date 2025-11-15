using HELIX.Painting;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual.GradientGenerators {
    [UxmlObject]
    public partial class RadialGradientGenerator : FillGradientGenerator {
        [UxmlAttribute] public Color InnerColor { get; set; } = Color.white;

        [UxmlAttribute] public Color OuterColor { get; set; } = Color.black;
        [UxmlAttribute] public Vector2 Center { get; set; } = new(0.5f, 0.5f);
        [UxmlAttribute] public Vector2 Focus { get; set; } = new(0.5f, 0.5f);
        [UxmlAttribute] public float Radius { get; set; } = 0.5f;
        [UxmlAttribute] public bool Normalized { get; set; } = true;
        [UxmlAttribute] public AddressMode AddressMode { get; set; } = AddressMode.Clamp;

        public override FillGradient Generate(PaintCanvas canvas) {
            if (Normalized) {
                return FillGradient.MakeRadialGradient(
                    InnerColor, OuterColor,
                    Center * canvas.size,
                    Radius * canvas.size.magnitude, Focus * canvas.size,
                    AddressMode
                );
            }

            return FillGradient.MakeRadialGradient(InnerColor, OuterColor, Center, Radius, Focus, AddressMode);
        }
    }
}
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
            var gradient = new Gradient {
                colorKeys = new[] { new GradientColorKey(InnerColor, 0f), new GradientColorKey(OuterColor, 1f) },
                alphaKeys = new[] { new GradientAlphaKey(InnerColor.a, 0f), new GradientAlphaKey(OuterColor.a, 1f) }
            };
            if (Normalized) {
                return FillGradient.MakeRadialGradient(
                    gradient,
                    Center * canvas.size,
                    Radius * canvas.size.magnitude, Focus * canvas.size,
                    AddressMode
                );
            }

            return FillGradient.MakeRadialGradient(gradient, Center, Radius, Focus, AddressMode);
        }
    }
}
using HELIX.Painting;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual.GradientGenerators {
    [UxmlObject]
    public partial class LinearGradientGenerator : FillGradientGenerator {
        [UxmlAttribute] public Color StartColor { get; set; } = Color.white;

        [UxmlAttribute] public Color EndColor { get; set; } = Color.black;
        [UxmlAttribute] public Vector2 StartPoint { get; set; } = Vector2.zero;
        [UxmlAttribute] public Vector2 EndPoint { get; set; } = new(1, 1);
        [UxmlAttribute] public bool Normalized { get; set; } = true;
        [UxmlAttribute] public AddressMode AddressMode { get; set; } = AddressMode.Clamp;

        public override FillGradient Generate(PaintCanvas canvas) {
            if (Normalized) {
                return FillGradient.MakeLinearGradient(
                    StartColor, EndColor,
                    StartPoint * canvas.size,
                    EndPoint * canvas.size, AddressMode
                );
            }

            return FillGradient.MakeLinearGradient(StartColor, EndColor, StartPoint, EndPoint, AddressMode);
        }
    }
}
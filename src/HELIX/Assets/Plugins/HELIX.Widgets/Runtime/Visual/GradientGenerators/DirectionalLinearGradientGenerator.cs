using HELIX.Painting;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual.PathDrawers {
    [UxmlObject]
    public partial class DirectionalLinearGradientGenerator : FillGradientGenerator {
        [UxmlAttribute] public Color StartColor { get; set; } = Color.white;

        [UxmlAttribute] public Color EndColor { get; set; } = Color.black;
        [UxmlAttribute] public float Angle { get; set; } = 0f;
        [UxmlAttribute] public AddressMode AddressMode { get; set; } = AddressMode.Clamp;

        public override FillGradient Generate(PaintCanvas canvas) {
            var direction = new Vector2(Mathf.Cos(Angle * Mathf.Deg2Rad), Mathf.Sin(Angle * Mathf.Deg2Rad));
            var center = canvas.size * 0.5f;
            var halfSize = canvas.size * 0.5f;
            var startPoint = center - Vector2.Scale(direction, halfSize);
            var endPoint = center + Vector2.Scale(direction, halfSize);
            return FillGradient.MakeLinearGradient(
                StartColor, EndColor,
                startPoint,
                endPoint, AddressMode
            );
        }
    }
}
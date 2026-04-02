using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual {
    public class SwatchVisualizer : BaseWidget {
        public SwatchVisualizer(ColorSwatch swatch) {
            this.FlexContainer(Axis.Horizontal, crossAxisAlign: Align.Stretch);
            for (var index = 0; index < swatch.weights.Length; index++) {
                var weight = swatch.weights[index];
                var lab = new LabColor(weight);
                var weightValue = ColorSwatch.WeightValues[index];

                new Label($"{weightValue.ToString()}\n{weight.ToHex()}")
                    .NoPaddingAndMargin().Sized(width: (1 / 11.0f).NormalizedPercent())
                    .TextColor(lab.l > 0.80f ? Color.black : Color.white).TextAlign(TextAnchor.MiddleCenter)
                    .BackgroundColor(weight).AddTo(this);
            }
        }
    }
}
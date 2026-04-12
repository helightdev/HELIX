using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Visual {
    public class SwatchVisualizer : BaseElement {
        public SwatchVisualizer(Color[] swatch) {
            this.FlexContainer(Axis.Horizontal, crossAxisAlign: Align.Stretch);
            for (var index = 0; index < swatch.Length; index++) {
                var weight = swatch[index];
                var lab = new LabColor(weight);
                var lch = (LchColor)lab;
                var weightValue = (uint)(index + 1);
                var timePerStep = 1 / (float)swatch.Length;
                new Label($"{weightValue.ToString()}\n{weight.ToHex()}\nOK L: {lch.l:P1} C: {lch.c:F3} H: {lch.h:F1}")
                    .NoPaddingAndMargin().Sized(timePerStep.NormalizedPercent())
                    .TextColor(lab.l > 0.80f ? Color.black : Color.white).TextAlign(TextAnchor.MiddleCenter)
                    .BackgroundColor(weight).AddTo(this);
            }
        }
    }
}
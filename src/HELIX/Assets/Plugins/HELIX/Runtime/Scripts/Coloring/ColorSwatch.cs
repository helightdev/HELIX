using UnityEngine;

namespace HELIX.Coloring {
    public class ColorSwatch {
        public static uint[] WeightValues = { 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950 };

        public readonly Color value;
        public readonly Color[] weights;

        public ColorSwatch(Color value, Color[] weights) {
            this.value = value;
            this.weights = weights;
        }

        public ColorSwatch(Color[] weights) {
            this.weights = weights;
            value = GetWeight(5);
        }

        private Color GetWeight(int index) {
            if (index < 0 || index >= weights.Length) return value;
            return weights[index];
        }

        public Color W50 => GetWeight(0);
        public Color W100 => GetWeight(1);
        public Color W200 => GetWeight(2);
        public Color W300 => GetWeight(3);
        public Color W400 => GetWeight(4);
        public Color W500 => GetWeight(5);
        public Color W600 => GetWeight(6);
        public Color W700 => GetWeight(7);
        public Color W800 => GetWeight(8);
        public Color W900 => GetWeight(9);
        public Color W950 => GetWeight(10);

        public static ColorSwatch FromSeed(Color gamma) {
            var lch = new LchColor(gamma.linear);
            var weights = CalculateWeights(lch.h);
            return new ColorSwatch(gamma, weights);
        }

        public static ColorSwatch FromHue(float hue, float chroma = 0.16f, float lumShift = 0) {
            var weights = CalculateWeights(hue, chroma, lumShift);
            return new ColorSwatch(weights);
        }

        public static ColorSwatch FromBalancedSeed(float l, float c, float h) {
            var centerLch = new LchColor(l, c, h);
            var center = (Color)(LabColor)centerLch;
            var weights = CalculateWeights(h, c, l - 0.69f);
            return new ColorSwatch(center.gamma, weights);
        }

        private static Color[] CalculateWeights(float hue, float chroma = 0.16f, float lumShift = 0) {
            var weights = new[] {
                new LchColor(0.98f, chroma * 0.072f, hue).ToGamma(), // 50
                new LchColor(0.95f + lumShift * 0.25f, chroma * 0.15f, hue).ToGamma(), // 100
                new LchColor(0.90f + lumShift * 0.5f, chroma * 0.34f, hue).ToGamma(), // 200
                new LchColor(0.83f + lumShift * 0.7f, chroma * 0.58f, hue).ToGamma(), // 300
                new LchColor(0.75f + lumShift * 0.9f, chroma * 0.82f, hue).ToGamma(), // 400

                new LchColor(0.69f + lumShift, chroma * 0.96f, hue).ToGamma(), // 500

                new LchColor(0.59f + lumShift * 0.8f, chroma * 1.00f, hue).ToGamma(), // 600
                new LchColor(0.51f + lumShift * 0.4f, chroma * 0.86f, hue).ToGamma(), // 700
                new LchColor(0.45f + lumShift * 0.2f, chroma * 0.72f, hue).ToGamma(), // 800
                new LchColor(0.37f + lumShift * 0.2f, chroma * 0.55f, hue).ToGamma(), // 900
                new LchColor(0.28f + lumShift * 0.1f, chroma * 0.37f, hue).ToGamma(), // 950
            };

            return weights;
        }

        public static implicit operator Color(ColorSwatch swatch) {
            return swatch.value;
        }
    }
}
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Coloring
{
    [Serializable]
    public sealed class RadixSwatch {
        public readonly string name;
        public readonly OkLchColor[] lightLch;
        public readonly OkLchColor[] darkLch;
        public readonly Color[] lightColors;
        public readonly Color[] darkColors;
        public readonly Color[] weight;

        public RadixPalette Light { get; }

        public RadixPalette Dark { get; }
        public Color Value => weight[5];

        public Color W50 => weight[0];
        public Color W100 => weight[1];
        public Color W200 => weight[2];
        public Color W300 => weight[3];
        public Color W400 => weight[4];
        public Color W500 => weight[5];

        public Color W600 => weight[6];
        public Color W700 => weight[7];
        public Color W800 => weight[8];
        public Color W900 => weight[9];
        public Color W950 => weight[10];

        public const int StepCount = 12;

        public static implicit operator Color(RadixSwatch swatch) => swatch.W500;
        public static implicit operator StyleColor(RadixSwatch swatch) => swatch.W500;

        public RadixSwatch(string name, string[] lightHex, string[] darkHex, float bias = 1f) {
            if (lightHex == null || lightHex.Length != StepCount) {
                throw new ArgumentException($"Light template '{name}' must contain exactly {StepCount} hex colors.");
            }

            if (darkHex == null || darkHex.Length != StepCount) {
                throw new ArgumentException($"Dark template '{name}' must contain exactly {StepCount} hex colors.");
            }

            this.name = name;
            lightColors = lightHex.Select(Colors.Hex).ToArray();
            darkColors = darkHex.Select(Colors.Hex).ToArray();
            lightLch = lightColors.Select(x => new OkLchColor(x.linear)).ToArray();
            darkLch = darkColors.Select(x => new OkLchColor(x.linear)).ToArray();
            
            Light = new RadixPalette(lightColors, Brightness.Light);
            Dark = new RadixPalette(darkColors, Brightness.Dark);

            var pivot = lightLch[8];
            weight = CalculateWeights(pivot.h, pivot.c, pivot.l - 0.69f);
        }

        public OkLchColor[] GetLch(Brightness brightness) => brightness == Brightness.Dark ? darkLch : lightLch;
        public Color[] GetColors(Brightness brightness) => brightness == Brightness.Dark ? darkColors : lightColors;
        public RadixPalette GetPalette(Brightness brightness) => brightness == Brightness.Dark ? Dark : Light;
        
        private static Color[] CalculateWeights(float hue, float chroma = 0.16f, float lumShift = 0) {
            var weights = new[] {
                new OkLchColor(0.98f, chroma * 0.072f, hue).ToGamma(), // 50
                new OkLchColor(0.95f + lumShift * 0.2f, chroma * 0.15f, hue).ToGamma(), // 100
                new OkLchColor(0.90f + lumShift * 0.5f, chroma * 0.34f, hue).ToGamma(), // 200
                new OkLchColor(0.83f + lumShift * 0.7f, chroma * 0.58f, hue).ToGamma(), // 300
                new OkLchColor(0.75f + lumShift * 0.9f, chroma * 0.82f, hue).ToGamma(), // 400

                new OkLchColor(0.69f + lumShift, chroma * 0.96f, hue).ToGamma(), // 500

                new OkLchColor(0.59f + lumShift * 0.8f, chroma * 1.00f, hue).ToGamma(), // 600
                new OkLchColor(0.51f + lumShift * 0.4f, chroma * 0.86f, hue).ToGamma(), // 700
                new OkLchColor(0.45f + lumShift * 0.2f, chroma * 0.72f, hue).ToGamma(), // 800
                new OkLchColor(0.37f + lumShift * 0.2f, chroma * 0.55f, hue).ToGamma(), // 900
                new OkLchColor(0.28f + lumShift * 0.1f, chroma * 0.37f, hue).ToGamma(), // 950
            };

            return weights;
        }
    }
}
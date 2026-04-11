using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HELIX.Coloring {
    public static class Colors {
        public static readonly ColorSwatch Red = new(
            new LchColor[] {
                new(0.971f, 0.013f, 17.38f), new(0.936f, 0.032f, 17.717f), new(0.885f, 0.062f, 18.334f),
                new(0.808f, 0.114f, 19.571f), new(0.704f, 0.191f, 22.216f), new(0.637f, 0.237f, 25.331f),
                new(0.577f, 0.245f, 27.325f), new(0.505f, 0.213f, 27.518f), new(0.444f, 0.177f, 26.899f),
                new(0.396f, 0.141f, 25.723f), new(0.258f, 0.092f, 26.042f),
            }
        );

        public static readonly ColorSwatch Orange = new(
            new LchColor[] {
                new(0.980f, 0.016f, 73.684f), new(0.954f, 0.038f, 75.164f), new(0.901f, 0.076f, 70.697f),
                new(0.837f, 0.128f, 66.29f), new(0.750f, 0.183f, 55.934f), new(0.705f, 0.213f, 47.604f),
                new(0.646f, 0.222f, 41.116f), new(0.553f, 0.195f, 38.402f), new(0.470f, 0.157f, 37.304f),
                new(0.408f, 0.123f, 38.172f), new(0.266f, 0.079f, 36.259f),
            }
        );

        public static readonly ColorSwatch Amber = new(
            new LchColor[] {
                new(0.987f, 0.022f, 95.277f), new(0.962f, 0.059f, 95.617f), new(0.924f, 0.120f, 95.746f),
                new(0.879f, 0.169f, 91.605f), new(0.828f, 0.189f, 84.429f), new(0.769f, 0.188f, 70.08f),
                new(0.666f, 0.179f, 58.318f), new(0.555f, 0.163f, 48.998f), new(0.473f, 0.137f, 46.201f),
                new(0.414f, 0.112f, 45.904f), new(0.279f, 0.077f, 45.635f),
            }
        );

        public static readonly ColorSwatch Yellow = new(
            new LchColor[] {
                new(0.987f, 0.026f, 102.212f), new(0.973f, 0.071f, 103.193f), new(0.945f, 0.129f, 101.54f),
                new(0.905f, 0.182f, 98.111f), new(0.852f, 0.199f, 91.936f), new(0.795f, 0.184f, 86.047f),
                new(0.681f, 0.162f, 75.834f), new(0.554f, 0.135f, 66.442f), new(0.476f, 0.114f, 61.907f),
                new(0.421f, 0.095f, 57.708f), new(0.286f, 0.066f, 53.813f),
            }
        );

        public static readonly ColorSwatch Lime = new(
            new LchColor[] {
                new(0.986f, 0.031f, 120.757f), new(0.967f, 0.067f, 122.328f), new(0.938f, 0.127f, 124.321f),
                new(0.897f, 0.196f, 126.665f), new(0.841f, 0.238f, 128.85f), new(0.768f, 0.233f, 130.85f),
                new(0.648f, 0.200f, 131.684f), new(0.532f, 0.157f, 131.589f), new(0.453f, 0.124f, 130.933f),
                new(0.405f, 0.101f, 131.063f), new(0.274f, 0.072f, 132.109f),
            }
        );

        public static readonly ColorSwatch Green = new(
            new LchColor[] {
                new(0.982f, 0.018f, 155.826f), new(0.962f, 0.044f, 156.743f), new(0.925f, 0.084f, 155.995f),
                new(0.871f, 0.150f, 154.449f), new(0.792f, 0.209f, 151.711f), new(0.723f, 0.219f, 149.579f),
                new(0.627f, 0.194f, 149.214f), new(0.527f, 0.154f, 150.069f), new(0.448f, 0.119f, 151.328f),
                new(0.393f, 0.095f, 152.535f), new(0.266f, 0.065f, 152.934f),
            }
        );

        public static readonly ColorSwatch Emerald = new(
            new LchColor[] {
                new(0.979f, 0.021f, 166.113f), new(0.950f, 0.052f, 163.051f), new(0.905f, 0.093f, 164.15f),
                new(0.845f, 0.143f, 164.978f), new(0.765f, 0.177f, 163.223f), new(0.696f, 0.170f, 162.48f),
                new(0.596f, 0.145f, 163.225f), new(0.508f, 0.118f, 165.612f), new(0.432f, 0.095f, 166.913f),
                new(0.378f, 0.077f, 168.94f), new(0.262f, 0.051f, 172.552f),
            }
        );

        public static readonly ColorSwatch Teal = new(
            new LchColor[] {
                new(0.984f, 0.014f, 180.72f), new(0.953f, 0.051f, 180.801f), new(0.910f, 0.096f, 180.426f),
                new(0.855f, 0.138f, 181.071f), new(0.777f, 0.152f, 181.912f), new(0.704f, 0.140f, 182.503f),
                new(0.600f, 0.118f, 184.704f), new(0.511f, 0.096f, 186.391f), new(0.437f, 0.078f, 188.216f),
                new(0.386f, 0.063f, 188.416f), new(0.277f, 0.046f, 192.524f),
            }
        );

        public static readonly ColorSwatch Cyan = new(
            new LchColor[] {
                new(0.984f, 0.019f, 200.873f), new(0.956f, 0.045f, 203.388f), new(0.917f, 0.080f, 205.041f),
                new(0.865f, 0.127f, 207.078f), new(0.789f, 0.154f, 211.53f), new(0.715f, 0.143f, 215.221f),
                new(0.609f, 0.126f, 221.723f), new(0.520f, 0.105f, 223.128f), new(0.450f, 0.085f, 224.283f),
                new(0.398f, 0.070f, 227.392f), new(0.302f, 0.056f, 229.695f),
            }
        );

        public static readonly ColorSwatch Sky = new(
            new LchColor[] {
                new(0.977f, 0.013f, 236.62f), new(0.951f, 0.026f, 236.824f), new(0.901f, 0.058f, 230.902f),
                new(0.828f, 0.111f, 230.318f), new(0.746f, 0.160f, 232.661f), new(0.685f, 0.169f, 237.323f),
                new(0.588f, 0.158f, 241.966f), new(0.500f, 0.134f, 242.749f), new(0.443f, 0.110f, 240.79f),
                new(0.391f, 0.090f, 240.876f), new(0.293f, 0.066f, 243.157f),
            }
        );

        public static readonly ColorSwatch Blue = new(
            new LchColor[] {
                new(0.970f, 0.014f, 254.604f), new(0.932f, 0.032f, 255.585f), new(0.882f, 0.059f, 254.128f),
                new(0.809f, 0.105f, 251.813f), new(0.707f, 0.165f, 254.624f), new(0.623f, 0.214f, 259.815f),
                new(0.546f, 0.245f, 262.881f), new(0.488f, 0.243f, 264.376f), new(0.424f, 0.199f, 265.638f),
                new(0.379f, 0.146f, 265.522f), new(0.282f, 0.091f, 267.935f),
            }
        );

        public static readonly ColorSwatch Indigo = new(
            new LchColor[] {
                new(0.962f, 0.018f, 272.314f), new(0.930f, 0.034f, 272.788f), new(0.870f, 0.065f, 274.039f),
                new(0.785f, 0.115f, 274.713f), new(0.673f, 0.182f, 276.935f), new(0.585f, 0.233f, 277.117f),
                new(0.511f, 0.262f, 276.966f), new(0.457f, 0.240f, 277.023f), new(0.398f, 0.195f, 277.366f),
                new(0.359f, 0.144f, 278.697f), new(0.257f, 0.090f, 281.288f),
            }
        );

        public static readonly ColorSwatch Violet = new(
            new LchColor[] {
                new(0.969f, 0.016f, 293.756f), new(0.943f, 0.029f, 294.588f), new(0.894f, 0.057f, 293.283f),
                new(0.811f, 0.111f, 293.571f), new(0.702f, 0.183f, 293.541f), new(0.606f, 0.250f, 292.717f),
                new(0.541f, 0.281f, 293.009f), new(0.491f, 0.270f, 292.581f), new(0.432f, 0.232f, 292.759f),
                new(0.380f, 0.189f, 293.745f), new(0.283f, 0.141f, 291.089f),
            }
        );

        public static readonly ColorSwatch Purple = new(
            new LchColor[] {
                new(0.977f, 0.014f, 308.299f), new(0.946f, 0.033f, 307.174f), new(0.902f, 0.063f, 306.703f),
                new(0.827f, 0.119f, 306.383f), new(0.714f, 0.203f, 305.504f), new(0.627f, 0.265f, 303.9f),
                new(0.558f, 0.288f, 302.321f), new(0.496f, 0.265f, 301.924f), new(0.438f, 0.218f, 303.724f),
                new(0.381f, 0.176f, 304.987f), new(0.291f, 0.149f, 302.717f),
            }
        );

        public static readonly ColorSwatch Fuchsia = new(
            new LchColor[] {
                new(0.977f, 0.017f, 320.058f), new(0.952f, 0.037f, 318.852f), new(0.903f, 0.076f, 319.62f),
                new(0.833f, 0.145f, 321.434f), new(0.740f, 0.238f, 322.16f), new(0.667f, 0.295f, 322.15f),
                new(0.591f, 0.293f, 322.896f), new(0.518f, 0.253f, 323.949f), new(0.452f, 0.211f, 324.591f),
                new(0.401f, 0.170f, 325.612f), new(0.293f, 0.136f, 325.661f),
            }
        );

        public static readonly ColorSwatch Pink = new(
            new LchColor[] {
                new(0.971f, 0.014f, 343.198f), new(0.948f, 0.028f, 342.258f), new(0.899f, 0.061f, 343.231f),
                new(0.823f, 0.120f, 346.018f), new(0.718f, 0.202f, 349.761f), new(0.656f, 0.241f, 354.308f),
                new(0.592f, 0.249f, 0.584f), new(0.525f, 0.223f, 3.958f), new(0.459f, 0.187f, 3.815f),
                new(0.408f, 0.153f, 2.432f), new(0.284f, 0.109f, 3.907f),
            }
        );

        public static readonly ColorSwatch Rose = new(
            new LchColor[] {
                new(0.969f, 0.015f, 12.422f), new(0.941f, 0.030f, 12.58f), new(0.892f, 0.058f, 10.001f),
                new(0.810f, 0.117f, 11.638f), new(0.712f, 0.194f, 13.428f), new(0.645f, 0.246f, 16.439f),
                new(0.586f, 0.253f, 17.585f), new(0.514f, 0.222f, 16.935f), new(0.455f, 0.188f, 13.697f),
                new(0.410f, 0.159f, 10.272f), new(0.271f, 0.105f, 12.094f),
            }
        );

        public static readonly ColorSwatch Slate = new(
            new LchColor[] {
                new(0.984f, 0.003f, 247.858f), new(0.968f, 0.007f, 247.896f), new(0.929f, 0.013f, 255.508f),
                new(0.869f, 0.022f, 252.894f), new(0.704f, 0.040f, 256.788f), new(0.554f, 0.046f, 257.417f),
                new(0.446f, 0.043f, 257.281f), new(0.372f, 0.044f, 257.287f), new(0.279f, 0.041f, 260.031f),
                new(0.208f, 0.042f, 265.755f), new(0.129f, 0.042f, 264.695f),
            }
        );

        public static readonly ColorSwatch Gray = new(
            new LchColor[] {
                new(0.985f, 0.002f, 247.839f), new(0.967f, 0.003f, 264.542f), new(0.928f, 0.006f, 264.531f),
                new(0.872f, 0.010f, 258.338f), new(0.707f, 0.022f, 261.325f), new(0.551f, 0.027f, 264.364f),
                new(0.446f, 0.030f, 256.802f), new(0.373f, 0.034f, 259.733f), new(0.278f, 0.033f, 256.848f),
                new(0.210f, 0.034f, 264.665f), new(0.130f, 0.028f, 261.692f),
            }
        );

        public static readonly ColorSwatch Zinc = new(
            new LchColor[] {
                new(0.985f, 0.000f, 0f), new(0.967f, 0.001f, 286.375f), new(0.920f, 0.004f, 286.32f),
                new(0.871f, 0.006f, 286.286f), new(0.705f, 0.015f, 286.067f), new(0.552f, 0.016f, 285.938f),
                new(0.442f, 0.017f, 285.786f), new(0.370f, 0.013f, 285.805f), new(0.274f, 0.006f, 286.033f),
                new(0.210f, 0.006f, 285.885f), new(0.141f, 0.005f, 285.823f),
            }
        );

        public static readonly ColorSwatch Neutral = new(
            new LchColor[] {
                new(0.985f, 0.000f, 0f), new(0.970f, 0.000f, 0f), new(0.922f, 0.000f, 0f), new(0.870f, 0.000f, 0f),
                new(0.708f, 0.000f, 0f), new(0.556f, 0.000f, 0f), new(0.439f, 0.000f, 0f), new(0.371f, 0.000f, 0f),
                new(0.269f, 0.000f, 0f), new(0.205f, 0.000f, 0f), new(0.145f, 0.000f, 0f),
            }
        );

        public static readonly ColorSwatch Stone = new(
            new LchColor[] {
                new(0.985f, 0.001f, 106.423f), new(0.970f, 0.001f, 106.424f), new(0.923f, 0.003f, 48.717f),
                new(0.869f, 0.005f, 56.366f), new(0.709f, 0.010f, 56.259f), new(0.553f, 0.013f, 58.071f),
                new(0.444f, 0.011f, 73.639f), new(0.374f, 0.010f, 67.558f), new(0.268f, 0.007f, 34.298f),
                new(0.216f, 0.006f, 56.043f), new(0.147f, 0.004f, 49.25f),
            }
        );

        public static readonly ColorSwatch Mauve = new(
            new LchColor[] {
                new(0.985f, 0.000f, 0f), new(0.960f, 0.003f, 325.6f), new(0.922f, 0.005f, 325.62f),
                new(0.865f, 0.012f, 325.68f), new(0.711f, 0.019f, 323.02f), new(0.542f, 0.034f, 322.5f),
                new(0.435f, 0.029f, 321.78f), new(0.364f, 0.029f, 323.89f), new(0.263f, 0.024f, 320.12f),
                new(0.212f, 0.019f, 322.12f), new(0.145f, 0.008f, 326f),
            }
        );

        public static readonly ColorSwatch Olive = new(
            new LchColor[] {
                new(0.988f, 0.003f, 106.5f), new(0.966f, 0.005f, 106.5f), new(0.930f, 0.007f, 106.5f),
                new(0.880f, 0.011f, 106.6f), new(0.737f, 0.021f, 106.9f), new(0.580f, 0.031f, 107.3f),
                new(0.466f, 0.025f, 107.3f), new(0.394f, 0.023f, 107.4f), new(0.286f, 0.016f, 107.4f),
                new(0.228f, 0.013f, 107.4f), new(0.153f, 0.006f, 107.1f),
            }
        );

        public static readonly ColorSwatch Mist = new(
            new LchColor[] {
                new(0.987f, 0.002f, 197.1f), new(0.963f, 0.002f, 197.1f), new(0.925f, 0.005f, 214.3f),
                new(0.872f, 0.007f, 219.6f), new(0.723f, 0.014f, 214.4f), new(0.560f, 0.021f, 213.5f),
                new(0.450f, 0.017f, 213.2f), new(0.378f, 0.015f, 216f), new(0.275f, 0.011f, 216.9f),
                new(0.218f, 0.008f, 223.9f), new(0.148f, 0.004f, 228.8f),
            }
        );

        public static readonly ColorSwatch Taupe = new(
            new LchColor[] {
                new(0.986f, 0.002f, 67.8f), new(0.960f, 0.002f, 17.2f), new(0.922f, 0.005f, 34.3f),
                new(0.868f, 0.007f, 39.5f), new(0.714f, 0.014f, 41.2f), new(0.547f, 0.021f, 43.1f),
                new(0.438f, 0.017f, 39.3f), new(0.367f, 0.016f, 35.7f), new(0.268f, 0.011f, 36.5f),
                new(0.214f, 0.009f, 43.1f), new(0.147f, 0.004f, 49.3f),
            }
        );

        public static readonly Color Transparent = new(0, 0, 0, 0);
        
        public static readonly Color Black = new(0, 0, 0);
        public static readonly Color Black05 = new(0, 0, 0, 0.05f);
        public static readonly Color Black10 = new(0, 0, 0, 0.10f);
        public static readonly Color Black15 = new(0, 0, 0, 0.15f);
        public static readonly Color Black20 = new(0, 0, 0, 0.20f);
        public static readonly Color Black30 = new(0, 0, 0, 0.30f);
        public static readonly Color Black40 = new(0, 0, 0, 0.40f);
        public static readonly Color Black50 = new(0, 0, 0, 0.50f);
        public static readonly Color Black60 = new(0, 0, 0, 0.60f);
        public static readonly Color Black70 = new(0, 0, 0, 0.70f);
        public static readonly Color Black80 = new(0, 0, 0, 0.80f);
        public static readonly Color Black90 = new(0, 0, 0, 0.90f);
        public static readonly Color Black95 = new(0, 0, 0, 0.95f);
        
        
        public static readonly Color White = new(1, 1, 1);
        public static readonly Color White05 = new(1, 1, 1, 0.05f);
        public static readonly Color White10 = new(1, 1, 1, 0.10f);
        public static readonly Color White15 = new(1, 1, 1, 0.15f);
        public static readonly Color White20 = new(1, 1, 1, 0.20f);
        public static readonly Color White30 = new(1, 1, 1, 0.30f);
        public static readonly Color White40 = new(1, 1, 1, 0.40f);
        public static readonly Color White50 = new(1, 1, 1, 0.50f);
        public static readonly Color White60 = new(1, 1, 1, 0.60f);
        public static readonly Color White70 = new(1, 1, 1, 0.70f);
        public static readonly Color White80 = new(1, 1, 1, 0.80f);
        public static readonly Color White90 = new(1, 1, 1, 0.90f);
        public static readonly Color White95 = new(1, 1, 1, 0.95f);

        public static readonly Dictionary<string, ColorSwatch> Named = new() {
            { "Red", Red },
            { "Orange", Orange },
            { "Amber", Amber },
            { "Yellow", Yellow },
            { "Lime", Lime },
            { "Green", Green },
            { "Emerald", Emerald },
            { "Teal", Teal },
            { "Cyan", Cyan },
            { "Sky", Sky },
            { "Blue", Blue },
            { "Indigo", Indigo },
            { "Violet", Violet },
            { "Purple", Purple },
            { "Fuchsia", Fuchsia },
            { "Pink", Pink },
            { "Rose", Rose },
            { "Slate", Slate },
            { "Gray", Gray },
            { "Zinc", Zinc },
            { "Neutral", Neutral },
            { "Stone", Stone },
            { "Taupe", Taupe },
            { "Mauve", Mauve },
            { "Mist", Mist },
            { "Olive", Olive }
        };

        public static ColorSwatch[] All = Named.Values.ToArray();

        public static Color Hex(string hex) {
            if (ColorUtility.TryParseHtmlString(hex, out var color)) return color;
            throw new ArgumentException($"Invalid hex color: {hex}");
        }

        public static Color Rgb(int r, int g, int b) {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        public static Color Argb(int a, int r, int g, int b) {
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        public static Color Lch(float l, float c, float h) {
            var lch = new LchColor(l, c, h);
            var color = (Color)lch;
            return color.gamma;
        }

        public static Color Lab(float l, float a, float b) {
            var lab = new LabColor(l, a, b);
            var color = (Color)lab;
            return color.gamma;
        }

        public static Color Hsv(float h, float s, float v) {
            return Color.HSVToRGB(h, s, v);
        }

        public static Color AlphaBlend(Color background, Color foreground) {
            var alpha = foreground.a;
            var backAlpha = background.a;
            var invAlpha = 1 - alpha;
            if (alpha == 0) return background;
            if (Mathf.Approximately(backAlpha, 1)) {
                return new Color(
                    foreground.r * alpha + background.r * invAlpha,
                    foreground.g * alpha + background.g * invAlpha,
                    foreground.b * alpha + background.b * invAlpha,
                    1
                );
            }

            backAlpha *= invAlpha;
            var outAlpha = alpha + backAlpha;
            return new Color(
                (foreground.r * alpha + background.r * backAlpha) / outAlpha,
                (foreground.g * alpha + background.g * backAlpha) / outAlpha,
                (foreground.b * alpha + background.b * backAlpha) / outAlpha,
                outAlpha
            );
        }

        public static Color WithOpacity(this Color color, float alpha) {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static Color MultiplyOpacity(this Color color, float alpha) {
            return new Color(color.r, color.g, color.b, color.a * alpha);
        }

        public static float ComputeLuminance(this Color gamma) {
            var linear = gamma.linear;
            return 0.2126f * linear.r + 0.7152f * linear.g + 0.0722f * linear.b;
        }

        public static string ToHex(this Color color) {
            var r = Mathf.Clamp01(color.r);
            var g = Mathf.Clamp01(color.g);
            var b = Mathf.Clamp01(color.b);
            var a = Mathf.Clamp01(color.a);
            return Mathf.Approximately(a, 1f)
                ? $"#{Mathf.RoundToInt(r * 255):X2}{Mathf.RoundToInt(g * 255):X2}{Mathf.RoundToInt(b * 255):X2}"
                : $"#{Mathf.RoundToInt(r * 255):X2}{Mathf.RoundToInt(g * 255):X2}{Mathf.RoundToInt(b * 255):X2}{Mathf.RoundToInt(a * 255):X2}";
        }
    }
}
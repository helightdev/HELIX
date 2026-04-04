using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HELIX.Coloring {
    public static class Colors {
        public static readonly ColorSwatch Red = ColorSwatch.FromBalancedSeed(0.637f, 0.237f, 25.331f);
        public static readonly ColorSwatch Orange = ColorSwatch.FromBalancedSeed(0.705f, 0.213f, 47.604f);
        public static readonly ColorSwatch Amber = ColorSwatch.FromBalancedSeed(0.769f, 0.188f, 70.08f);
        public static readonly ColorSwatch Yellow = ColorSwatch.FromBalancedSeed(0.795f, 0.184f, 86.047f);
        public static readonly ColorSwatch Lime = ColorSwatch.FromBalancedSeed(0.768f, 0.233f, 130.85f);
        public static readonly ColorSwatch Green = ColorSwatch.FromBalancedSeed(0.723f, 0.219f, 149.579f);
        public static readonly ColorSwatch Emerald = ColorSwatch.FromBalancedSeed(0.696f, 0.17f, 162.48f);
        public static readonly ColorSwatch Teal = ColorSwatch.FromBalancedSeed(0.704f, 0.14f, 182.50f);
        public static readonly ColorSwatch Cyan = ColorSwatch.FromBalancedSeed(0.715f, 0.143f, 214.221f);
        public static readonly ColorSwatch Sky = ColorSwatch.FromBalancedSeed(0.685f, 0.169f, 237.323f);
        public static readonly ColorSwatch Blue = ColorSwatch.FromBalancedSeed(0.623f, 0.214f, 259.815f);
        public static readonly ColorSwatch Indigo = ColorSwatch.FromBalancedSeed(0.585f, 0.233f, 277.117f);
        public static readonly ColorSwatch Violet = ColorSwatch.FromBalancedSeed(0.606f, 0.25f, 292.717f);
        public static readonly ColorSwatch Purple = ColorSwatch.FromBalancedSeed(0.627f, 0.265f, 303.9f);
        public static readonly ColorSwatch Fuchsia = ColorSwatch.FromBalancedSeed(0.667f, 0.295f, 322.15f);
        public static readonly ColorSwatch Pink = ColorSwatch.FromBalancedSeed(0.656f, 0.241f, 354.308f);
        public static readonly ColorSwatch Rose = ColorSwatch.FromBalancedSeed(0.645f, 0.246f, 16.439f);

        public static readonly ColorSwatch Slate = ColorSwatch.FromHue(257, 0.046f);
        public static readonly ColorSwatch Gray = ColorSwatch.FromHue(264, 0.027f);
        public static readonly ColorSwatch Zinc = ColorSwatch.FromHue(285, 0.016f);
        public static readonly ColorSwatch Neutral = ColorSwatch.FromHue(0, 0);
        public static readonly ColorSwatch Stone = ColorSwatch.FromHue(58, 0.013f);
        public static readonly ColorSwatch Taupe = ColorSwatch.FromHue(43, 0.021f);
        public static readonly ColorSwatch Mauve = ColorSwatch.FromHue(322, 0.034f);
        public static readonly ColorSwatch Mist = ColorSwatch.FromHue(213, 0.021f);
        public static readonly ColorSwatch Olive = ColorSwatch.FromHue(107, 0.031f);
        
        public static readonly Color Transparent = new(0, 0, 0, 0);
        public static readonly Color Black = new(0, 0, 0);
        public static readonly Color White = new(1, 1, 1);

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
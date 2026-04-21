using System;
using HELIX.Coloring.Material;
using UnityEngine;

namespace HELIX.Coloring {
  public static class Colors {

    public static Color Red => MaterialColors.Red;
    public static Color Green => MaterialColors.Green;
    public static Color Blue => MaterialColors.Blue;
    public static Color Yellow => MaterialColors.Yellow;
    public static Color Cyan => MaterialColors.Cyan;
    public static Color Pink => MaterialColors.Pink;
    public static Color Purple => MaterialColors.Purple;
    public static Color DeepPurple => MaterialColors.DeepPurple;
    public static Color Indigo => MaterialColors.Indigo;
    public static Color LightBlue => MaterialColors.LightBlue;
    public static Color Teal => MaterialColors.Teal;
    public static Color Orange => MaterialColors.Orange;
    public static Color DeepOrange => MaterialColors.DeepOrange;
    public static Color Brown => MaterialColors.Brown;
    public static Color Grey => MaterialColors.Grey;
    public static Color BlueGrey => MaterialColors.BlueGrey;
    public static Color LightGreen => MaterialColors.LightGreen;
    public static Color Lime => MaterialColors.Lime;
    public static Color Amber => MaterialColors.Amber;

    /// <summary>
    /// A gray color with no alpha. Aims to mitigate the effect of straight alpha blending.
    /// Lerping/Blending with this color will most often yield better results than defaulting to black transparent.
    /// </summary>
    public static readonly Color Transparent = new Color(0.5f, 0.5f, 0.5f, 0);

    public static readonly Color BlackTransparent = new(0, 0, 0, 0);
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

    public static readonly Color WhiteTransparent = new(1, 1, 1, 0);
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

    public static Color Argb(int argb) {
      return argb.ArgbToColor();
    }

    public static Color Argb(uint argb) {
      return argb.ArgbToColor();
    }

    public static Color OkLch(float l, float c, float h) {
      var lch = new OkLchColor(l, c, h);
      var color = (Color)lch;
      return color.gamma;
    }

    public static Color OkLab(float l, float a, float b) {
      var lab = new OkLabColor(l, a, b);
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
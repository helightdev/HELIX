using System;
using Unity.Mathematics;
using UnityEngine;

namespace HELIX.Coloring {
  public static class Colors {
    public static readonly Color Red = OkLch(0.637f, 0.237f, 25.331f);
    public static readonly Color Green = OkLch(0.723f, 0.219f, 149.579f);
    public static readonly Color Blue = OkLch(0.623f, 0.214f, 259.815f);
    public static readonly Color Yellow = OkLch(0.795f, 0.184f, 86.047f);
    public static readonly Color Cyan = OkLch(0.715f, 0.143f, 215.221f);
    public static readonly Color Pink = OkLch(0.656f, 0.241f, 354.308f);
    public static readonly Color Purple = OkLch(0.627f, 0.265f, 303.9f);
    public static readonly Color DeepPurple = OkLch(0.606f, 0.25f, 292.717f);
    public static readonly Color Indigo = OkLch(0.585f, 0.233f, 277.117f);
    public static readonly Color LightBlue = OkLch(0.685f, 0.169f, 237.323f);
    public static readonly Color Teal = OkLch(0.704f, 0.14f, 182.503f);
    public static readonly Color Orange = OkLch(0.705f, 0.213f, 47.604f);
    public static readonly Color DeepOrange = OkLch(0.705f, 0.213f, 47.604f);
    public static readonly Color Brown = OkLch(0.547f, 0.021f, 43.1f);
    public static readonly Color Grey = OkLch(0.551f, 0.027f, 264.364f);
    public static readonly Color BlueGrey = OkLch(0.554f, 0.046f, 257.417f);
    public static readonly Color LightGreen = OkLch(0.768f, 0.233f, 130.85f);
    public static readonly Color Lime = OkLch(0.768f, 0.233f, 130.85f);
    public static readonly Color Amber = OkLch(0.769f, 0.188f, 70.08f);

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

    public static Color Rgb(float3 rgb) {
      return new Color(rgb.x, rgb.y, rgb.z);
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

    /// <summary>
    ///   Converts a Unity gamma-space Color into ARGB.
    /// </summary>
    public static int ToArgb(this Color color) {
      var a = Mathf.Clamp(Mathf.RoundToInt(color.a * 255.0f), 0, 255);
      var r = Mathf.Clamp(Mathf.RoundToInt(color.r * 255.0f), 0, 255);
      var g = Mathf.Clamp(Mathf.RoundToInt(color.g * 255.0f), 0, 255);
      var b = Mathf.Clamp(Mathf.RoundToInt(color.b * 255.0f), 0, 255);

      return (a << 24)
        | (r << 16)
        | (g << 8)
        | b;
    }

    /// <summary>
    ///   Converts an ARGB integer into a Unity gamma-space Color.
    /// </summary>
    public static Color ArgbToColor(this int argb) {
      var a = ((argb >> 24) & 255) / 255.0f;
      var r = ((argb >> 16) & 255) / 255.0f;
      var g = ((argb >> 8) & 255) / 255.0f;
      var b = (argb & 255) / 255.0f;
      return new Color(r, g, b, a);
    }

    /// <summary>
    ///   Converts an ARGB integer into a Unity gamma-space Color.
    /// </summary>
    public static Color ArgbToColor(this uint argb) {
      return unchecked((int)argb).ArgbToColor();
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

    public static Color Hsl(float h, float s, float l) {
      var v = l + s * Mathf.Min(l, 1 - l);
      var sv = v == 0 ? 0 : 2 * (1 - l / v);
      return Color.HSVToRGB(h, sv, v);
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

    public static Color Lerp(Color from, Color to, float t) => Color.Lerp(from, to, t);

    public static Color ContrastBlend(
      Color background,
      Color overlay,
      float time
    ) {
      if (background.a < 0.5f) return AlphaBlend(background, overlay.MultiplyOpacity(time));

      var a = background.ToOkLab();
      var b = overlay.ToOkLab();
      return ContrastBlend(a, b, time).ToGamma();
    }

    public static OkLabColor ContrastBlend(
      OkLabColor background,
      OkLabColor overlay,
      float time
    ) {
      const float leeway = 0.2f;
      var dl = overlay.l - background.l;
      if (math.distance(background, overlay) <= math.EPSILON) return background;
      var movement = math.min(time, math.abs(dl));
      var l = background.l + math.sign(dl) * movement;
      var t = math.clamp((l - background.l) / dl, time - leeway, time + leeway);
      return math.lerp(background, overlay, t);
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

    public static OkLabColor ToOkLab(this Color gamma) {
      var linear = gamma.linear;
      return OkLabHelper.FromLinearRgb(new float3(linear.r, linear.g, linear.b));
    }

    public static float3 ToRgb(this Color color) => new(color.r, color.g, color.b);
  }
}

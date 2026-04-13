using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace MaterialColorUtilities
{
    /// <summary>
    /// Unity-facing helpers and extensions for Material Color Utilities.
    ///
    /// This file is intended to sit on top of the lower-level ported types:
    /// Hct, Cam16, Blend, DynamicScheme, DynamicColor, TonalPalette,
    /// MaterialDynamicColors, Contrast, Variant, and the scheme preset classes.
    /// </summary>
    public static class MaterialUnity
    {
        /// <summary>
        /// Creates a dynamic scheme from an ARGB seed color and a preset variant.
        /// </summary>
        public static DynamicScheme CreateScheme(
            int sourceArgb,
            Variant variant = Variant.TonalSpot,
            bool isDark = false,
            double contrastLevel = 0.0)
        {
            return CreateScheme(Hct.FromInt(sourceArgb), variant, isDark, contrastLevel);
        }

        /// <summary>
        /// Creates a dynamic scheme from a Unity gamma-space color and a preset variant.
        /// </summary>
        public static DynamicScheme CreateScheme(
            Color sourceColor,
            Variant variant = Variant.TonalSpot,
            bool isDark = false,
            double contrastLevel = 0.0)
        {
            return CreateScheme(sourceColor.ToArgb(), variant, isDark, contrastLevel);
        }

        /// <summary>
        /// Creates a dynamic scheme from an HCT seed color and a preset variant.
        /// </summary>
        public static DynamicScheme CreateScheme(
            Hct sourceColorHct,
            Variant variant = Variant.TonalSpot,
            bool isDark = false,
            double contrastLevel = 0.0)
        {
            return variant switch
            {
                Variant.Monochrome => new SchemeMonochrome(sourceColorHct, isDark, contrastLevel),
                Variant.Neutral => new SchemeNeutral(sourceColorHct, isDark, contrastLevel),
                Variant.TonalSpot => new SchemeTonalSpot(sourceColorHct, isDark, contrastLevel),
                Variant.Vibrant => new SchemeVibrant(sourceColorHct, isDark, contrastLevel),
                Variant.Expressive => new SchemeExpressive(sourceColorHct, isDark, contrastLevel),
                Variant.Content => new SchemeContent(sourceColorHct, isDark, contrastLevel),
                Variant.Fidelity => new SchemeFidelity(sourceColorHct, isDark, contrastLevel),
                Variant.Rainbow => new SchemeRainbow(sourceColorHct, isDark, contrastLevel),
                Variant.FruitSalad => new SchemeFruitSalad(sourceColorHct, isDark, contrastLevel),
                _ => new SchemeTonalSpot(sourceColorHct, isDark, contrastLevel),
            };
        }

        /// <summary>
        /// Creates a MaterialColor wrapper from an ARGB color.
        /// </summary>
        public static MaterialColor CreateMaterialColor(int argb, string name = null)
        {
            return new MaterialColor(argb, name);
        }

        /// <summary>
        /// Creates a MaterialColor wrapper from a Unity gamma-space color.
        /// </summary>
        public static MaterialColor CreateMaterialColor(Color color, string name = null)
        {
            return new MaterialColor(color.ToArgb(), name);
        }

        /// <summary>
        /// Creates a MaterialColor wrapper from a tonal palette.
        /// </summary>
        public static MaterialColor CreateMaterialColor(TonalPalette palette, string name = null)
        {
            return new MaterialColor(palette, name);
        }
    }

    /// <summary>
    /// A Unity-friendly wrapper similar to Flutter's MaterialColor.
    /// Provides swatch access over a tonal palette.
    /// </summary>
    public sealed class MaterialColor
    {
        private static readonly int[] DefaultSwatchWeights =
        {
            50, 100, 200, 300, 400, 500, 600, 700, 800, 900
        };

        public readonly string Name;

        private readonly TonalPalette _tonalPalette;
        private readonly Hct _seed;

        public MaterialColor(int argb, string name = null)
        {
            Name = name ?? string.Empty;
            _seed = Hct.FromInt(argb);
            _tonalPalette = TonalPalette.FromHct(_seed);
        }

        public MaterialColor(Hct hct, string name = null)
        {
            Name = name ?? string.Empty;
            _seed = hct;
            _tonalPalette = TonalPalette.FromHct(_seed);
        }

        public MaterialColor(TonalPalette tonalPalette, string name = null)
        {
            Name = name ?? string.Empty;
            _tonalPalette = tonalPalette ?? throw new ArgumentNullException(nameof(tonalPalette));
            _seed = tonalPalette.KeyColor;
        }

        public Color this[int weight] => _tonalPalette.GetHct(WeightToTone(weight)).ToColor();

        public Color Shade50 => this[50];
        public Color Shade100 => this[100];
        public Color Shade200 => this[200];
        public Color Shade300 => this[300];
        public Color Shade400 => this[400];
        public Color Shade500 => this[500];
        public Color Shade600 => this[600];
        public Color Shade700 => this[700];
        public Color Shade800 => this[800];
        public Color Shade900 => this[900];

        public Color Value => Shade500;
        public TonalPalette Palette => _tonalPalette;
        public Hct Seed => _seed;

        /// <summary>
        /// Returns a standard 10-step Material-style swatch.
        /// Order: 50,100,200,300,400,500,600,700,800,900.
        /// </summary>
        public Color[] ToSwatch()
        {
            Color[] colors = new Color[DefaultSwatchWeights.Length];
            for (int i = 0; i < DefaultSwatchWeights.Length; i++)
            {
                colors[i] = this[DefaultSwatchWeights[i]];
            }

            return colors;
        }

        /// <summary>
        /// Returns a swatch for arbitrary Material-style weights.
        /// </summary>
        public Color[] ToSwatch(params int[] weights)
        {
            if (weights == null || weights.Length == 0)
            {
                return ToSwatch();
            }

            Color[] colors = new Color[weights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                colors[i] = this[weights[i]];
            }

            return colors;
        }

        /// <summary>
        /// Returns a swatch for arbitrary HCT tone values.
        /// </summary>
        public Color[] ToToneSwatch(params double[] tones)
        {
            if (tones == null || tones.Length == 0)
            {
                return ToSwatch();
            }

            Color[] colors = new Color[tones.Length];
            for (int i = 0; i < tones.Length; i++)
            {
                colors[i] = _tonalPalette.GetHct(tones[i]).ToColor();
            }

            return colors;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name)
                ? $"MaterialColor({_seed})"
                : $"MaterialColor(Name=\"{Name}\", Value={_seed})";
        }

        public static implicit operator Color(MaterialColor materialColor) => materialColor.Value;
        public static implicit operator StyleColor(MaterialColor materialColor) => materialColor.Value;
        public static implicit operator Hct(MaterialColor materialColor) => materialColor._seed;
        public static implicit operator TonalPalette(MaterialColor materialColor) => materialColor.Palette;

        private static double WeightToTone(int weight)
        {
            return weight switch
            {
                50 => 95.0,
                100 => 90.0,
                200 => 80.0,
                300 => 70.0,
                400 => 60.0,
                500 => 50.0,
                600 => 40.0,
                700 => 30.0,
                800 => 20.0,
                900 => 10.0,
                _ => (1000 - weight) / 10.0,
            };
        }
    }

    /// <summary>
    /// Extension methods for working with Unity colors and Material Color Utilities.
    /// Unity Color values are treated as gamma-space colors.
    /// </summary>
    public static class MaterialColorExtensions
    {
        /// <summary>
        /// Converts an ARGB integer into a Unity gamma-space Color.
        /// </summary>
        public static Color ArgbToColor(this int argb)
        {
            float a = ColorUtils.AlphaFromArgb(argb) / 255.0f;
            float r = ColorUtils.RedFromArgb(argb) / 255.0f;
            float g = ColorUtils.GreenFromArgb(argb) / 255.0f;
            float b = ColorUtils.BlueFromArgb(argb) / 255.0f;
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Converts an ARGB integer into a UI Toolkit StyleColor.
        /// </summary>
        public static StyleColor ArgbToStyleColor(this int argb)
        {
            return new StyleColor(argb.ArgbToColor());
        }

        /// <summary>
        /// Converts an ARGB integer into HCT.
        /// </summary>
        public static Hct ArgbToHct(this int argb)
        {
            return Hct.FromInt(argb);
        }

        /// <summary>
        /// Converts an ARGB integer into a tonal palette.
        /// </summary>
        public static TonalPalette ArgbToTonalPalette(this int argb)
        {
            return TonalPalette.FromHct(Hct.FromInt(argb));
        }

        /// <summary>
        /// Converts an ARGB integer into a MaterialColor wrapper.
        /// </summary>
        public static MaterialColor ArgbToMaterialColor(this int argb, string name = null)
        {
            return new MaterialColor(argb, name);
        }

        /// <summary>
        /// Converts a Unity gamma-space Color into ARGB.
        /// </summary>
        public static int ToArgb(this Color color)
        {
            int a = Mathf.Clamp(Mathf.RoundToInt(color.a * 255.0f), 0, 255);
            int r = Mathf.Clamp(Mathf.RoundToInt(color.r * 255.0f), 0, 255);
            int g = Mathf.Clamp(Mathf.RoundToInt(color.g * 255.0f), 0, 255);
            int b = Mathf.Clamp(Mathf.RoundToInt(color.b * 255.0f), 0, 255);

            return (a << 24)
                 | (r << 16)
                 | (g << 8)
                 | b;
        }

        /// <summary>
        /// Converts a Unity gamma-space Color into HCT.
        /// </summary>
        public static Hct ToHct(this Color color)
        {
            return Hct.FromInt(color.ToArgb());
        }

        /// <summary>
        /// Converts a Unity gamma-space Color into a tonal palette.
        /// </summary>
        public static TonalPalette ToTonalPalette(this Color color)
        {
            return TonalPalette.FromHct(color.ToHct());
        }

        /// <summary>
        /// Converts a Unity gamma-space Color into a MaterialColor wrapper.
        /// </summary>
        public static MaterialColor ToMaterialColor(this Color color, string name = null)
        {
            return new MaterialColor(color.ToArgb(), name);
        }

        /// <summary>
        /// Converts an HCT color into a Unity gamma-space Color.
        /// </summary>
        public static Color ToColor(this Hct hct)
        {
            return hct.ToInt().ArgbToColor();
        }

        /// <summary>
        /// Converts an HCT color into a UI Toolkit StyleColor.
        /// </summary>
        public static StyleColor ToStyleColor(this Hct hct)
        {
            return new StyleColor(hct.ToColor());
        }

        /// <summary>
        /// Converts an HCT color into a MaterialColor wrapper.
        /// </summary>
        public static MaterialColor ToMaterialColor(this Hct hct, string name = null)
        {
            return new MaterialColor(hct, name);
        }

        /// <summary>
        /// Converts a tonal palette into a MaterialColor wrapper.
        /// </summary>
        public static MaterialColor ToMaterialColor(this TonalPalette tonalPalette, string name = null)
        {
            return new MaterialColor(tonalPalette, name);
        }

        /// <summary>
        /// Returns the key color of a tonal palette as a Unity gamma-space Color.
        /// </summary>
        public static Color ToColor(this TonalPalette tonalPalette)
        {
            return tonalPalette.KeyColor.ToColor();
        }

        /// <summary>
        /// Returns the key color of a tonal palette as a UI Toolkit StyleColor.
        /// </summary>
        public static StyleColor ToStyleColor(this TonalPalette tonalPalette)
        {
            return new StyleColor(tonalPalette.KeyColor.ToColor());
        }

        /// <summary>
        /// Creates a dynamic scheme from an ARGB integer.
        /// </summary>
        public static DynamicScheme CreateScheme(
            this int sourceArgb,
            Variant variant = Variant.TonalSpot,
            bool isDark = false,
            double contrastLevel = 0.0)
        {
            return MaterialUnity.CreateScheme(sourceArgb, variant, isDark, contrastLevel);
        }

        /// <summary>
        /// Creates a dynamic scheme from a Unity gamma-space Color.
        /// </summary>
        public static DynamicScheme CreateScheme(
            this Color sourceColor,
            Variant variant = Variant.TonalSpot,
            bool isDark = false,
            double contrastLevel = 0.0)
        {
            return MaterialUnity.CreateScheme(sourceColor, variant, isDark, contrastLevel);
        }

        /// <summary>
        /// Creates a dynamic scheme from an HCT color.
        /// </summary>
        public static DynamicScheme CreateScheme(
            this Hct sourceColorHct,
            Variant variant = Variant.TonalSpot,
            bool isDark = false,
            double contrastLevel = 0.0)
        {
            return MaterialUnity.CreateScheme(sourceColorHct, variant, isDark, contrastLevel);
        }

        /// <summary>
        /// Creates a dynamic scheme from a preset variant and ARGB seed.
        /// </summary>
        public static DynamicScheme CreateScheme(
            this Variant variant,
            int sourceArgb,
            bool isDark = false,
            double contrastLevel = 0.0)
        {
            return MaterialUnity.CreateScheme(sourceArgb, variant, isDark, contrastLevel);
        }

        /// <summary>
        /// Creates a dynamic scheme from a preset variant and Unity gamma-space Color seed.
        /// </summary>
        public static DynamicScheme CreateScheme(
            this Variant variant,
            Color sourceColor,
            bool isDark = false,
            double contrastLevel = 0.0)
        {
            return MaterialUnity.CreateScheme(sourceColor, variant, isDark, contrastLevel);
        }

        /// <summary>
        /// Creates a dynamic scheme from a preset variant and HCT seed.
        /// </summary>
        public static DynamicScheme CreateScheme(
            this Variant variant,
            Hct sourceColorHct,
            bool isDark = false,
            double contrastLevel = 0.0)
        {
            return MaterialUnity.CreateScheme(sourceColorHct, variant, isDark, contrastLevel);
        }

        /// <summary>
        /// Resolves a dynamic role to ARGB from a scheme.
        /// </summary>
        public static int GetArgb(this DynamicScheme scheme, DynamicColor role)
        {
            return role.GetArgb(scheme);
        }

        /// <summary>
        /// Resolves a dynamic role to HCT from a scheme.
        /// </summary>
        public static Hct GetHct(this DynamicScheme scheme, DynamicColor role)
        {
            return role.GetHct(scheme);
        }

        /// <summary>
        /// Resolves a dynamic role to a Unity gamma-space Color from a scheme.
        /// </summary>
        public static Color GetColor(this DynamicScheme scheme, DynamicColor role)
        {
            return role.GetArgb(scheme).ArgbToColor();
        }

        /// <summary>
        /// Resolves a dynamic role to a UI Toolkit StyleColor from a scheme.
        /// </summary>
        public static StyleColor GetStyleColor(this DynamicScheme scheme, DynamicColor role)
        {
            return new StyleColor(role.GetArgb(scheme).ArgbToColor());
        }

        /// <summary>
        /// Returns the primary color of a scheme as a Unity gamma-space Color.
        /// </summary>
        public static Color PrimaryColor(this DynamicScheme scheme) => scheme.Primary.ArgbToColor();

        /// <summary>
        /// Returns the secondary color of a scheme as a Unity gamma-space Color.
        /// </summary>
        public static Color SecondaryColor(this DynamicScheme scheme) => scheme.Secondary.ArgbToColor();

        /// <summary>
        /// Returns the tertiary color of a scheme as a Unity gamma-space Color.
        /// </summary>
        public static Color TertiaryColor(this DynamicScheme scheme) => scheme.Tertiary.ArgbToColor();

        /// <summary>
        /// Returns the background color of a scheme as a Unity gamma-space Color.
        /// </summary>
        public static Color BackgroundColor(this DynamicScheme scheme) => scheme.Background.ArgbToColor();

        /// <summary>
        /// Returns the surface color of a scheme as a Unity gamma-space Color.
        /// </summary>
        public static Color SurfaceColor(this DynamicScheme scheme) => scheme.Surface.ArgbToColor();

        /// <summary>
        /// Returns the on-surface color of a scheme as a Unity gamma-space Color.
        /// </summary>
        public static Color OnSurfaceColor(this DynamicScheme scheme) => scheme.OnSurface.ArgbToColor();

        /// <summary>
        /// Harmonizes one ARGB color toward another.
        /// </summary>
        public static int HarmonizeArgb(this int designArgb, int sourceArgb)
        {
            return Blend.Harmonize(designArgb, sourceArgb);
        }

        /// <summary>
        /// Harmonizes one Unity gamma-space Color toward another.
        /// </summary>
        public static Color Harmonize(this Color designColor, Color sourceColor)
        {
            return Blend.Harmonize(designColor.ToArgb(), sourceColor.ToArgb()).ArgbToColor();
        }

        /// <summary>
        /// Blends hue in HCT from one ARGB color toward another.
        /// </summary>
        public static int BlendHctHueArgb(this int fromArgb, int toArgb, double amount)
        {
            return Blend.HctHue(fromArgb, toArgb, amount);
        }

        /// <summary>
        /// Blends hue in HCT from one Unity gamma-space Color toward another.
        /// </summary>
        public static Color BlendHctHue(this Color fromColor, Color toColor, double amount)
        {
            return Blend.HctHue(fromColor.ToArgb(), toColor.ToArgb(), amount).ArgbToColor();
        }

        /// <summary>
        /// Blends in CAM16-UCS from one ARGB color toward another.
        /// </summary>
        public static int BlendCam16UcsArgb(this int fromArgb, int toArgb, double amount)
        {
            return Blend.Cam16Ucs(fromArgb, toArgb, amount);
        }

        /// <summary>
        /// Blends in CAM16-UCS from one Unity gamma-space Color toward another.
        /// </summary>
        public static Color BlendCam16Ucs(this Color fromColor, Color toColor, double amount)
        {
            return Blend.Cam16Ucs(fromColor.ToArgb(), toColor.ToArgb(), amount).ArgbToColor();
        }

        /// <summary>
        /// Creates a new HCT color at the same hue/chroma but a different tone.
        /// </summary>
        public static Hct WithTone(this Hct hct, double tone)
        {
            return Hct.From(hct.Hue, hct.Chroma, tone);
        }

        /// <summary>
        /// Creates a new HCT color at the same chroma/tone but a different hue.
        /// </summary>
        public static Hct WithHue(this Hct hct, double hue)
        {
            return Hct.From(hue, hct.Chroma, hct.Tone);
        }

        /// <summary>
        /// Creates a new HCT color at the same hue/tone but a different chroma.
        /// </summary>
        public static Hct WithChroma(this Hct hct, double chroma)
        {
            return Hct.From(hct.Hue, chroma, hct.Tone);
        }

        /// <summary>
        /// Creates a new Unity gamma-space Color by changing tone in HCT.
        /// </summary>
        public static Color WithTone(this Color color, double tone)
        {
            return color.ToHct().WithTone(tone).ToColor();
        }

        /// <summary>
        /// Creates a new Unity gamma-space Color by changing hue in HCT.
        /// </summary>
        public static Color WithHue(this Color color, double hue)
        {
            return color.ToHct().WithHue(hue).ToColor();
        }

        /// <summary>
        /// Creates a new Unity gamma-space Color by changing chroma in HCT.
        /// </summary>
        public static Color WithChroma(this Color color, double chroma)
        {
            return color.ToHct().WithChroma(chroma).ToColor();
        }

        /// <summary>
        /// Converts an ARGB integer to a #AARRGGBB string.
        /// </summary>
        public static string ToIntArgbHex(this int argb)
        {
            return $"#{argb:X8}";
        }
    }
}
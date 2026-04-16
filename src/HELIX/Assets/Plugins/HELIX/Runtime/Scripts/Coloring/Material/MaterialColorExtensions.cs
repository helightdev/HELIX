using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Coloring.Material
{
    /// <summary>
    ///     Extension methods for working with Unity colors and Material Color Utilities.
    ///     Unity Color values are treated as gamma-space colors.
    /// </summary>
    public static class MaterialColorExtensions {
        /// <summary>
        ///     Converts an ARGB integer into a Unity gamma-space Color.
        /// </summary>
        public static Color ArgbToColor(this int argb) {
            var a = MaterialColorUtils.AlphaFromArgb(argb) / 255.0f;
            var r = MaterialColorUtils.RedFromArgb(argb) / 255.0f;
            var g = MaterialColorUtils.GreenFromArgb(argb) / 255.0f;
            var b = MaterialColorUtils.BlueFromArgb(argb) / 255.0f;
            return new Color(r, g, b, a);
        }
        
        /// <summary>
        ///     Converts an ARGB integer into a Unity gamma-space Color.
        /// </summary>
        public static Color ArgbToColor(this uint argb) {
            return ArgbToColor(unchecked((int)argb));
        }

        /// <summary>
        ///     Converts an ARGB integer into HCT.
        /// </summary>
        public static Hct ArgbToHct(this int argb) {
            return Hct.FromInt(argb);
        }
        
        /// <summary>
        ///     Converts an ARGB integer into HCT.
        /// </summary>
        public static Hct ArgbToHct(this uint argb) {
            return Hct.FromInt(unchecked((int)argb));
        }

        /// <summary>
        ///     Converts an ARGB integer into a tonal palette.
        /// </summary>
        public static TonalPalette ArgbToTonalPalette(this int argb) {
            return TonalPalette.FromHct(Hct.FromInt(argb));
        }

        /// <summary>
        ///     Converts an ARGB integer into a MaterialColor wrapper.
        /// </summary>
        public static MaterialColor ArgbToMaterialColor(this int argb, string name = null) {
            return new TonalMaterialColor(argb, name);
        }

        /// <summary>
        ///     Converts a Unity gamma-space Color into ARGB.
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
        ///     Converts a Unity gamma-space Color into HCT.
        /// </summary>
        public static Hct ToHct(this Color color) {
            return Hct.FromInt(color.ToArgb());
        }

        /// <summary>
        ///     Converts a Unity gamma-space Color into a tonal palette.
        /// </summary>
        public static TonalPalette ToTonalPalette(this Color color) {
            return TonalPalette.FromHct(color.ToHct());
        }

        /// <summary>
        ///     Converts a Unity gamma-space Color into a MaterialColor wrapper.
        /// </summary>
        public static MaterialColor ToMaterialColor(this Color color, string name = null) {
            return new TonalMaterialColor(color.ToArgb(), name);
        }

        /// <summary>
        ///     Converts an HCT color into a Unity gamma-space Color.
        /// </summary>
        public static Color ToColor(this Hct hct) {
            return hct.ToInt().ArgbToColor();
        }

        /// <summary>
        ///     Converts an HCT color into a UI Toolkit StyleColor.
        /// </summary>
        public static StyleColor ToStyleColor(this Hct hct) {
            return new StyleColor(hct.ToColor());
        }

        /// <summary>
        ///     Converts an HCT color into a MaterialColor wrapper.
        /// </summary>
        public static MaterialColor ToMaterialColor(this Hct hct, string name = null) {
            return new TonalMaterialColor(hct, name);
        }

        /// <summary>
        ///     Converts a tonal palette into a MaterialColor wrapper.
        /// </summary>
        public static MaterialColor ToMaterialColor(this TonalPalette tonalPalette, string name = null) {
            return new TonalMaterialColor(tonalPalette, name);
        }

        /// <summary>
        ///     Returns the key color of a tonal palette as a Unity gamma-space Color.
        /// </summary>
        public static Color ToColor(this TonalPalette tonalPalette) {
            return tonalPalette.KeyColor.ToColor();
        }

        /// <summary>
        ///     Returns the key color of a tonal palette as a UI Toolkit StyleColor.
        /// </summary>
        public static StyleColor ToStyleColor(this TonalPalette tonalPalette) {
            return new StyleColor(tonalPalette.KeyColor.ToColor());
        }

        /// <summary>
        ///     Creates a dynamic scheme from an ARGB integer.
        /// </summary>
        public static DynamicScheme CreateScheme(
            this int sourceArgb,
            Variant variant = Variant.TonalSpot,
            bool isDark = false,
            double contrastLevel = 0.0
        ) {
            return MaterialSchemeFactory.CreateScheme(sourceArgb, variant, isDark, contrastLevel);
        }

        /// <summary>
        ///     Creates a dynamic scheme from a Unity gamma-space Color.
        /// </summary>
        public static DynamicScheme CreateScheme(
            this Color sourceColor,
            Variant variant = Variant.TonalSpot,
            bool isDark = false,
            double contrastLevel = 0.0
        ) {
            return MaterialSchemeFactory.CreateScheme(sourceColor, variant, isDark, contrastLevel);
        }

        /// <summary>
        ///     Creates a dynamic scheme from an HCT color.
        /// </summary>
        public static DynamicScheme CreateScheme(
            this Hct sourceColorHct,
            Variant variant = Variant.TonalSpot,
            bool isDark = false,
            double contrastLevel = 0.0
        ) {
            return MaterialSchemeFactory.CreateScheme(sourceColorHct, variant, isDark, contrastLevel);
        }

        /// <summary>
        ///     Creates a dynamic scheme from a preset variant and ARGB seed.
        /// </summary>
        public static DynamicScheme CreateScheme(
            this Variant variant,
            int sourceArgb,
            bool isDark = false,
            double contrastLevel = 0.0
        ) {
            return MaterialSchemeFactory.CreateScheme(sourceArgb, variant, isDark, contrastLevel);
        }

        /// <summary>
        ///     Creates a dynamic scheme from a preset variant and Unity gamma-space Color seed.
        /// </summary>
        public static DynamicScheme CreateScheme(
            this Variant variant,
            Color sourceColor,
            bool isDark = false,
            double contrastLevel = 0.0
        ) {
            return MaterialSchemeFactory.CreateScheme(sourceColor, variant, isDark, contrastLevel);
        }

        /// <summary>
        ///     Creates a dynamic scheme from a preset variant and HCT seed.
        /// </summary>
        public static DynamicScheme CreateScheme(
            this Variant variant,
            Hct sourceColorHct,
            bool isDark = false,
            double contrastLevel = 0.0
        ) {
            return MaterialSchemeFactory.CreateScheme(sourceColorHct, variant, isDark, contrastLevel);
        }

        /// <summary>
        ///     Resolves a dynamic role to ARGB from a scheme.
        /// </summary>
        public static int GetArgb(this DynamicScheme scheme, DynamicColor role) {
            return role.GetArgb(scheme);
        }

        /// <summary>
        ///     Resolves a dynamic role to HCT from a scheme.
        /// </summary>
        public static Hct GetHct(this DynamicScheme scheme, DynamicColor role) {
            return role.GetHct(scheme);
        }

        /// <summary>
        ///     Resolves a dynamic role to a Unity gamma-space Color from a scheme.
        /// </summary>
        public static Color GetColor(this DynamicScheme scheme, DynamicColor role) {
            return role.GetArgb(scheme).ArgbToColor();
        }

        /// <summary>
        ///     Harmonizes one ARGB color toward another.
        /// </summary>
        public static int HarmonizeArgb(this int designArgb, int sourceArgb) {
            return Blend.Harmonize(designArgb, sourceArgb);
        }

        /// <summary>
        ///     Harmonizes one Unity gamma-space Color toward another.
        /// </summary>
        public static Color Harmonize(this Color designColor, Color sourceColor) {
            return Blend.Harmonize(designColor.ToArgb(), sourceColor.ToArgb()).ArgbToColor();
        }

        /// <summary>
        ///     Blends hue in HCT from one ARGB color toward another.
        /// </summary>
        public static int BlendHctHueArgb(this int fromArgb, int toArgb, double amount) {
            return Blend.HctHue(fromArgb, toArgb, amount);
        }

        /// <summary>
        ///     Blends hue in HCT from one Unity gamma-space Color toward another.
        /// </summary>
        public static Color BlendHctHue(this Color fromColor, Color toColor, double amount) {
            return Blend.HctHue(fromColor.ToArgb(), toColor.ToArgb(), amount).ArgbToColor();
        }

        /// <summary>
        ///     Blends in CAM16-UCS from one ARGB color toward another.
        /// </summary>
        public static int BlendCam16UcsArgb(this int fromArgb, int toArgb, double amount) {
            return Blend.Cam16Ucs(fromArgb, toArgb, amount);
        }

        /// <summary>
        ///     Blends in CAM16-UCS from one Unity gamma-space Color toward another.
        /// </summary>
        public static Color BlendCam16Ucs(this Color fromColor, Color toColor, double amount) {
            return Blend.Cam16Ucs(fromColor.ToArgb(), toColor.ToArgb(), amount).ArgbToColor();
        }

        /// <summary>
        ///     Creates a new HCT color at the same hue/chroma but a different tone.
        /// </summary>
        public static Hct WithTone(this Hct hct, double tone) {
            return Hct.From(hct.Hue, hct.Chroma, tone);
        }

        /// <summary>
        ///     Creates a new HCT color at the same chroma/tone but a different hue.
        /// </summary>
        public static Hct WithHue(this Hct hct, double hue) {
            return Hct.From(hue, hct.Chroma, hct.Tone);
        }

        /// <summary>
        ///     Creates a new HCT color at the same hue/tone but a different chroma.
        /// </summary>
        public static Hct WithChroma(this Hct hct, double chroma) {
            return Hct.From(hct.Hue, chroma, hct.Tone);
        }

        /// <summary>
        ///     Creates a new Unity gamma-space Color by changing tone in HCT.
        /// </summary>
        public static Color WithTone(this Color color, double tone) {
            return color.ToHct().WithTone(tone).ToColor();
        }

        /// <summary>
        ///     Creates a new Unity gamma-space Color by changing hue in HCT.
        /// </summary>
        public static Color WithHue(this Color color, double hue) {
            return color.ToHct().WithHue(hue).ToColor();
        }

        /// <summary>
        ///     Creates a new Unity gamma-space Color by changing chroma in HCT.
        /// </summary>
        public static Color WithChroma(this Color color, double chroma) {
            return color.ToHct().WithChroma(chroma).ToColor();
        }

        /// <summary>
        ///     Converts an ARGB integer to a #AARRGGBB string.
        /// </summary>
        public static string ToIntArgbHex(this int argb) {
            return $"#{argb:X8}";
        }
    }
}
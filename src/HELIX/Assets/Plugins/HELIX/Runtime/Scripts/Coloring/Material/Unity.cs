using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Coloring.Material {
    /// <summary>
    ///     Unity-facing helpers and extensions for Material Color Utilities.
    ///     This file is intended to sit on top of the lower-level ported types:
    ///     Hct, Cam16, Blend, DynamicScheme, DynamicColor, TonalPalette,
    ///     MaterialDynamicColors, Contrast, Variant, and the scheme preset classes.
    /// </summary>
    public static class MaterialUnity {
        /// <summary>
        ///     Creates a dynamic scheme from an ARGB seed color and a preset variant.
        /// </summary>
        public static DynamicScheme CreateScheme(
            int sourceArgb,
            Variant variant = Variant.TonalSpot,
            bool isDark = false,
            double contrastLevel = 0.0
        ) {
            return CreateScheme(Hct.FromInt(sourceArgb), variant, isDark, contrastLevel);
        }

        /// <summary>
        ///     Creates a dynamic scheme from a Unity gamma-space color and a preset variant.
        /// </summary>
        public static DynamicScheme CreateScheme(
            Color sourceColor,
            Variant variant = Variant.TonalSpot,
            bool isDark = false,
            double contrastLevel = 0.0
        ) {
            return CreateScheme(sourceColor.ToArgb(), variant, isDark, contrastLevel);
        }

        /// <summary>
        ///     Creates a dynamic scheme from an HCT seed color and a preset variant.
        /// </summary>
        public static DynamicScheme CreateScheme(
            Hct sourceColorHct,
            Variant variant = Variant.TonalSpot,
            bool isDark = false,
            double contrastLevel = 0.0
        ) {
            return variant switch {
                Variant.Monochrome => new SchemeMonochrome(sourceColorHct, isDark, contrastLevel),
                Variant.Neutral    => new SchemeNeutral(sourceColorHct, isDark, contrastLevel),
                Variant.TonalSpot  => new SchemeTonalSpot(sourceColorHct, isDark, contrastLevel),
                Variant.Vibrant    => new SchemeVibrant(sourceColorHct, isDark, contrastLevel),
                Variant.Expressive => new SchemeExpressive(sourceColorHct, isDark, contrastLevel),
                Variant.Content    => new SchemeContent(sourceColorHct, isDark, contrastLevel),
                Variant.Fidelity   => new SchemeFidelity(sourceColorHct, isDark, contrastLevel),
                Variant.Rainbow    => new SchemeRainbow(sourceColorHct, isDark, contrastLevel),
                Variant.FruitSalad => new SchemeFruitSalad(sourceColorHct, isDark, contrastLevel),
                _                  => new SchemeTonalSpot(sourceColorHct, isDark, contrastLevel)
            };
        }

        /// <summary>
        ///     Creates a MaterialColor wrapper from an ARGB color.
        /// </summary>
        public static MaterialColorSwatch CreateMaterialColor(int argb, string name = null) {
            return new TonalMaterialColor(argb, name);
        }

        /// <summary>
        ///     Creates a MaterialColor wrapper from a Unity gamma-space color.
        /// </summary>
        public static MaterialColorSwatch CreateMaterialColor(Color color, string name = null) {
            return new TonalMaterialColor(color.ToArgb(), name);
        }

        /// <summary>
        ///     Creates a MaterialColor wrapper from a tonal palette.
        /// </summary>
        public static MaterialColorSwatch CreateMaterialColor(TonalPalette palette, string name = null) {
            return new TonalMaterialColor(palette, name);
        }
    }

    /// <summary>
    ///     Extension methods for working with Unity colors and Material Color Utilities.
    ///     Unity Color values are treated as gamma-space colors.
    /// </summary>
    public static class MaterialColorExtensions {
        /// <summary>
        ///     Converts an ARGB integer into a Unity gamma-space Color.
        /// </summary>
        public static Color ArgbToColor(this int argb) {
            var a = ColorUtils.AlphaFromArgb(argb) / 255.0f;
            var r = ColorUtils.RedFromArgb(argb) / 255.0f;
            var g = ColorUtils.GreenFromArgb(argb) / 255.0f;
            var b = ColorUtils.BlueFromArgb(argb) / 255.0f;
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
        public static MaterialColorSwatch ArgbToMaterialColor(this int argb, string name = null) {
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
        public static MaterialColorSwatch ToMaterialColor(this Color color, string name = null) {
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
        public static MaterialColorSwatch ToMaterialColor(this Hct hct, string name = null) {
            return new TonalMaterialColor(hct, name);
        }

        /// <summary>
        ///     Converts a tonal palette into a MaterialColor wrapper.
        /// </summary>
        public static MaterialColorSwatch ToMaterialColor(this TonalPalette tonalPalette, string name = null) {
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
            return MaterialUnity.CreateScheme(sourceArgb, variant, isDark, contrastLevel);
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
            return MaterialUnity.CreateScheme(sourceColor, variant, isDark, contrastLevel);
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
            return MaterialUnity.CreateScheme(sourceColorHct, variant, isDark, contrastLevel);
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
            return MaterialUnity.CreateScheme(sourceArgb, variant, isDark, contrastLevel);
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
            return MaterialUnity.CreateScheme(sourceColor, variant, isDark, contrastLevel);
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
            return MaterialUnity.CreateScheme(sourceColorHct, variant, isDark, contrastLevel);
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
        ///     Resolves a dynamic role to a UI Toolkit StyleColor from a scheme.
        /// </summary>
        public static StyleColor GetStyleColor(this DynamicScheme scheme, DynamicColor role) {
            return new StyleColor(role.GetArgb(scheme).ArgbToColor());
        }

        /// <summary>
        ///     Returns the primary color of a scheme as a Unity gamma-space Color.
        /// </summary>
        public static Color PrimaryColor(this DynamicScheme scheme) {
            return scheme.Primary.ArgbToColor();
        }

        /// <summary>
        ///     Returns the secondary color of a scheme as a Unity gamma-space Color.
        /// </summary>
        public static Color SecondaryColor(this DynamicScheme scheme) {
            return scheme.Secondary.ArgbToColor();
        }

        /// <summary>
        ///     Returns the tertiary color of a scheme as a Unity gamma-space Color.
        /// </summary>
        public static Color TertiaryColor(this DynamicScheme scheme) {
            return scheme.Tertiary.ArgbToColor();
        }

        /// <summary>
        ///     Returns the background color of a scheme as a Unity gamma-space Color.
        /// </summary>
        public static Color BackgroundColor(this DynamicScheme scheme) {
            return scheme.Background.ArgbToColor();
        }

        /// <summary>
        ///     Returns the surface color of a scheme as a Unity gamma-space Color.
        /// </summary>
        public static Color SurfaceColor(this DynamicScheme scheme) {
            return scheme.Surface.ArgbToColor();
        }

        /// <summary>
        ///     Returns the on-surface color of a scheme as a Unity gamma-space Color.
        /// </summary>
        public static Color OnSurfaceColor(this DynamicScheme scheme) {
            return scheme.OnSurface.ArgbToColor();
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

    /// <summary>
    ///     Base type for color swatches.
    ///     Supports a primary value, indexed shades, and conversion to Unity colors.
    /// </summary>
    public abstract class MaterialSwatch {
        public readonly string name;

        protected MaterialSwatch(int primaryArgb, string name = null) {
            PrimaryArgb = primaryArgb;
            this.name = name ?? string.Empty;
        }

        protected MaterialSwatch(uint primaryArgb, string name = null) : this(unchecked((int)primaryArgb), name) { }

        /// <summary>
        ///     Primary ARGB value of the swatch. For normal swatches this is typically shade 500.
        ///     For accent swatches this is typically shade 200.
        /// </summary>
        public int PrimaryArgb { get; }

        public Color Value => PrimaryArgb.ArgbToColor();
        public StyleColor StyleValue => Value;

        /// <summary>
        ///     Ordered list of valid shade weights for this swatch.
        /// </summary>
        public abstract ReadOnlySpan<int> Weights { get; }

        /// <summary>
        ///     Resolves a shade by weight.
        /// </summary>
        public abstract Color this[int weight] { get; }

        /// <summary>
        ///     Resolves a shade ARGB by weight.
        /// </summary>
        public virtual int GetArgb(int weight) {
            return this[weight].ToArgb();
        }

        /// <summary>
        ///     Returns all shades in the natural order of this swatch type.
        /// </summary>
        public Color[] ToSwatch() {
            var weights = Weights;
            var colors = new Color[weights.Length];
            for (var i = 0; i < weights.Length; i++) colors[i] = this[weights[i]];
            return colors;
        }

        /// <summary>
        ///     Returns shades for arbitrary weights.
        /// </summary>
        public Color[] ToSwatch(params int[] weights) {
            if (weights == null || weights.Length == 0) return ToSwatch();

            var colors = new Color[weights.Length];
            for (var i = 0; i < weights.Length; i++) colors[i] = this[weights[i]];
            return colors;
        }

        public override string ToString() {
            return string.IsNullOrEmpty(name)
                ? $"{GetType().Name}({PrimaryArgb.ToIntArgbHex()})"
                : $"{GetType().Name}(Name=\"{name}\", Value={PrimaryArgb.ToIntArgbHex()})";
        }

        public static implicit operator Color(MaterialSwatch swatch) {
            return swatch.Value;
        }

        public static implicit operator StyleColor(MaterialSwatch swatch) {
            return swatch.Value;
        }
    }

    /// <summary>
    ///     Base class for standard Material-style swatches with weights:
    ///     50, 100, 200, 300, 400, 500, 600, 700, 800, 900.
    /// </summary>
    public abstract class MaterialColorSwatch : MaterialSwatch {
        private static readonly int[] _weights = { 50, 100, 200, 300, 400, 500, 600, 700, 800, 900 };

        protected MaterialColorSwatch(int primaryArgb, string name = null) : base(primaryArgb, name) { }
        protected MaterialColorSwatch(uint primaryArgb, string name = null) : base(primaryArgb, name) { }

        public override ReadOnlySpan<int> Weights => _weights;

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
    }

    /// <summary>
    ///     Base class for accent swatches with weights:
    ///     100, 200, 400, 700.
    /// </summary>
    public abstract class MaterialAccentColorSwatch : MaterialSwatch {
        private static readonly int[] _weights = { 100, 200, 400, 700 };

        protected MaterialAccentColorSwatch(int primaryArgb, string name = null) : base(primaryArgb, name) { }
        protected MaterialAccentColorSwatch(uint primaryArgb, string name = null) : base(primaryArgb, name) { }

        public override ReadOnlySpan<int> Weights => _weights;

        public Color Shade100 => this[100];
        public Color Shade200 => this[200];
        public Color Shade400 => this[400];
        public Color Shade700 => this[700];
    }

    /// <summary>
    ///     Generated swatch backed by a tonal palette.
    ///     This replaces the old MaterialColor wrapper behavior.
    /// </summary>
    public sealed class TonalMaterialColor : MaterialColorSwatch {
        public TonalMaterialColor(int argb, string name = null)
            : this(Hct.FromInt(argb), name) { }

        public TonalMaterialColor(uint argb, string name = null)
            : this(unchecked((int)argb), name) { }

        public TonalMaterialColor(Hct hct, string name = null)
            : this(TonalPalette.FromHct(hct), hct, name) { }

        public TonalMaterialColor(TonalPalette tonalPalette, string name = null)
            : this(tonalPalette, tonalPalette?.KeyColor, name) { }

        private TonalMaterialColor(TonalPalette tonalPalette, Hct seed, string name = null)
            : base(tonalPalette?.Get(500) ?? 0, name) {
            Palette = tonalPalette ?? throw new ArgumentNullException(nameof(tonalPalette));
            Seed = seed ?? tonalPalette.KeyColor;
        }

        public TonalPalette Palette { get; }
        public Hct Seed { get; }

        public override Color this[int weight] => Palette.GetHct(WeightToTone(weight)).ToColor();

        /// <summary>
        ///     Returns a swatch for arbitrary HCT tone values.
        /// </summary>
        public Color[] ToToneSwatch(params double[] tones) {
            if (tones == null || tones.Length == 0) return ToSwatch();

            var colors = new Color[tones.Length];
            for (var i = 0; i < tones.Length; i++) colors[i] = Palette.GetHct(tones[i]).ToColor();
            return colors;
        }

        public static implicit operator Hct(TonalMaterialColor materialColor) {
            return materialColor.Seed;
        }

        public static implicit operator TonalPalette(TonalMaterialColor materialColor) {
            return materialColor.Palette;
        }

        private static double WeightToTone(int weight) {
            return weight switch {
                50  => 95.0,
                100 => 90.0,
                200 => 80.0,
                300 => 70.0,
                400 => 60.0,
                500 => 50.0,
                600 => 40.0,
                700 => 30.0,
                800 => 20.0,
                900 => 10.0,
                _   => (1000 - weight) / 10.0
            };
        }
    }

    /// <summary>
    ///     Generated accent swatch backed by a tonal palette.
    ///     Useful if you want accent-style indexing but still generated from HCT.
    /// </summary>
    public sealed class TonalMaterialAccentColor : MaterialAccentColorSwatch {
        public TonalMaterialAccentColor(int argb, string name = null)
            : this(Hct.FromInt(argb), name) { }

        public TonalMaterialAccentColor(uint argb, string name = null)
            : this(unchecked((int)argb), name) { }

        public TonalMaterialAccentColor(Hct hct, string name = null)
            : this(TonalPalette.FromHct(hct), hct, name) { }

        public TonalMaterialAccentColor(TonalPalette tonalPalette, string name = null)
            : this(tonalPalette, tonalPalette?.KeyColor, name) { }

        private TonalMaterialAccentColor(TonalPalette tonalPalette, Hct seed, string name = null)
            : base(tonalPalette?.Get(80) ?? 0, name) {
            Palette = tonalPalette ?? throw new ArgumentNullException(nameof(tonalPalette));
            Seed = seed ?? tonalPalette.KeyColor;
        }

        public TonalPalette Palette { get; }
        public Hct Seed { get; }

        public override Color this[int weight] => Palette.GetHct(AccentWeightToTone(weight)).ToColor();

        public static implicit operator Hct(TonalMaterialAccentColor materialColor) {
            return materialColor.Seed;
        }

        public static implicit operator TonalPalette(TonalMaterialAccentColor materialColor) {
            return materialColor.Palette;
        }

        private static double AccentWeightToTone(int weight) {
            return weight switch {
                100 => 90.0,
                200 => 80.0,
                400 => 40.0,
                700 => 20.0,
                _   => throw new ArgumentOutOfRangeException(nameof(weight), $"Invalid accent weight {weight}. Expected 100, 200, 400, or 700.")
            };
        }
    }

    /// <summary>
    ///     Exact pre-created standard swatch.
    ///     Use this when you need to match Flutter's hard-coded tables exactly.
    /// </summary>
    public sealed class FixedMaterialColor : MaterialColorSwatch {
        private readonly int[] _argbs;

        public FixedMaterialColor(
            int primaryArgb,
            int shade50,
            int shade100,
            int shade200,
            int shade300,
            int shade400,
            int shade500,
            int shade600,
            int shade700,
            int shade800,
            int shade900,
            string name = null
        ) : base(primaryArgb, name) {
            _argbs = new[] {
                shade50, shade100, shade200, shade300, shade400,
                shade500, shade600, shade700, shade800, shade900
            };
        }

        public FixedMaterialColor(
            uint primaryArgb,
            uint shade50,
            uint shade100,
            uint shade200,
            uint shade300,
            uint shade400,
            uint shade500,
            uint shade600,
            uint shade700,
            uint shade800,
            uint shade900,
            string name = null
        ) : this(
            unchecked((int)primaryArgb),
            unchecked((int)shade50),
            unchecked((int)shade100),
            unchecked((int)shade200),
            unchecked((int)shade300),
            unchecked((int)shade400),
            unchecked((int)shade500),
            unchecked((int)shade600),
            unchecked((int)shade700),
            unchecked((int)shade800),
            unchecked((int)shade900),
            name
        ) { }

        public override Color this[int weight] => GetArgb(weight).ArgbToColor();

        public override int GetArgb(int weight) {
            return weight switch {
                50  => _argbs[0],
                100 => _argbs[1],
                200 => _argbs[2],
                300 => _argbs[3],
                400 => _argbs[4],
                500 => _argbs[5],
                600 => _argbs[6],
                700 => _argbs[7],
                800 => _argbs[8],
                900 => _argbs[9],
                _   => throw new ArgumentOutOfRangeException(nameof(weight), $"Invalid swatch weight {weight}.")
            };
        }
    }

    /// <summary>
    ///     Exact pre-created accent swatch.
    ///     Use this when you need to match Flutter accent tables exactly.
    /// </summary>
    public sealed class FixedMaterialAccentColor : MaterialAccentColorSwatch {
        private readonly int[] _argbs;

        public FixedMaterialAccentColor(
            int primaryArgb,
            int shade100,
            int shade200,
            int shade400,
            int shade700,
            string name = null
        ) : base(primaryArgb, name) {
            _argbs = new[] { shade100, shade200, shade400, shade700 };
        }

        public FixedMaterialAccentColor(
            uint primaryArgb,
            uint shade100,
            uint shade200,
            uint shade400,
            uint shade700,
            string name = null
        ) : this(
            unchecked((int)primaryArgb),
            unchecked((int)shade100),
            unchecked((int)shade200),
            unchecked((int)shade400),
            unchecked((int)shade700),
            name
        ) { }

        public override Color this[int weight] => GetArgb(weight).ArgbToColor();

        public override int GetArgb(int weight) {
            return weight switch {
                100 => _argbs[0],
                200 => _argbs[1],
                400 => _argbs[2],
                700 => _argbs[3],
                _   => throw new ArgumentOutOfRangeException(nameof(weight), $"Invalid accent swatch weight {weight}.")
            };
        }
    }
    
}
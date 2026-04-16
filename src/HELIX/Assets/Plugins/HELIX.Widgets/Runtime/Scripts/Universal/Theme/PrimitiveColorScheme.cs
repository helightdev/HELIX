using HELIX.Coloring;
using HELIX.Coloring.Material;
using UnityEngine;

namespace HELIX.Widgets.Universal.Theme {
    public class PrimitiveColorScheme {
        public static readonly PrimitiveColorScheme Default = From(MaterialColors.Indigo);
        
        public ColorTokenPalette primary;
        public ColorTokenPalette secondary;
        public ColorTokenPalette tertiary;
        public ColorTokenPalette error;
        public SurfaceColorPalette surface;
        public LayerOpacityProgression layerOpacityProgression;
        public Color scrim;
        public Color shadow;
        public Color surfaceTint;
        public Color outline;
        public Color outlineVariant;
        public Brightness brightness;

        public static PrimitiveColorScheme From(DynamicScheme scheme) =>
            new() {
                primary = ColorTokenPalette.Primary(scheme),
                secondary = ColorTokenPalette.Secondary(scheme),
                tertiary = ColorTokenPalette.Tertiary(scheme),
                error = ColorTokenPalette.Error(scheme),
                surface = SurfaceColorPalette.From(scheme),
                layerOpacityProgression = LayerOpacityProgression.Default,
                scrim = scheme.GetColor(MaterialDynamicColors.Scrim),
                shadow = scheme.GetColor(MaterialDynamicColors.Shadow),
                surfaceTint = scheme.GetColor(MaterialDynamicColors.SurfaceTint),
                outline = scheme.GetColor(MaterialDynamicColors.Outline),
                outlineVariant = scheme.GetColor(MaterialDynamicColors.OutlineVariant),
                brightness = scheme.IsDark ? Brightness.Dark : Brightness.Light
            };

        public static PrimitiveColorScheme From(
            Color seedColor,
            Brightness brightness = Brightness.Light,
            Variant variant = Variant.TonalSpot,
            double contrastLevel = 0
        ) {
            var scheme = seedColor.CreateScheme(variant, brightness == Brightness.Dark, contrastLevel);
            return From(scheme);
        }
    }

    public class ColorTokenPalette {
        public Color main;
        public Color onMain;
        public Color container;
        public Color onContainer;

        public Color solid;
        public Color solidDim;
        public Color onSolid;
        public Color onSolidVariant;

        protected TonalPalette tonalPalette;

        public virtual TonalPalette TonalPalette => tonalPalette ??= TonalPalette.FromHct(main.ToHct());

        public static ColorTokenPalette Primary(DynamicScheme scheme) =>
            new() {
                main = scheme.GetColor(MaterialDynamicColors.Primary),
                onMain = scheme.GetColor(MaterialDynamicColors.OnPrimary),
                container = scheme.GetColor(MaterialDynamicColors.PrimaryContainer),
                onContainer = scheme.GetColor(MaterialDynamicColors.OnPrimaryContainer),
                solid = scheme.GetColor(MaterialDynamicColors.PrimaryFixed),
                solidDim = scheme.GetColor(MaterialDynamicColors.PrimaryFixedDim),
                onSolid = scheme.GetColor(MaterialDynamicColors.OnPrimaryFixed),
                onSolidVariant = scheme.GetColor(MaterialDynamicColors.OnPrimaryFixedVariant),
            };

        public static ColorTokenPalette Secondary(DynamicScheme scheme) =>
            new() {
                main = scheme.GetColor(MaterialDynamicColors.Secondary),
                onMain = scheme.GetColor(MaterialDynamicColors.OnSecondary),
                container = scheme.GetColor(MaterialDynamicColors.SecondaryContainer),
                onContainer = scheme.GetColor(MaterialDynamicColors.OnSecondaryContainer),
                solid = scheme.GetColor(MaterialDynamicColors.SecondaryFixed),
                solidDim = scheme.GetColor(MaterialDynamicColors.SecondaryFixedDim),
                onSolid = scheme.GetColor(MaterialDynamicColors.OnSecondaryFixed),
                onSolidVariant = scheme.GetColor(MaterialDynamicColors.OnSecondaryFixedVariant),
            };

        public static ColorTokenPalette Tertiary(DynamicScheme scheme) =>
            new() {
                main = scheme.GetColor(MaterialDynamicColors.Tertiary),
                onMain = scheme.GetColor(MaterialDynamicColors.OnTertiary),
                container = scheme.GetColor(MaterialDynamicColors.TertiaryContainer),
                onContainer = scheme.GetColor(MaterialDynamicColors.OnTertiaryContainer),
                solid = scheme.GetColor(MaterialDynamicColors.TertiaryFixed),
                solidDim = scheme.GetColor(MaterialDynamicColors.TertiaryFixedDim),
                onSolid = scheme.GetColor(MaterialDynamicColors.OnTertiaryFixed),
                onSolidVariant = scheme.GetColor(MaterialDynamicColors.OnTertiaryFixedVariant),
            };

        public static ColorTokenPalette Error(DynamicScheme scheme) =>
            new() {
                main = scheme.GetColor(MaterialDynamicColors.Error),
                onMain = scheme.GetColor(MaterialDynamicColors.OnError),
                container = scheme.GetColor(MaterialDynamicColors.ErrorContainer),
                onContainer = scheme.GetColor(MaterialDynamicColors.OnErrorContainer),
                solid = scheme.GetColor(MaterialDynamicColors.ErrorContainer),
                solidDim = scheme.GetColor(MaterialDynamicColors.ErrorContainer),
                onSolid = scheme.GetColor(MaterialDynamicColors.OnErrorContainer),
                onSolidVariant = scheme.GetColor(MaterialDynamicColors.OnErrorContainer),
            };
    }

    public class SurfaceColorPalette {
        public Color main;
        public Color variant;
        public Color onMain;
        public Color onVariant;
        public Color containerLow;
        public Color container;
        public Color containerHigh;
        public Color containerHighest;
        public Color inverse;
        public Color onInverse;
        public Color onInverseAccent;

        public static SurfaceColorPalette From(DynamicScheme scheme) =>
            new() {
                main = scheme.GetColor(MaterialDynamicColors.Surface),
                variant = scheme.GetColor(MaterialDynamicColors.SurfaceVariant),
                onMain = scheme.GetColor(MaterialDynamicColors.OnSurface),
                onVariant = scheme.GetColor(MaterialDynamicColors.OnSurfaceVariant),
                containerLow = scheme.GetColor(MaterialDynamicColors.SurfaceContainerLow),
                container = scheme.GetColor(MaterialDynamicColors.SurfaceContainer),
                containerHigh = scheme.GetColor(MaterialDynamicColors.SurfaceContainerHigh),
                containerHighest = scheme.GetColor(MaterialDynamicColors.SurfaceContainerHighest),
                inverse = scheme.GetColor(MaterialDynamicColors.InverseSurface),
                onInverse = scheme.GetColor(MaterialDynamicColors.InverseOnSurface),
                onInverseAccent = scheme.GetColor(MaterialDynamicColors.InversePrimary)
            };
    }

    public class LayerOpacityProgression {
        public static readonly LayerOpacityProgression Default = new() {
            disabledLow = 0.1f,
            disabledHigh = 0.38f,
            low = 0.08f,
            normal = 0.12f,
            high = 0.38f
        };
        
        public float disabledLow;
        public float disabledHigh;
        
        public float low;
        public float normal;
        public float high;
    }
}
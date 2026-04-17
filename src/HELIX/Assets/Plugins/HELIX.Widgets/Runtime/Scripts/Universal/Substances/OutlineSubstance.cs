using System;
using HELIX.Coloring;
using HELIX.Types;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;

namespace HELIX.Widgets.Universal.Substances {
    public static class OutlineSubstance {
        public static BoxSubstance Outline(
            ColorTokenPalette palette,
            SurfaceColorPalette surface,
            BorderRadius borderRadius,
            LayerOpacityProgression progression,
            bool contrast = false
        ) {
            return new BoxSubstance {
                borderRadius = new AllWidgetStateProperty<BorderRadius>(borderRadius),
                border = new WidgetStatePropertyMap<Border> {
                    [WidgetState.Disabled] = Border.All(1, surface.onMain.WithOpacity(progression.disabledHigh)),
                    [WidgetState.None] = contrast ? Border.All(1, surface.onMain) : Border.All(1, palette.main)
                },
                backgroundStyle = new WidgetStatePropertyMap<BackgroundStyle> {
                    [WidgetState.Disabled] = Colors.Transparent,
                    [WidgetState.ModAny | WidgetState.Selected | WidgetState.Pressed] =
                        contrast
                            ? surface.onMain.WithOpacity(progression.normal)
                            : palette.main.WithOpacity(progression.normal),
                    [WidgetState.Hovered] =
                        contrast
                            ? surface.onMain.WithOpacity(progression.low)
                            : palette.main.WithOpacity(progression.low),
                    [WidgetState.None] = Colors.Transparent
                },
                transitions = new AllWidgetStateProperty<Transition[]>(
                    new Transition[] { new(StyleProperties.BackgroundColor) { duration = 0.1f } }
                )
            };
        }

        public static BuilderAndSubstance<TBuilder, BoxSubstance> Outline<TBuilder>(
            this ISubstanceBuilder<TBuilder> builder,
            ColorTokenPalette palette = null,
            SurfaceColorPalette surface = null,
            BorderRadius? borderRadius = null,
            HInputRadius inputRadius = HInputRadius.Medium,
            bool contrast = false
        ) where TBuilder : ISubstanceBuilder<TBuilder> =>
            builder.Append(context => {
                    var colors = PrimitiveBaseTheme.Colors.Get(context, listen: builder.Listening);
                    var radius = PrimitiveBaseTheme.Radius.Get(context, listen: builder.Listening);
                    palette ??= colors.primary;
                    surface ??= colors.surface;
                    var effectiveRadius = borderRadius.GetValueOrDefault(
                        inputRadius switch {
                            HInputRadius.None   => BorderRadius.None,
                            HInputRadius.Small  => BorderRadius.All(radius.Radius1),
                            HInputRadius.Medium => BorderRadius.All(radius.Radius2),
                            HInputRadius.Large  => BorderRadius.All(radius.Radius3),
                            HInputRadius.Full   => BorderRadius.All(9999),
                            _ => throw new ArgumentOutOfRangeException(
                                nameof(inputRadius),
                                inputRadius,
                                null
                            )
                        }
                    );

                    return Outline(palette, surface, effectiveRadius, colors.layerOpacityProgression, contrast);
                }
            );
    }
}
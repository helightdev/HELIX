using System;
using HELIX.Coloring;
using HELIX.Types;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;

namespace HELIX.Widgets.Universal.Substances {
    public static class SoftSubstance {

        public static BoxSubstance Soft(
            ColorTokenPalette palette,
            SurfaceColorPalette surface,
            BorderRadius borderRadius,
            LayerOpacityProgression progression
        ) {
            return new BoxSubstance {
                borderRadius = new AllWidgetStateProperty<BorderRadius>(borderRadius),
                background = new WidgetStatePropertyMap<BackgroundStyle> {
                    // This is not accurate as it uses lerping instead of actual alpha compositing, but without
                    // this, the toggling tends to cause very visible flickering otherwise.
                    [WidgetState.Disabled] = Color.Lerp(surface.main, surface.onMain, progression.disabledLow),

                    [WidgetState.ModAny | WidgetState.Selected | WidgetState.Pressed] =
                        Color.Lerp(palette.container, palette.onContainer, progression.normal),
                    [WidgetState.Hovered] = Color.Lerp(
                        palette.container,
                        palette.onContainer,
                        progression.low
                    ),
                    [WidgetState.None] = palette.container
                },
                transitions = new AllWidgetStateProperty<Transition[]>(
                    new Transition[] { new(StyleProperties.BackgroundColor) { duration = 0.1f } }
                )
            };
        }

        public static BuilderAndSubstance<TBuilder, BoxSubstance> Soft<TBuilder>(
            this ISubstanceBuilder<TBuilder> builder,
            ColorTokenPalette palette = null,
            SurfaceColorPalette surface = null,
            BorderRadius? borderRadius = null,
            HInputRadius inputRadius = HInputRadius.Medium
        ) where TBuilder : ISubstanceBuilder<TBuilder> {
            return builder.Append(context => {
                    var colors = PrimitiveBaseTheme.Colors.Get(context, builder.Listening);
                    var radius = PrimitiveBaseTheme.Radius.Get(context, builder.Listening);
                    palette ??= colors.primary;
                    surface ??= colors.surface;
                    var effectiveRadius = borderRadius.GetValueOrDefault(
                        inputRadius switch {
                            HInputRadius.None => BorderRadius.None,
                            HInputRadius.Small => BorderRadius.All(radius.Radius1),
                            HInputRadius.Medium => BorderRadius.All(radius.Radius2),
                            HInputRadius.Large => BorderRadius.All(radius.Radius3),
                            HInputRadius.Full => BorderRadius.All(9999),
                            _ => throw new ArgumentOutOfRangeException(
                                nameof(inputRadius),
                                inputRadius,
                                null
                            )
                        }
                    );

                    return Soft(palette, surface, effectiveRadius, colors.layerOpacityProgression);
                }
            );
        }

    }
}
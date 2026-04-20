using System;
using HELIX.Coloring;
using HELIX.Types;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine;

namespace HELIX.Widgets.Universal.Substances {
  public static class FlatSubstance {
    public static BoxSubstance Flat(
      ColorTokenPalette inactive,
      ColorTokenPalette palette,
      SurfaceColorPalette surface,
      BorderRadius borderRadius,
      LayerOpacityProgression progression
    ) {
      return new BoxSubstance {
        borderRadius = new AllWidgetStateProperty<BorderRadius>(borderRadius),
        background = new WidgetStatePropertyMap<BackgroundStyle> {
          [WidgetState.Disabled] = surface.onMain.WithOpacity(progression.disabledLow),
          [WidgetState.ModAny | WidgetState.Pressed | WidgetState.Selected] =
            Color.Lerp(palette.main, surface.onMain, progression.normal),
          [WidgetState.Hovered] = Color.Lerp(inactive.main, surface.onMain, progression.low),
          [WidgetState.None] = inactive.main
        },
        transitions = new AllWidgetStateProperty<Transition[]>(
          new Transition[] { new(StyleProperties.BackgroundColor) { duration = 0.1f } }
        )
      };
    }

    public static BuilderAndSubstance<TBuilder, BoxSubstance> Flat<TBuilder>(
      this ISubstanceBuilder<TBuilder> builder,
      ColorTokenPalette inactive = null,
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
          inactive ??= colors.primary;
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
          return Flat(inactive, palette, surface, effectiveRadius, colors.layerOpacityProgression);
        }
      );
    }
  }
}
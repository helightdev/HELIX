using System;
using HELIX.Coloring;
using HELIX.Types;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Universal.Substances;
using HELIX.Widgets.Universal.Theme;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal.Styles {
  public class DefaultButtonStyles {
    public static HButtonStyle DefaultStyleOf(
      IThemeProvider context,
      HButtonVariant variant,
      HButtonSize size,
      HInputRadius rad,
      bool contrast = false,
      ColorTokenPalette palette = null,
      ColorTokenPalette inactive = null,
      SurfaceColorPalette surfacePalette = null,
      Substance focusLayer = null
    ) {
      var typography = PrimitiveBaseTheme.Typography.Get(context);
      var radius = PrimitiveBaseTheme.Radius.Get(context);
      var spacing = PrimitiveBaseTheme.Spacing.Get(context);
      var colors = PrimitiveBaseTheme.Colors.Get(context);
      focusLayer ??= PrimitiveTheme.ButtonFocusLayer.Get(context);
      palette ??= colors.primary;
      surfacePalette ??= colors.surface;
      inactive ??= colors.secondary;
      var borderRadius = rad switch {
        HInputRadius.None => BorderRadius.None,
        HInputRadius.Small => BorderRadius.All(radius.Radius1),
        HInputRadius.Medium => BorderRadius.All(radius.Radius2),
        HInputRadius.Large => BorderRadius.All(radius.Radius3),
        HInputRadius.Full => BorderRadius.All(9999),
        _ => throw new ArgumentOutOfRangeException(nameof(rad), rad, null)
      };

      SubstanceBuilder layers = variant switch {
        HButtonVariant.Default or HButtonVariant.Flat or HButtonVariant.FlatTwoState =>
          new SubstanceBuilder(context as BuildContext)
            .Flat(
              variant == HButtonVariant.FlatTwoState ? inactive : palette,
              palette,
              surfacePalette,
              borderRadius
            ),
        HButtonVariant.Soft => new SubstanceBuilder(context as BuildContext)
          .Soft(palette, surfacePalette, borderRadius),
        HButtonVariant.SoftTwoState => new SubstanceBuilder(context as BuildContext).Append(provider =>
          new ConditionalSubstance(
            new WidgetStatePropertyMap<SubstanceLayers> {
              [WidgetState.Disabled] = new SubstanceBuilder(provider)
                .Soft(palette, surfacePalette, borderRadius)
                .Build(),
              [WidgetState.Selected] = new SubstanceBuilder(provider)
                .Flat(inactive, palette, surfacePalette, borderRadius)
                .Build(),
              [WidgetState.None] = new SubstanceBuilder(provider)
                .Soft(palette, surfacePalette, borderRadius)
                .Build()
            }
          )
        ),
        HButtonVariant.Outline => new SubstanceBuilder(context as BuildContext)
          .Outline(palette, surfacePalette, borderRadius),
        HButtonVariant.Ghost => new SubstanceBuilder(context as BuildContext)
          .Ghost(palette, surfacePalette, borderRadius),
        HButtonVariant.TwoState => new SubstanceBuilder(context as BuildContext).Append(provider =>
          new ConditionalSubstance(
            new WidgetStatePropertyMap<SubstanceLayers> {
              [WidgetState.Selected] = new SubstanceBuilder(provider)
                .Flat(inactive, palette, surfacePalette, borderRadius)
                .Build(),
              [WidgetState.None] = new SubstanceBuilder(provider)
                .Outline(palette, surfacePalette, borderRadius)
                .Build()
            }
          )
        ),
        _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null)
      };

      layers.Append(_ => focusLayer);

      BoxConstraints constraints;
      StyleLength4 padding;
      float fontSize;
      switch (size) {
        case HButtonSize.Small:
          constraints = BoxConstraints.Tight(StyleKeyword.Auto, typography.LineHeight2);
          fontSize = typography.FontSize2;
          padding = variant == HButtonVariant.Ghost
            ? EdgeInsets.Symmetric(spacing.Space2, spacing.Space1)
            : EdgeInsets.Symmetric(spacing.Space2, 0);
          break;
        case HButtonSize.Regular:
          constraints = BoxConstraints.Tight(StyleKeyword.Auto, typography.LineHeight3);
          fontSize = typography.FontSize3;
          padding = variant == HButtonVariant.Ghost
            ? EdgeInsets.Symmetric(spacing.Space2, spacing.Space1)
            : EdgeInsets.Symmetric(spacing.Space3, 0);
          break;
        case HButtonSize.Medium:
          constraints = BoxConstraints.Tight(StyleKeyword.Auto, typography.LineHeight4);
          fontSize = typography.FontSize4;
          padding = variant == HButtonVariant.Ghost
            ? EdgeInsets.Symmetric(spacing.Space3, spacing.Space1 * 1.5f)
            : EdgeInsets.Symmetric(spacing.Space4, 0);
          break;
        case HButtonSize.Large:
          constraints = BoxConstraints.Tight(StyleKeyword.Auto, typography.LineHeight5);
          fontSize = typography.FontSize5;
          padding = variant == HButtonVariant.Ghost
            ? EdgeInsets.Symmetric(spacing.Space4, spacing.Space2)
            : EdgeInsets.Symmetric(spacing.Space5, 0);
          break;
        default: throw new ArgumentOutOfRangeException(nameof(size), size, null);
      }

      var fontColor = contrast
        ? surfacePalette.onMain
        : variant switch {
          HButtonVariant.Soft or HButtonVariant.SoftTwoState => palette.onContainer,
          HButtonVariant.Ghost or HButtonVariant.Outline or HButtonVariant.TwoState =>
            palette.main,
          _ => palette.onMain
        };

      var fontColorInactive = variant switch {
        HButtonVariant.FlatTwoState => inactive.onMain,
        _ => fontColor
      };
      var defaultText = new WidgetStatePropertyMap<TextStyle> {
        [WidgetState.Disabled] = new TextStyle {
          fontSize = fontSize,
          color = surfacePalette.onMain.WithOpacity(0.38f)
        },
        [WidgetState.ModAny | WidgetState.Selected | WidgetState.Hovered | WidgetState.Pressed] = new TextStyle {
          fontSize = fontSize,
          color = fontColor
        },
        [WidgetState.None] = new TextStyle {
          fontSize = fontSize,
          color = fontColorInactive
        }
      };

      var selectedText = defaultText;
      if (variant is HButtonVariant.TwoState or HButtonVariant.SoftTwoState) {
        selectedText = new WidgetStatePropertyMap<TextStyle> {
          [WidgetState.Disabled] = new TextStyle {
            fontSize = fontSize,
            color = surfacePalette.onMain.WithOpacity(0.38f)
          },
          [WidgetState.None] = new TextStyle {
            fontSize = fontSize,
            color = palette.onMain
          }
        };
      }

      return new HButtonStyle {
        padding = new AllWidgetStateProperty<StyleLength4>(padding),
        constraints = new AllWidgetStateProperty<BoxConstraints>(constraints),
        layers = layers.Build(),
        textStyle = WidgetStateProperties.Func(state =>
          state.Selected() ? selectedText.ResolveOrDefault(state) : defaultText.ResolveOrDefault(state)
        )
      };
    }
  }
}
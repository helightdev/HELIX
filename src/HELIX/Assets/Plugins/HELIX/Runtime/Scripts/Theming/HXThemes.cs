using HELIX.Coloring;
using HELIX.Types;
using UnityEngine;

namespace HELIX.Theming {
  public static class HXThemes {
    public static readonly ThemeData DefaultDark = ThemeData.Build(theme => {
        theme.brightness = Brightness.Dark;
        theme.ApplySurface(
          background: Colors.OkLch(0.145f, 0f, 0f),
          onBackground: Colors.OkLch(0.97f, 0f, 0f),
          onBackgroundVariant: Colors.OkLch(0.708f, 0f, 0f),
          containerLow: Colors.OkLch(0.205f, 0f, 0f),
          container: Colors.OkLch(0.269f, 0f, 0f),
          containerHigh: Colors.OkLch(0.371f, 0f, 0f),
          containerHighest: Colors.OkLch(0.439f, 0f, 0f),
          onContainer: Colors.OkLch(0.97f, 0f, 0f)
        );
        theme.primary = new ColorTokenPalette(
          Colors.OkLch(0.585f, 0.233f, 277.117f),
          Colors.OkLch(0.257f, 0.09f, 281.288f),
          Colors.OkLch(0.97f, 0f, 0f)
        );
        theme.secondary = new ColorTokenPalette(
          Colors.OkLch(0.556f, 0f, 0f),
          Colors.OkLch(0.269f, 0f, 0f),
          Colors.OkLch(0.985f, 0f, 0f)
        );
        theme.tertiary = new ColorTokenPalette(
          Colors.OkLch(0.554f, 0.046f, 257.417f),
          Colors.OkLch(0.279f, 0.041f, 260.031f),
          Colors.OkLch(0.97f, 0f, 0f)
        );
        theme.error = new ColorTokenPalette(
          Colors.OkLch(0.637f, 0.237f, 25.331f),
          Colors.OkLch(0.258f, 0.092f, 26.042f),
          Colors.OkLch(0.97f, 0f, 0f)
        );
        theme.ApplyDefaultProgressions();
        theme.ApplyDefaultTypography();
        theme.ApplyDefaultSupports();
      }
    );

    public static void ApplySurface(
      this ThemeData themeData,
      Color background,
      Color onBackground,
      Color onBackgroundVariant,
      Color container,
      Color onContainer,
      Color containerLow,
      Color containerHigh,
      Color containerHighest
    ) {
      themeData.surface = new ColorPair(background, onBackground);
      themeData.surfaceContainerLow = new ColorPair(containerLow, onContainer);
      themeData.surfaceContainer = new ColorPair(container, onContainer);
      themeData.surfaceContainerHigh = new ColorPair(containerHigh, onContainer);
      themeData.surfaceContainerHighest = new ColorPair(containerHighest, onContainer);
      themeData.surfaceVariant = new ColorPair(background, onBackgroundVariant);
      themeData.surfaceInverse = new ColorPair(onBackground, background);
    }

    public static void ApplyDefaultSupports(this ThemeData themeData) {
      themeData.outlineColor = themeData.surface.onValue.WithOpacity(0.10f);
      themeData.scrimColor = Colors.Black20;
      themeData.shadowColor = Colors.Black80;
      themeData.surfaceTintColor = Colors.White;
      themeData.focusColor = themeData.primary.main.value;

      themeData.SetColorProvider(ThemeProperties.RoleDisabledLowProvider, ColorRoles.DisabledLow);
      themeData.SetColorProvider(ThemeProperties.RoleDisabledHighProvider, ColorRoles.DisabledHigh);
    }

    public static void ApplyDefaultProgressions(this ThemeData themeData) {
      themeData.blend = BlendProgression.Default;
      //themeData.blend.ApproximatelyConvertToLinear(themeData.brightness);
      themeData.border = BorderProgression.Default;
      themeData.radius = RadiusProgression.Generate(4.5f, 1f);
      themeData.spacing = SpacingProgression.Generate(5f, 1f);
    }

    public static void ApplyDefaultTypography(this ThemeData themeData) {
      var baseStyle = new TextStyle(color: themeData.surface.onValue);

      themeData.display = TypographyGroup.Display(baseStyle);
      themeData.headline = TypographyGroup.Headline(baseStyle);
      themeData.title = TypographyGroup.Title(baseStyle);
      themeData.label = TypographyGroup.Label(baseStyle);
      themeData.body = TypographyGroup.Body(baseStyle);
    }
  }
}
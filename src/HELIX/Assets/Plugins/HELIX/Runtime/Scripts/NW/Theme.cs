using System;
using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Types;
using UnityEngine;

namespace HELIX.NW {
  public static class BuiltinThemes {
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
        theme.ApplyDefaultSupports();
        theme.ApplyDefaultProgressions();
        theme.ApplyDefaultTypography();
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
    }

    public static void ApplyDefaultProgressions(this ThemeData themeData) {
      themeData.blend = BlendProgression.Default;
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

    // public static BaseTheme NeutralLight = new BaseTheme() {
    //   surface = new ColorPair(Colors.OkLch(1f, 0f, 0f), Colors.OkLch(0.145f, 0f, 0f)),
    //   surfaceContainerLow = new ColorPair(Colors.OkLch(985f, 0f, 0f), Colors.OkLch(0.145f, 0f, 0f)),
    //   surfaceContainer = new ColorPair(Colors.OkLch(0.97f, 0f, 0f), Colors.OkLch(0.556f, 0f, 0f)),
    //   surfaceContainerHigh = new ColorPair(Colors.OkLch(922f, 0f, 0f), Colors.OkLch(0.145f, 0f, 0f)),
    //   surfaceContainerHighest = new ColorPair(Colors.OkLch(0.922f, 0f, 0f), Colors.OkLch(0.145f, 0f, 0f)),
    //   surfaceVariant = new ColorPair(Colors.OkLch(0.97f, 0f, 0f), Colors.OkLch(0.556f, 0f, 0f)),
    //   surfaceInverse = new ColorPair(Colors.OkLch(0.145f, 0f, 0f), Colors.OkLch(1f, 0f, 0f)),
    // };
    //
    // public static BaseTheme NeutralDark = new BaseTheme() {
    //   surface = new ColorPair(Colors.OkLch(0.145f, 0f, 0f), Colors.OkLch(0.985f, 0f, 0f)),
    //   surfaceContainerLow = new ColorPair(Colors.OkLch(0.205f, 0f, 0f), Colors.OkLch(0.985f, 0f, 0f)),
    //   surfaceContainer = new ColorPair(Colors.OkLch(0.269f, 0f, 0f), Colors.OkLch(0.708f, 0f, 0f)),
    //   surfaceContainerHigh = new ColorPair(
    //     Colors.AlphaBlend(Colors.OkLch(0.269f, 0f, 0f), Colors.White10),
    //     Colors.OkLch(0.985f, 0f, 0f)
    //   ),
    //   surfaceContainerHighest = new ColorPair(
    //     Colors.AlphaBlend(Colors.OkLch(0.269f, 0f, 0f), Colors.White15),
    //     Colors.OkLch(0.985f, 0f, 0f)
    //   ),
    //   surfaceVariant = new ColorPair(Colors.OkLch(0.269f, 0f, 0f), Colors.OkLch(0.708f, 0f, 0f)),
    //   surfaceInverse = new ColorPair(Colors.OkLch(0.708f, 0f, 0f), Colors.OkLch(0.145f, 0f, 0f)),
    // };
  }

  public record ThemeData {
    public static readonly ContextKey<ThemeData> Context = new("Theme", BuiltinThemes.DefaultDark);

    public ColorTokenPalette primary;
    public ColorTokenPalette secondary;
    public ColorTokenPalette tertiary;
    public ColorTokenPalette error;

    public ColorPair surface;
    public ColorPair surfaceContainerLow;
    public ColorPair surfaceContainer;
    public ColorPair surfaceContainerHigh;
    public ColorPair surfaceContainerHighest;
    public ColorPair surfaceInverse;
    public ColorPair surfaceVariant;

    public Brightness brightness;
    public Color scrimColor;
    public Color shadowColor;
    public Color surfaceTintColor;
    public Color outlineColor;
    public Color focusColor;

    public BlendProgression blend;
    public RadiusProgression radius;
    public SpacingProgression spacing;
    public BorderProgression border;

    public TypographyGroup display;
    public TypographyGroup headline;
    public TypographyGroup title;
    public TypographyGroup label;
    public TypographyGroup body;

    public Dictionary<ColorRole, Color> customColors = new();
    public Dictionary<ThemeProperty, object> properties = new();

    public static ThemeData Build(Action<ThemeData> builder) {
      var theme = new ThemeData();
      builder(theme);
      return theme;
    }

    public Color GetColor(ColorRole role) {
      var lookup = role & ~ColorRole.GroupBlend;
      var resolvedColor = lookup switch {
        ColorRole.None => Colors.Transparent,
        ColorRole.Transparent => Colors.Transparent,

        ColorRoles.Primary => primary.main.value,
        ColorRoles.OnPrimary => primary.main.onValue,
        ColorRoles.PrimaryContainer => primary.container.value,
        ColorRoles.OnPrimaryContainer => primary.container.onValue,

        ColorRoles.Secondary => secondary.main.value,
        ColorRoles.OnSecondary => secondary.main.onValue,
        ColorRoles.SecondaryContainer => secondary.container.value,
        ColorRoles.OnSecondaryContainer => secondary.container.onValue,

        ColorRoles.Tertiary => tertiary.main.value,
        ColorRoles.OnTertiary => tertiary.main.onValue,
        ColorRoles.TertiaryContainer => tertiary.container.value,
        ColorRoles.OnTertiaryContainer => tertiary.container.onValue,

        ColorRoles.Error => error.main.value,
        ColorRoles.OnError => error.main.onValue,
        ColorRoles.ErrorContainer => error.container.value,
        ColorRoles.OnErrorContainer => error.container.onValue,

        ColorRoles.Surface => surface.value,
        ColorRoles.OnSurface => surface.onValue,
        ColorRoles.SurfaceVariant => surfaceVariant.value,
        ColorRoles.OnSurfaceVariant => surfaceVariant.onValue,
        ColorRoles.SurfaceInverse => surfaceInverse.value,
        ColorRoles.OnSurfaceInverse => surfaceInverse.onValue,
        ColorRoles.SurfaceContainer => surfaceContainer.value,
        ColorRoles.OnSurfaceContainer => surfaceContainer.onValue,
        ColorRoles.SurfaceContainerLow => surfaceContainerLow.value,
        ColorRoles.OnSurfaceContainerLow => surfaceContainerLow.onValue,
        ColorRoles.SurfaceContainerHigh => surfaceContainerHigh.value,
        ColorRoles.OnSurfaceContainerHigh => surfaceContainerHigh.onValue,
        ColorRoles.SurfaceContainerHighest => surfaceContainerHighest.value,
        ColorRoles.OnSurfaceContainerHighest => surfaceContainerHighest.onValue,

        ColorRoles.Scrim => scrimColor,
        ColorRoles.Shadow => shadowColor,
        ColorRoles.SurfaceTint => surfaceTintColor,
        ColorRoles.Outline => outlineColor,
        ColorRoles.Focus => focusColor,

        _ => ResolvedFallbackColor(role)
      };

      var groupBlend = role & ColorRole.GroupBlend;
      if (groupBlend > ColorRole.None) {
        var level = groupBlend switch {
          ColorRole.BlendDisabledLow => BlendLevel.DisabledLow,
          ColorRole.BlendDisabledHigh => BlendLevel.DisabledHigh,
          ColorRole.BlendLow => BlendLevel.Low,
          ColorRole.BlendNormal => BlendLevel.Normal,
          ColorRole.BlendHigh => BlendLevel.High,
          _ => throw new ArgumentOutOfRangeException(nameof(groupBlend), groupBlend, null)
        };

        resolvedColor = resolvedColor.MultiplyOpacity(GetBlendLevel(level));
      }

      return resolvedColor;
    }

    private Color ResolvedFallbackColor(ColorRole role) {
      if (customColors.TryGetValue(role, out var color)) {
        return color;
      }

      if (role.HasFlag(ColorRole.Transparent)) {
        return Colors.Transparent;
      }

      throw new ArgumentOutOfRangeException(nameof(role), role, null);
    }

    public Color this[ColorRole role] => GetColor(role);
    public float this[BlendLevel level] => GetBlendLevel(level);
    public float this[BorderRole role] => GetBorderWidth(role);
    public float this[RadiusRole role] => GetRadius(role);
    public float this[SpacingRole role] => GetSpacing(role);

    public T GetComputedProperty<T>(ThemeProperty<T> property) {
      if (properties.TryGetValue(property, out var value)) {
        return (T)value;
      }

      if (property.Compute(this, out var computed)) {
        properties[property] = computed;
        return computed;
      }

      if (property.hasDefault) {
        properties[property] = property.defaultValue;
        return property.defaultValue;
      }

      throw new KeyNotFoundException(
        $"Theme property {property} was not found, has no default value and can't be computed"
      );
    }

    public float GetBlendLevel(BlendLevel level) {
      return level switch {
        BlendLevel.DisabledLow => blend.disabledLow,
        BlendLevel.DisabledHigh => blend.disabledHigh,
        BlendLevel.Low => blend.low,
        BlendLevel.Normal => blend.normal,
        BlendLevel.High => blend.high,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
      };
    }

    public float GetBorderWidth(BorderRole role) {
      return role switch {
        BorderRole.None => 0f,
        BorderRole.Small => border.borderSmall,
        BorderRole.Normal => border.borderNormal,
        BorderRole.Large => border.borderLarge,
        BorderRole.ExtraLarge => border.borderExtraLarge,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
      };
    }

    public float GetRadius(RadiusRole role) {
      return role switch {
        RadiusRole.None => 0f,
        RadiusRole.Radius1 => radius.radius1,
        RadiusRole.Radius2 => radius.radius2,
        RadiusRole.Radius3 => radius.radius3,
        RadiusRole.Radius4 => radius.radius4,
        RadiusRole.Radius5 => radius.radius5,
        RadiusRole.Radius6 => radius.radius6,
        RadiusRole.Round => 9999f,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
      };
    }

    public float GetSpacing(SpacingRole role) {
      return role switch {
        SpacingRole.None => 0f,
        SpacingRole.Spacing1 => spacing.spacing1,
        SpacingRole.Spacing2 => spacing.spacing2,
        SpacingRole.Spacing3 => spacing.spacing3,
        SpacingRole.Spacing4 => spacing.spacing4,
        SpacingRole.Spacing5 => spacing.spacing5,
        SpacingRole.Spacing6 => spacing.spacing6,
        SpacingRole.Spacing7 => spacing.spacing7,
        SpacingRole.Spacing8 => spacing.spacing8,
        SpacingRole.Spacing9 => spacing.spacing9,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
      };
    }

    public Color BlendLerp(ColorRole from, ColorRole to, BlendLevel level) {
      var fromColor = GetColor(from);
      var toColor = GetColor(to);
      var t = GetBlendLevel(level);
      return Color.Lerp(fromColor, toColor, t);
    }

    public Color BlendLerp(Color from, Color to, BlendLevel level) {
      var t = GetBlendLevel(level);
      return Color.Lerp(from, to, t);
    }

    public Color BlendOverlay(Color baseColor, Color overlayColor, BlendLevel level) {
      var t = GetBlendLevel(level);
      return Colors.AlphaBlend(baseColor, overlayColor.WithOpacity(t));
    }

    public Color BlendOverlay(Color baseColor, ColorRole overlayRole, BlendLevel level) {
      var overlayColor = GetColor(overlayRole);
      return BlendOverlay(baseColor, overlayColor, level);
    }

    public ref TextStyle GetTextStyleRef(TextRole role) {
      return ref GetTypographyTokenRef(role).style;
    }

    public ref TypographyToken GetTypographyTokenRef(TextRole role) {
      switch (role) {
        case TextRole.DisplayLarge: return ref display.large;
        case TextRole.DisplayMedium: return ref display.medium;
        case TextRole.DisplaySmall: return ref display.small;

        case TextRole.HeadlineLarge: return ref headline.large;
        case TextRole.HeadlineMedium: return ref headline.medium;
        case TextRole.HeadlineSmall: return ref headline.small;

        case TextRole.TitleLarge: return ref title.large;
        case TextRole.TitleMedium: return ref title.medium;
        case TextRole.TitleSmall: return ref title.small;

        case TextRole.LabelLarge: return ref label.large;
        case TextRole.LabelMedium: return ref label.medium;
        case TextRole.LabelSmall: return ref label.small;

        case TextRole.BodyLarge: return ref body.large;
        case TextRole.BodyMedium: return ref body.medium;
        case TextRole.BodySmall: return ref body.small;

        default: throw new ArgumentOutOfRangeException(nameof(role), role, null);
      }
    }
  }

  [Flags]
  public enum ColorRole : int {
    Primary = 1 << 0,
    Secondary = 1 << 1,
    Tertiary = 1 << 2,
    Error = 1 << 3,
    Surface = 1 << 4,
    SurfaceLow = 1 << 5,
    SurfaceHigh = 1 << 6,
    SurfaceHighest = 1 << 7,
    SurfaceInverse = 1 << 8,
    SurfaceVariant = 1 << 9,
    Custom1 = 1 << 10,
    Custom2 = 1 << 11,
    Custom3 = 1 << 12,
    Custom4 = 1 << 13,
    Custom5 = 1 << 14,
    Custom6 = 1 << 15,
    Custom7 = 1 << 16,
    Custom8 = 1 << 17,
    Custom9 = 1 << 18,
    ModA = 1 << 19,
    ModB = 1 << 20,
    ModC = 1 << 21,
    ModD = 1 << 22,
    Colors = 1 << 23,
    Container = 1 << 24,
    On = 1 << 25,
    BlendDisabledLow = 1 << 26,
    BlendDisabledHigh = 1 << 27,
    BlendLow = 1 << 28,
    BlendNormal = 1 << 29,
    BlendHigh = 1 << 30,
    None = 0,

    Transparent = 1 << 0 | Colors,
    Scrim = 1 << 1 | Colors,
    Shadow = 1 << 2 | Colors,
    SurfaceTint = 1 << 3 | Colors,
    Outline = 1 << 4 | Colors,
    Focus = 1 << 5 | Colors,

    GroupBlend = BlendDisabledLow | BlendDisabledHigh | BlendLow | BlendNormal | BlendHigh
  }

  public static class ColorRoles {
    public const ColorRole Transparent = ColorRole.Transparent;
    public const ColorRole Primary = ColorRole.Primary;
    public const ColorRole Secondary = ColorRole.Secondary;
    public const ColorRole Tertiary = ColorRole.Tertiary;
    public const ColorRole Error = ColorRole.Error;
    public const ColorRole Surface = ColorRole.Surface;

    public const ColorRole OnPrimary = ColorRole.Primary | ColorRole.On;
    public const ColorRole OnSecondary = ColorRole.Secondary | ColorRole.On;
    public const ColorRole OnTertiary = ColorRole.Tertiary | ColorRole.On;
    public const ColorRole OnError = ColorRole.Error | ColorRole.On;
    public const ColorRole OnSurface = ColorRole.Surface | ColorRole.On;

    public const ColorRole PrimaryContainer = ColorRole.Primary | ColorRole.Container;
    public const ColorRole SecondaryContainer = ColorRole.Secondary | ColorRole.Container;
    public const ColorRole TertiaryContainer = ColorRole.Tertiary | ColorRole.Container;
    public const ColorRole ErrorContainer = ColorRole.Error | ColorRole.Container;
    public const ColorRole SurfaceContainer = ColorRole.Surface | ColorRole.Container;
    public const ColorRole SurfaceContainerLow = ColorRole.SurfaceLow | ColorRole.Container;
    public const ColorRole SurfaceContainerHigh = ColorRole.SurfaceHigh | ColorRole.Container;
    public const ColorRole SurfaceContainerHighest = ColorRole.SurfaceHighest | ColorRole.Container;

    public const ColorRole OnPrimaryContainer = ColorRole.Primary | ColorRole.Container | ColorRole.On;
    public const ColorRole OnSecondaryContainer = ColorRole.Secondary | ColorRole.Container | ColorRole.On;
    public const ColorRole OnTertiaryContainer = ColorRole.Tertiary | ColorRole.Container | ColorRole.On;
    public const ColorRole OnErrorContainer = ColorRole.Error | ColorRole.Container | ColorRole.On;
    public const ColorRole OnSurfaceContainer = ColorRole.Surface | ColorRole.Container | ColorRole.On;
    public const ColorRole OnSurfaceContainerLow = ColorRole.SurfaceLow | ColorRole.Container | ColorRole.On;
    public const ColorRole OnSurfaceContainerHigh = ColorRole.SurfaceHigh | ColorRole.Container | ColorRole.On;
    public const ColorRole OnSurfaceContainerHighest = ColorRole.SurfaceHighest | ColorRole.Container | ColorRole.On;

    public const ColorRole SurfaceInverse = ColorRole.SurfaceInverse;
    public const ColorRole OnSurfaceInverse = ColorRole.SurfaceInverse | ColorRole.On;
    public const ColorRole SurfaceVariant = ColorRole.SurfaceVariant;
    public const ColorRole OnSurfaceVariant = ColorRole.SurfaceVariant | ColorRole.On;

    public const ColorRole OnSurfaceDisabledLow = ColorRole.Surface | ColorRole.On | ColorRole.BlendDisabledLow;
    public const ColorRole OnSurfaceDisabledHigh = ColorRole.Surface | ColorRole.On | ColorRole.BlendDisabledHigh;

    public const ColorRole Scrim = ColorRole.Scrim;
    public const ColorRole Shadow = ColorRole.Shadow;
    public const ColorRole SurfaceTint = ColorRole.SurfaceTint;
    public const ColorRole Outline = ColorRole.Outline;
    public const ColorRole Focus = ColorRole.Focus;
  }

  public enum TextRole {
    DisplayLarge,
    DisplayMedium,
    DisplaySmall,
    HeadlineLarge,
    HeadlineMedium,
    HeadlineSmall,
    TitleLarge,
    TitleMedium,
    TitleSmall,
    LabelLarge,
    LabelMedium,
    LabelSmall,
    BodyLarge,
    BodyMedium,
    BodySmall
  }

  public enum RadiusRole {
    None,
    Radius1,
    Radius2,
    Radius3,
    Radius4,
    Radius5,
    Radius6,
    Round
  }

  public enum SpacingRole {
    None = 0,
    Spacing1 = 1,
    Spacing2 = 2,
    Spacing3 = 3,
    Spacing4 = 4,
    Spacing5 = 5,
    Spacing6 = 6,
    Spacing7 = 7,
    Spacing8 = 8,
    Spacing9 = 9
  }

  public enum BorderRole { None, Small, Normal, Large, ExtraLarge }

  public static class RoleExtensions {
    public static ColorRole Container(this ColorRole role) {
      return role | ColorRole.Container;
    }

    public static ColorRole On(this ColorRole role) {
      return role | ColorRole.On;
    }

    public static ColorRole OnContainer(this ColorRole role) {
      return role | ColorRole.Container | ColorRole.On;
    }

    public static ColorRole Background(this ColorRole role) {
      return role & ~ColorRole.On;
    }
  }

  public enum BlendLevel { DisabledLow, DisabledHigh, Low, Normal, High }

  public struct ColorTokenPalette {
    public ColorPair main;
    public ColorPair container;

    public ColorTokenPalette(ColorPair main, ColorPair container) {
      this.main = main;
      this.container = container;
    }

    public ColorTokenPalette(ColorPair main) : this() {
      this.main = main;
      container = main;
    }

    public ColorTokenPalette(Color main, Color container, Color foreground) {
      this.main = new ColorPair(main, foreground);
      this.container = new ColorPair(container, foreground);
    }
  }

  public struct ColorPair {
    public static readonly ColorPair WhiteOnBlack = new(Colors.Black, Colors.White);
    public static readonly ColorPair BlackOnWhite = new(Colors.White, Colors.Black);
    public static readonly ColorPair White = new(Colors.Transparent, Colors.White);
    public static readonly ColorPair Black = new(Colors.Transparent, Colors.Black);

    public Color value;
    public Color onValue;

    public ColorPair(Color value, Color onValue) {
      this.value = value;
      this.onValue = onValue;
    }
  }

  public struct BlendProgression {
    public static readonly BlendProgression Default = new() {
      disabledLow = 0.1f,
      disabledHigh = 0.38f,
      low = 0.08f,
      normal = 0.12f,
      high = 0.38f
    };

    public float disabledHigh;
    public float disabledLow;
    public float high;
    public float low;
    public float normal;
  }

  public struct RadiusProgression {
    public float radius1;
    public float radius2;
    public float radius3;
    public float radius4;
    public float radius5;
    public float radius6;

    public static RadiusProgression Generate(float basis, float factor) {
      var unit = basis / 3f * factor;
      return new RadiusProgression {
        radius1 = unit * 3,
        radius2 = unit * 4,
        radius3 = unit * 6,
        radius4 = unit * 8,
        radius5 = unit * 12,
        radius6 = unit * 16
      };
    }
  }

  public struct SpacingProgression {
    public float spacing1;
    public float spacing2;
    public float spacing3;
    public float spacing4;
    public float spacing5;
    public float spacing6;
    public float spacing7;
    public float spacing8;
    public float spacing9;

    public static SpacingProgression Generate(float basis, float factor) {
      var unit = basis * factor;
      return new SpacingProgression {
        spacing1 = unit,
        spacing2 = unit * 2,
        spacing3 = unit * 3,
        spacing4 = unit * 4,
        spacing5 = unit * 6,
        spacing6 = unit * 8,
        spacing7 = unit * 10,
        spacing8 = unit * 12,
        spacing9 = unit * 16
      };
    }
  }

  public struct BorderProgression {
    public static readonly BorderProgression Default = new() {
      borderSmall = 1f,
      borderNormal = 2f,
      borderLarge = 4f,
      borderExtraLarge = 6f
    };

    public float borderSmall;
    public float borderNormal;
    public float borderLarge;
    public float borderExtraLarge;
  }

  public struct TypographyGroup {
    public TypographyToken small;
    public TypographyToken medium;
    public TypographyToken large;

    public static TypographyGroup Display(
      TextStyle basis,
      float factor = 1f,
      float lineHeightFactor = 1f,
      float letterSpacingFactor = 1f
    ) {
      factor *= 1.33f;
      var large = basis;
      large.size = factor * 57f;
      large.letterSpacing = factor * letterSpacingFactor * -0.25f;

      var medium = basis;
      medium.size = factor * 45f;
      medium.letterSpacing = factor * letterSpacingFactor * 0f;

      var small = basis;
      small.size = factor * 36f;
      small.letterSpacing = factor * letterSpacingFactor * 0f;

      return new TypographyGroup {
        large = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 64f,
          style = large
        },
        medium = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 52f,
          style = medium
        },
        small = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 44f,
          style = small
        }
      };
    }

    public static TypographyGroup Headline(
      TextStyle basis,
      float factor = 1f,
      float lineHeightFactor = 1f,
      float letterSpacingFactor = 1f
    ) {
      factor *= 1.33f;
      var large = basis;
      large.size = factor * 32f;
      large.letterSpacing = factor * letterSpacingFactor * 0f;

      var medium = basis;
      medium.size = factor * 28f;
      medium.letterSpacing = factor * letterSpacingFactor * 0f;

      var small = basis;
      small.size = factor * 24f;
      small.letterSpacing = factor * letterSpacingFactor * 0f;

      return new TypographyGroup {
        large = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 40f,
          style = large
        },
        medium = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 36f,
          style = medium
        },
        small = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 32f,
          style = small
        }
      };
    }

    public static TypographyGroup Title(
      TextStyle basis,
      float factor = 1f,
      float lineHeightFactor = 1f,
      float letterSpacingFactor = 1f
    ) {
      factor *= 1.33f;
      var large = basis;
      large.size = factor * 22f;
      large.letterSpacing = factor * letterSpacingFactor * 0f;

      var medium = basis;
      medium.size = factor * 16f;
      medium.letterSpacing = factor * letterSpacingFactor * 0.15f;

      var small = basis;
      small.size = factor * 14f;
      small.letterSpacing = factor * letterSpacingFactor * 0.1f;

      return new TypographyGroup {
        large = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 28f,
          style = large
        },
        medium = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 24f,
          style = medium
        },
        small = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 20f,
          style = small
        }
      };
    }

    public static TypographyGroup Label(
      TextStyle basis,
      float factor = 1f,
      float lineHeightFactor = 1f,
      float letterSpacingFactor = 1f
    ) {
      factor *= 1.33f;
      var large = basis;
      large.size = factor * 14f;
      large.letterSpacing = factor * letterSpacingFactor * 0.1f;

      var medium = basis;
      medium.size = factor * 12f;
      medium.letterSpacing = factor * letterSpacingFactor * 0.5f;

      var small = basis;
      small.size = factor * 11f;
      small.letterSpacing = factor * letterSpacingFactor * 0.5f;

      return new TypographyGroup {
        large = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 20f,
          style = large
        },
        medium = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 16f,
          style = medium
        },
        small = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 16f,
          style = small
        }
      };
    }

    public static TypographyGroup Body(
      TextStyle basis,
      float factor = 1f,
      float lineHeightFactor = 1f,
      float letterSpacingFactor = 1f
    ) {
      factor *= 1.33f;
      var large = basis;
      large.size = factor * 16f;
      large.letterSpacing = factor * letterSpacingFactor * 0.5f;

      var medium = basis;
      medium.size = factor * 14f;
      medium.letterSpacing = factor * letterSpacingFactor * 0.25f;

      var small = basis;
      small.size = factor * 12f;
      small.letterSpacing = factor * letterSpacingFactor * 0.4f;

      return new TypographyGroup {
        large = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 24f,
          style = large
        },
        medium = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 20f,
          style = medium
        },
        small = new TypographyToken {
          lineHeight = factor * lineHeightFactor * 16f,
          style = small
        }
      };
    }
  }

  public struct TypographyToken {
    public float lineHeight;
    public TextStyle style;

    public TypographyToken(float lineHeight, TextStyle style) {
      this.lineHeight = lineHeight;
      this.style = style;
    }
  }

  public static class ThemeProperties {
  }

  public abstract class ThemeProperty { }

  public class ThemeProperty<T> : ThemeProperty {
    public readonly bool hasDefault;
    public readonly T defaultValue;
    public readonly Func<ThemeData, T> computeFunc;

    public ThemeProperty(T defaultValue) {
      this.defaultValue = defaultValue;
      hasDefault = true;
    }

    public ThemeProperty(Func<ThemeData, T> computeFunc) {
      this.computeFunc = computeFunc;
      hasDefault = false;
    }

    public ThemeProperty() {
      hasDefault = false;
    }

    public virtual bool Compute(ThemeData themeData, out T value) {
      if (computeFunc != null) {
        value = computeFunc(themeData);
        return true;
      }
      value = default;
      return false;
    }

    public T this[ThemeData themeData] => themeData.GetComputedProperty(this);

    public T ReadScopeOrDefault() => this[ThemeData.Context.ReadScope()];
  }
}
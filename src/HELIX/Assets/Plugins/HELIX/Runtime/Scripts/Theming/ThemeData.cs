using System;
using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Compose;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Theming {
  public record ThemeData {
    public static readonly ContextKey<ThemeData> Key = new("Theme", HXThemes.DefaultDark);

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

    private Dictionary<ColorRole, ThemeProperty<Color>> _customColors = new();
    private Dictionary<ThemeProperty, object> _properties = new();
    private Dictionary<ThemeProperty, object> _computedProperties = new();

    public static ThemeData Build(Action<ThemeData> builder) {
      var theme = new ThemeData();
      builder(theme);
      return theme;
    }

    public Color GetColor(ColorRole role) {
      var resolvedColor = role switch {
        ColorRole.None => surface.value.WithOpacity(0),
        ColorRole.Transparent => surface.value.WithOpacity(0),

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


      return resolvedColor;
    }

    private Color ResolvedFallbackColor(ColorRole role) {
      if (_customColors.TryGetValue(role, out var color)) {
        return GetProperty(color);
      }

      // Special handling to make transparent be based on the color it is used with
      if (role.HasFlag(ColorRole.Transparent)) {
        var without = role & ~ColorRole.Transparent;
        if (without == ColorRole.None) without = ColorRole.Surface;
        var reference = this[without];
        return reference.WithOpacity(0f);
      }

      throw new ArgumentOutOfRangeException(nameof(role), role, null);
    }

    public ThemeData Copy() {
      var copy = this with { };
      copy._customColors = new Dictionary<ColorRole, ThemeProperty<Color>>(_customColors);
      copy._properties = new Dictionary<ThemeProperty, object>(_properties);
      copy._computedProperties = new Dictionary<ThemeProperty, object>(_computedProperties);
      return copy;
    }

    public Color this[ColorRole role] => GetColor(role);
    public float this[BlendLevel level] => GetBlendLevel(level);
    public float this[BorderRole role] => GetBorderWidth(role);
    public float this[RadiusRole role] => GetRadius(role);
    public float this[SpacingRole role] => GetSpacing(role);
    public ref TypographyToken this[TextRole role] => ref GetTypographyTokenRef(role);

    public T GetProperty<T>(ThemeProperty<T> property) {
      if (_properties.TryGetValue(property, out var value)) {
        return (T)value;
      }
      if (_computedProperties.TryGetValue(property, out var computedValue)) {
        return (T)computedValue;
      }

      if (property.Compute(this, out var computed)) {
        _computedProperties[property] = computed;
        return computed;
      }

      if (property.hasDefault) {
        _computedProperties[property] = property.defaultValue;
        return property.defaultValue;
      }

      throw new KeyNotFoundException(
        $"Theme property {property} was not found, has no default value and can't be computed"
      );
    }

    public void SetProperty<T>(ThemeProperty<T> property, T value) {
      _properties[property] = value;
      _computedProperties.Clear();
    }

    public void SetColorProvider(ThemeProperty<Color> provider, ColorRole role) => _customColors[role] = provider;

    public float GetBlendLevel(BlendLevel level) {
      return level switch {
        BlendLevel.None => 0f,
        BlendLevel.AccentLow => blend.accentLow,
        BlendLevel.AccentHigh => blend.accentHigh,
        BlendLevel.Low => blend.low,
        BlendLevel.Normal => blend.normal,
        BlendLevel.High => blend.high,
        BlendLevel.Full => 1f,
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
      var value = role & RadiusRole.ValueMask;
      var current = value switch {
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
      var step = role & ~RadiusRole.ValueMask;
      if (step == RadiusRole.None) return current;
      var time = 0f;
      if (step.HasFlag(RadiusRole.Half)) time += 0.5f;
      if (step.HasFlag(RadiusRole.Quarter)) time += 0.25f;
      var previous = value switch {
        RadiusRole.None => 0f,
        RadiusRole.Radius1 => 0f,
        RadiusRole.Radius2 => radius.radius1,
        RadiusRole.Radius3 => radius.radius2,
        RadiusRole.Radius4 => radius.radius3,
        RadiusRole.Radius5 => radius.radius4,
        RadiusRole.Radius6 => radius.radius5,
        RadiusRole.Round => radius.radius6,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
      };
      return Mathf.Lerp(previous, current, time);
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
      return Colors.Lerp(fromColor, toColor, t);
    }

    public Color ContrastLerp(ColorRole from, ColorRole to, BlendLevel level) {
      var fromColor = GetColor(from);
      var toColor = GetColor(to);
      var t = GetBlendLevel(level);
      return Colors.ContrastBlend(fromColor, toColor, t);
    }

    public Color ContrastLerp(Color from, Color to, BlendLevel level) {
      var t = GetBlendLevel(level);
      return Colors.ContrastBlend(from, to, t);
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
    Transparent = 1 << 26,
    None = 0,

    Scrim = 1 << 1 | Colors,
    Shadow = 1 << 2 | Colors,
    SurfaceTint = 1 << 3 | Colors,
    Outline = 1 << 4 | Colors,
    Focus = 1 << 5 | Colors,
    DisabledLow = 1 << 6 | Colors,
    DisabledHigh = 1 << 7 | Colors
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

  [Flags]
  public enum RadiusRole {
    None = 0,
    Radius1 = 1 << 0,
    Radius2 = 1 << 1,
    Radius3 = 1 << 2,
    Radius4 = 1 << 3,
    Radius5 = 1 << 4,
    Radius6 = 1 << 5,
    Round = 1 << 6,
    Quarter = 1 << 10,
    Half = 1 << 11,
    QuarterHalf = Quarter | Half,
    ValueMask = Radius1 | Radius2 | Radius3 | Radius4 | Radius5 | Radius6
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

  public enum BlendLevel {
    None,
    Low, Normal, High, AccentLow, AccentHigh,
    Full
  }

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
      low = 0.08f, normal = 0.12f, high = 0.38f, accentLow = 0.05f, accentHigh = 0.12f
    };

    public float low;
    public float normal;
    public float high;
    public float accentLow;
    public float accentHigh;
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
        radius1 = unit * 3, radius2 = unit * 4, radius3 = unit * 6, radius4 = unit * 8, radius5 = unit * 12,
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
        spacing1 = unit, spacing2 = unit * 2, spacing3 = unit * 3, spacing4 = unit * 4, spacing5 = unit * 6,
        spacing6 = unit * 8, spacing7 = unit * 10, spacing8 = unit * 12, spacing9 = unit * 16
      };
    }
  }

  public struct BorderProgression {
    public static readonly BorderProgression Default = new() {
      borderSmall = 1f, borderNormal = 1f, borderLarge = 2f, borderExtraLarge = 4f
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
        large = new TypographyToken { lineHeight = factor * lineHeightFactor * 64f, style = large },
        medium = new TypographyToken { lineHeight = factor * lineHeightFactor * 52f, style = medium },
        small = new TypographyToken { lineHeight = factor * lineHeightFactor * 44f, style = small }
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
        large = new TypographyToken { lineHeight = factor * lineHeightFactor * 40f, style = large },
        medium = new TypographyToken { lineHeight = factor * lineHeightFactor * 36f, style = medium },
        small = new TypographyToken { lineHeight = factor * lineHeightFactor * 32f, style = small }
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
        large = new TypographyToken { lineHeight = factor * lineHeightFactor * 28f, style = large },
        medium = new TypographyToken { lineHeight = factor * lineHeightFactor * 24f, style = medium },
        small = new TypographyToken { lineHeight = factor * lineHeightFactor * 20f, style = small }
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
        large = new TypographyToken { lineHeight = factor * lineHeightFactor * 20f, style = large },
        medium = new TypographyToken { lineHeight = factor * lineHeightFactor * 16f, style = medium },
        small = new TypographyToken { lineHeight = factor * lineHeightFactor * 16f, style = small }
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
        large = new TypographyToken { lineHeight = factor * lineHeightFactor * 24f, style = large },
        medium = new TypographyToken { lineHeight = factor * lineHeightFactor * 20f, style = medium },
        small = new TypographyToken { lineHeight = factor * lineHeightFactor * 16f, style = small }
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

    public static implicit operator TextStyle(TypographyToken token) => token.style;
  }

  public abstract class ThemeProperty { }

  public sealed class ThemeProperty<T> : ThemeProperty {
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

    public bool Compute(ThemeData themeData, out T value) {
      if (computeFunc != null) {
        value = computeFunc(themeData);
        return true;
      }
      value = default;
      return false;
    }

    public T this[ThemeData themeData] {
      get => themeData.GetProperty(this);
      set => themeData.SetProperty(this, value);
    }

    public T this[VisualElement element] => ThemeData.Key.ReadAt(element).GetProperty(this);
    public T this[in Composition cx] => ThemeData.Key[in cx].GetProperty(this);
  }
}
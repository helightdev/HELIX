using HELIX.Coloring;
using UnityEngine;

namespace HELIX.Compose {
  public static class HXStyles {
    public static readonly StateProperty<BlendLevel>
      DefaultBlendLevels = new StatePropertyMap<BlendLevel> {
        [State.Active] = BlendLevel.Normal, [State.Hovered] = BlendLevel.Low, [State.None] = BlendLevel.None
      },
      DefaultAccentBlendLevels = new StatePropertyMap<BlendLevel> {
        [State.Active] = BlendLevel.AccentHigh, [State.Hovered] = BlendLevel.AccentLow, [State.None] = BlendLevel.None
      };

    public static StateProperty<BlendLevel> BlendLevelSelector(StateProperty<ColorRole> roles) {
      return StateProperties.Func(state => {
          var role = roles[state];
          var isBg = role.HasFlag(ColorRole.Container) || role.HasFlag(ColorRole.Surface) || role.HasFlag(ColorRole.On);
          return isBg ? DefaultBlendLevels[state] : DefaultAccentBlendLevels[state];
        }
      );
    }

    public static StateProperty<ColorRole> ColorOverlaySelector(StateProperty<ColorRole> roles) {
      return StateProperties.Func(state => {
          var role = roles[state];
          if (role is ColorRole.Transparent or ColorRole.None) return ColorRoles.OnSurface;
          if (role.HasFlag(ColorRole.On)) return role & ~ColorRole.On;
          return role | ColorRole.On;
        }
      );
    }

    public static StateProperty<Color> Resolve(
      this StateProperty<ColorRole> roles,
      ThemeData theme
    ) {
      return StateProperties.Func(states => theme[roles.ResolveOrDefault(states)]);
    }

    public static StateProperty<float> Resolve(
      this StateProperty<BlendLevel> roles,
      ThemeData theme
    ) {
      return StateProperties.Func(states => theme[roles[states]]);
    }

    public static StateProperty<Color> ContrastBlend(
      StateProperty<Color> color,
      StateProperty<Color> onColor,
      StateProperty<float> blendLevels
    ) {
      return StateProperties.Func(states =>
        Colors.ContrastBlend(color[states], onColor[states], blendLevels[states])
      );
    }

    public static void StateBlend(
      ThemeData theme,
      StateProperty<ColorRole> color,
      StateProperty<ColorRole> onColor,
      out StateProperty<Color> background,
      StateProperty<float> blendLevels = null,
      bool isBackground = false
    ) {
      onColor ??= ColorOverlaySelector(color);
      blendLevels ??= BlendLevelSelector(color).Resolve(theme);
      var onBlended = onColor.Resolve(theme);
      var blended = ContrastBlend(color.Resolve(theme), onBlended, blendLevels);
      background = new FuncStateProperty<Color>(state => {
          if (state.HasFlag(State.Disabled))
            return isBackground ? theme[ColorRoles.DisabledLow] : theme[ColorRoles.DisabledHigh];
          if (state.HasFlag(State.Error))
            return isBackground ? theme[ColorRoles.ErrorContainer] : theme[ColorRoles.Error];
          return blended[state];
        }
      );
    }

    public static void StateBlend(
      ThemeData theme,
      StateProperty<ColorRole> color,
      StateProperty<ColorRole> onColor,
      out StateProperty<Color> background,
      out StateProperty<Color> foreground,
      StateProperty<float> blendLevels = null,
      bool isBackground = true
    ) {
      onColor ??= ColorOverlaySelector(color);
      blendLevels ??= BlendLevelSelector(color).Resolve(theme);
      var onBlended = onColor.Resolve(theme);
      var blended = ContrastBlend(color.Resolve(theme), onBlended, blendLevels);
      background = new FuncStateProperty<Color>(state => {
          if (state.HasFlag(State.Disabled))
            return isBackground ? theme[ColorRoles.DisabledLow] : theme[ColorRoles.DisabledHigh];
          if (state.HasFlag(State.Error))
            return isBackground ? theme[ColorRoles.ErrorContainer] : theme[ColorRoles.Error];
          return blended[state];
        }
      );
      foreground = new FuncStateProperty<Color>(state => {
          if (state.HasFlag(State.Disabled)) return theme[ColorRoles.DisabledHigh];
          if (state.HasFlag(State.Error)) return theme[ColorRoles.OnError];
          return onBlended[state];
        }
      );
    }

    public static void StateBlend(
      ThemeData theme,
      StateProperty<ColorRole> inactive,
      StateProperty<ColorRole> onInactive,
      StateProperty<ColorRole> active,
      StateProperty<ColorRole> onActive,
      out StateProperty<Color> background,
      out StateProperty<Color> foreground,
      StateProperty<float> inactiveBlendLevels = null,
      StateProperty<float> activeBlendLevels = null,
      bool isBackground = true
    ) {
      onInactive ??= ColorOverlaySelector(inactive);
      onActive ??= ColorOverlaySelector(active);
      inactiveBlendLevels ??= BlendLevelSelector(inactive).Resolve(theme);
      activeBlendLevels ??= BlendLevelSelector(active).Resolve(theme);
      var onInactiveBlended = onInactive.Resolve(theme);
      var onActiveBlended = onActive.Resolve(theme);
      var inactiveBlended = ContrastBlend(inactive.Resolve(theme), onInactiveBlended, inactiveBlendLevels);
      var activeBlended = ContrastBlend(active.Resolve(theme), onActiveBlended, activeBlendLevels);
      background = new FuncStateProperty<Color>(state => {
          if (state.HasFlag(State.Disabled))
            return isBackground ? theme[ColorRoles.DisabledLow] : theme[ColorRoles.DisabledHigh];
          if (state.HasFlag(State.Error))
            return isBackground ? theme[ColorRoles.ErrorContainer] : theme[ColorRoles.Error];
          if (state.HasFlag(State.Selected)) return activeBlended[state];
          return inactiveBlended[state];
        }
      );
      foreground = new FuncStateProperty<Color>(state => {
          if (state.HasFlag(State.Disabled)) return theme[ColorRoles.DisabledHigh];
          if (state.HasFlag(State.Error)) return theme[ColorRoles.OnError];
          if (state.HasFlag(State.Selected)) return onActiveBlended[state];
          return onInactiveBlended[state];
        }
      );
    }

    public static StateProperty<TextStyle> TextColor(
      StateProperty<Color> color
    ) {
      return StateProperties.Func(state => new TextStyle(color: color[state]));
    }

    public static Composable<State> DefaultFocusOutline(
      ThemeData theme,
      ColorRole focusColor = ColorRoles.Focus,
      BorderRole border = BorderRole.Normal,
      BorderRole inset = BorderRole.Large,
      RadiusRole radius = RadiusRole.Radius2,
      State focusState = State.Focused,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      var outdent = focusStyle == ButtonFocusStyle.Outdent;
      return new HXSolidBoxStyle(
        radius: new AllStateProperty<BorderRadius>(outdent ? theme[radius] + theme[inset] : theme[radius]),
        border: new StatePropertyMap<Border> {
          [focusState] = Border.All(theme[border], theme[focusColor]), [State.None] = Border.None
        },
        position: new StatePropertyMap<StyleLength4> { [State.None] = outdent ? -theme[inset] : 0f }
      ).Bake();
    }
  }

  public enum ButtonFocusStyle { Outdent, Indent, IndentReserved }

  public struct HXSolidBoxStyle {
    public StateProperty<Border> border;
    public StateProperty<BorderRadius> radius;
    public StateProperty<Color> color;
    public StateProperty<float> opacity;
    public StateProperty<BoxConstraints> constraints;
    public StateProperty<StyleLength4> position;
    public StateProperty<bool> absolute;
    public StateProperty<TransitionOptions> transition;

    public HXSolidBoxStyle(
      StateProperty<Border> border = null,
      StateProperty<BorderRadius> radius = null,
      StateProperty<Color> color = null,
      StateProperty<float> opacity = null,
      StateProperty<BoxConstraints> constraints = null,
      StateProperty<StyleLength4> position = null,
      StateProperty<bool> absolute = null,
      StateProperty<TransitionOptions> transition = null
    ) {
      this.border = border ?? StateProperties.Never<Border>();
      this.radius = radius ?? StateProperties.Never<BorderRadius>();
      this.color = color ?? StateProperties.Never<Color>();
      this.opacity = opacity ?? StateProperties.Never<float>();
      this.constraints = constraints ?? StateProperties.Never<BoxConstraints>();
      this.position = position ?? StateProperties.Never<StyleLength4>();
      this.absolute = absolute ?? StateProperties.Never<bool>();
      this.transition = transition ?? StateProperties.Never<TransitionOptions>();
    }

    public readonly Composable<State> Bake() {
      var style = this;
      return (ref Composition cx, State state) => cx.DrawSolidBox(
        style.border.ResolveOrDefault(state, Border.None),
        style.radius.ResolveOrDefault(state, BorderRadius.None),
        style.color.ResolveOrDefault(state, Colors.Transparent),
        style.opacity.ResolveOrDefault(state, 1f),
        style.constraints.ResolveOrDefault(state, BoxConstraints.Initial),
        style.position.ResolveOrDefault(state, StyleLength4.Zero),
        style.absolute.ResolveOrDefault(state, true),
        style.transition.ResolveOrDefault(state, TransitionOptions.Default)
      );
    }
  }

  public struct HXImageStyle {
    private static BackgroundImage? _noImage;

    public StateProperty<BackgroundImage?> image;
    public StateProperty<Color> tint;
    public StateProperty<Color> background;
    public StateProperty<Border> border;
    public StateProperty<BorderRadius> radius;
    public StateProperty<float> opacity;
    public StateProperty<BoxConstraints> constraints;
    public StateProperty<StyleLength4> position;
    public StateProperty<bool> absolute;
    public StateProperty<TransitionOptions> transition;

    public HXImageStyle(
      StateProperty<BackgroundImage?> image = null,
      StateProperty<Color> tint = null,
      StateProperty<Color> background = null,
      StateProperty<Border> border = null,
      StateProperty<BorderRadius> radius = null,
      StateProperty<float> opacity = null,
      StateProperty<BoxConstraints> constraints = null,
      StateProperty<StyleLength4> position = null,
      StateProperty<bool> absolute = null,
      StateProperty<TransitionOptions> transition = null
    ) {
      this.image = image ?? StateProperties.Never<BackgroundImage?>();
      this.tint = tint ?? StateProperties.Never<Color>();
      this.background = background ?? StateProperties.Never<Color>();
      this.border = border ?? StateProperties.Never<Border>();
      this.radius = radius ?? StateProperties.Never<BorderRadius>();
      this.opacity = opacity ?? StateProperties.Never<float>();
      this.constraints = constraints ?? StateProperties.Never<BoxConstraints>();
      this.position = position ?? StateProperties.Never<StyleLength4>();
      this.absolute = absolute ?? StateProperties.Never<bool>();
      this.transition = transition ?? StateProperties.Never<TransitionOptions>();
    }

    public readonly Composable<State> Bake() {
      var style = this;
      return (ref Composition cx, State state) => {
        var hasImage = style.image.HasValueFor(state);
        ref var backgroundImage = ref _noImage;
        if (hasImage) backgroundImage = ref style.image.GetValueRef(state);
        cx.DrawImage(
          backgroundImage,
          style.tint.ResolveOrDefault(state, Colors.White),
          style.background.ResolveOrDefault(state, Colors.Transparent),
          style.border.ResolveOrDefault(state, Border.None),
          style.radius.ResolveOrDefault(state, BorderRadius.None),
          style.opacity.ResolveOrDefault(state, 1f),
          style.constraints.ResolveOrDefault(state, BoxConstraints.Initial),
          style.position.ResolveOrDefault(state, StyleLength4.Zero),
          style.absolute.ResolveOrDefault(state, true),
          style.transition.ResolveOrDefault(state, TransitionOptions.Default)
        );
      };
    }
  }

  public struct HXControlBoxStyle {
    public static readonly HXControlBoxStyle Default = ThemeProperties.ButtonToggle[HXThemes.DefaultDark];

    public StateProperty<StyleLength4> padding;
    public StateProperty<StyleLength4> margin;
    public StateProperty<Alignment> alignment;
    public StateProperty<BoxConstraints> constraints;
    public StateProperty<TextStyle> textStyle;
    public Composable<State> background;

    public HXControlBoxStyle(
      StateProperty<StyleLength4> padding = null,
      StateProperty<StyleLength4> margin = null,
      StateProperty<Alignment> alignment = null,
      StateProperty<BoxConstraints> constraints = null,
      StateProperty<TextStyle> textStyle = null,
      Composable<State> background = null
    ) {
      this.padding = padding ?? StateProperties.Never<StyleLength4>();
      this.margin = margin ?? StateProperties.Never<StyleLength4>();
      this.alignment = alignment ?? StateProperties.Never<Alignment>();
      this.constraints = constraints ?? StateProperties.Never<BoxConstraints>();
      this.textStyle = textStyle ?? StateProperties.Never<TextStyle>();
      this.background = background;
    }

    public readonly void ApplyColumn(State flag, IComposable composable) {
      var element = composable.Element;
      constraints.ResolveOrDefault(flag, BoxConstraints.Initial).Apply(element);
      alignment.ResolveOrDefault(flag, Alignment.Center).AlignAsColumn(element);
      element.Padding(padding.ResolveOrDefault(flag, StyleLength4.Zero));
      element.Margin(margin.ResolveOrDefault(flag, StyleLength4.Zero));
      composable.Flag |= UssFlag.GroupAlign | UssFlag.Size | UssFlag.Padding | UssFlag.Margin;
    }

    public readonly void RenderContext(in ContextAccessor context, State state) {
      TextStyle.Merge(in context, textStyle, state);
    }

    public readonly void RenderContainer(ref Composition cx, State state) {
      ApplyColumn(state, cx.boundary);
    }

    public readonly void RenderBackground(ref Composition cx, State state) {
      background?.Invoke(ref cx, state);
    }

    public readonly void RenderContent(ref Composition cx, State state) {
      ApplyColumn(state, cx.boundary);
      background?.Invoke(ref cx, state);
    }

    public readonly void RenderBoundary(ref Composition cx, State state) {
      ApplyColumn(state, cx.boundary);
      using (cx.WriteContext(out var context)) TextStyle.Merge(in context, textStyle, state);
      background?.Invoke(ref cx, state);
    }
  }
}
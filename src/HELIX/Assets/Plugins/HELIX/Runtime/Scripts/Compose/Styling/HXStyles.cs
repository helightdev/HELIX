using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine;

namespace HELIX.Compose {
  public static class HXStyles {
    public static readonly StateProperty<BlendLevel> DefaultBlendLevels = new StatePropertyMap<BlendLevel> {
      [State.Active] = BlendLevel.Normal,
      [State.Hovered] = BlendLevel.Low,
      [State.None] = BlendLevel.None
    };

    public static readonly StateProperty<BlendLevel> DefaultAccentBlendLevels = new StatePropertyMap<BlendLevel> {
      [State.Active] = BlendLevel.AccentHigh,
      [State.Hovered] = BlendLevel.AccentLow,
      [State.None] = BlendLevel.None
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
      this StateProperty<ColorRole> roles, ThemeData theme
    ) => StateProperties.Func(states => theme[roles.ResolveOrDefault(states)]);

    public static StateProperty<float> Resolve(
      this StateProperty<BlendLevel> roles, ThemeData theme
    ) => StateProperties.Func(states => theme[roles[states]]);

    public static StateProperty<Color> ContrastBlend(
      StateProperty<Color> color,
      StateProperty<Color> onColor,
      StateProperty<float> blendLevels
    ) => StateProperties.Func(states =>
      Colors.ContrastBlend(color[states], onColor[states], blendLevels[states])
    );

    public static void SimpleBlend(
      ThemeData theme,
      StateProperty<ColorRole> color,
      StateProperty<ColorRole> onColor,
      out StateProperty<Color> background,
      out StateProperty<Color> foreground,
      StateProperty<float> blendLevels = null
    ) {
      onColor ??= ColorOverlaySelector(color);
      blendLevels ??= BlendLevelSelector(color).Resolve(theme);
      var onBlended = onColor.Resolve(theme);
      var blended = ContrastBlend(color.Resolve(theme), onBlended, blendLevels);
      background = new FuncStateProperty<Color>(state => {
          if (state.HasFlag(State.Disabled)) return theme[ColorRoles.OnSurfaceBlendLow];
          if (state.HasFlag(State.Error)) return theme[ColorRoles.Error];
          return blended[state];
        }
      );
      foreground = new FuncStateProperty<Color>(state => {
          if (state.HasFlag(State.Disabled)) return theme[ColorRoles.OnSurfaceBlendHigh];
          if (state.HasFlag(State.Error)) return theme[ColorRoles.OnError];
          return onBlended[state];
        }
      );
    }

    public static void SimpleToggleBlend(
      ThemeData theme,
      StateProperty<ColorRole> inactive,
      StateProperty<ColorRole> onInactive,
      StateProperty<ColorRole> active,
      StateProperty<ColorRole> onActive,
      out StateProperty<Color> background,
      out StateProperty<Color> foreground,
      StateProperty<float> inactiveBlendLevels = null,
      StateProperty<float> activeBlendLevels = null
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
          if (state.HasFlag(State.Disabled)) return theme[ColorRoles.OnSurfaceBlendLow];
          if (state.HasFlag(State.Error)) return theme[ColorRoles.Error];
          if (state.HasFlag(State.Selected)) return activeBlended[state];
          return inactiveBlended[state];
        }
      );
      foreground = new FuncStateProperty<Color>(state => {
          if (state.HasFlag(State.Disabled)) return theme[ColorRoles.OnSurfaceBlendHigh];
          if (state.HasFlag(State.Error)) return theme[ColorRoles.OnError];
          if (state.HasFlag(State.Selected)) return onActiveBlended[state];
          return onInactiveBlended[state];
        }
      );
    }

    public static StateProperty<TextStyle> TextColor(
      StateProperty<Color> color
    ) => StateProperties.Func(state => new TextStyle(color: color[state]));

    public static Composable<State> FocusOutline(
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
          [focusState] = Border.All(theme[border], theme[focusColor]),
          [State.None] = Border.None
        },
        position: new StatePropertyMap<StyleLength4> {
          [State.None] = outdent ? -theme[inset] : 0f,
        }
      ).Bake();
    }

    public static void FocusBorderPosition(
      ButtonFocusStyle style,
      float focusMargin,
      float radius,
      out StateProperty<StyleLength4> positionProperty,
      out StateProperty<BorderRadius> radiusProperty,
      State baseState = State.None
    ) {
      var fMargin = focusMargin;
      var fRadius = Mathf.Max(radius - fMargin, 0f);

      if (style == ButtonFocusStyle.Outdent) {
        fMargin = 0f;
        fRadius = radius;
      }

      if (baseState == State.None) {
        radiusProperty = new StatePropertyMap<BorderRadius> {
          [State.Focused] = fRadius,
          [State.None] = style == ButtonFocusStyle.IndentReserved ? fRadius : radius
        };
        positionProperty = new StatePropertyMap<StyleLength4> {
          [State.Focused] = fMargin,
          [State.None] = style == ButtonFocusStyle.IndentReserved ? fMargin : 0f
        };
      } else {
        radiusProperty = new StatePropertyMap<BorderRadius> {
          [State.Focused | baseState] = fRadius,
          [baseState] = style == ButtonFocusStyle.IndentReserved ? fRadius : radius,
          [State.None] = radius
        };
        positionProperty = new StatePropertyMap<StyleLength4> {
          [State.Focused | baseState] = fMargin,
          [State.None | baseState] = style == ButtonFocusStyle.IndentReserved ? fMargin : 0f
        };
      }
    }

    public static Composable<State> Filled(
      ThemeData theme,
      ColorRole color = ColorRoles.Primary,
      ColorRole? overlayColor = null,
      RadiusRole radius = RadiusRole.Radius2,
      BorderRole focusMargin = BorderRole.Large,
      ColorRole disabledColor = ColorRoles.OnSurfaceBlendLow,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      var overlay = overlayColor ?? color | ColorRole.On;
      var hover = theme.BlendLerp(theme[color], theme[overlay], BlendLevel.AccentLow);
      var active = theme.BlendLerp(theme[color], theme[overlay], BlendLevel.AccentHigh);
      FocusBorderPosition(
        focusStyle,
        theme[focusMargin],
        theme[radius],
        out var positionProperty,
        out var radiusProperty
      );

      return new HXSolidBoxStyle(
        color: new StatePropertyMap<Color> {
          [State.Disabled] = theme[disabledColor],
          [State.Active] = active,
          [State.Hovered] = hover,
          [State.None] = theme[color],
        },
        position: positionProperty,
        radius: radiusProperty
      ).Bake();
    }

    public static Composable<State> Ghost(
      ThemeData theme,
      ColorRole color = ColorRoles.Transparent,
      ColorRole onColor = ColorRoles.OnSurface,
      ColorRole? overlayColor = null,
      ColorRole selectedOnColor = ColorRoles.Primary,
      ColorRole focusColor = ColorRoles.Focus,
      ColorRole disabledColor = ColorRoles.Transparent,
      RadiusRole radius = RadiusRole.Radius2,
      BorderRole focusBorder = BorderRole.Normal,
      BorderRole focusMargin = BorderRole.Large,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      var overlay = overlayColor ?? onColor;
      var hover = overlay | ColorRole.BlendLow;
      var pressed = overlay | ColorRole.BlendNormal;
      var selectedHover = selectedOnColor | ColorRole.BlendAccentLow;
      var selectedPressed = selectedOnColor | ColorRole.BlendAccentHigh;
      FocusBorderPosition(
        focusStyle, theme[focusMargin], theme[radius], out var positionProperty, out var radiusProperty
      );
      var background = new HXSolidBoxStyle(
        color: new StatePropertyMap<Color> {
          [State.Disabled] = theme[disabledColor],
          [State.Active | State.Selected] =
            Colors.AlphaBlend(theme[color], theme[selectedPressed]),
          [State.Hovered | State.Selected] =
            Colors.AlphaBlend(theme[color], theme[selectedHover]),
          [State.Selected] = theme[color],
          [State.Active] = Colors.AlphaBlend(theme[color], theme[pressed]),
          [State.Hovered] = Colors.AlphaBlend(theme[color], theme[hover]),
          [State.None] = theme[color]
        },
        position: positionProperty,
        radius: radiusProperty
      ).Bake();
      var focus = FocusOutline(
        theme,
        focusColor: focusColor,
        border: focusBorder,
        radius: radius,
        focusStyle: focusStyle
      );
      return (ref Composition cx, State state) => {
        background(ref cx, state);
        focus(ref cx, state);
      };
    }

    public static Composable<State> GhostToggle(
      ThemeData theme,
      ColorRole color = ColorRoles.Transparent,
      ColorRole onColor = ColorRoles.OnSurface,
      ColorRole? hoverColor = null,
      ColorRole? pressedColor = null,
      ColorRole? selectedColor = null,
      ColorRole selectedOnColor = ColorRoles.Primary,
      ColorRole? selectedHoverColor = null,
      ColorRole? selectedPressedColor = null,
      ColorRole focusColor = ColorRoles.Focus,
      ColorRole disabledColor = ColorRoles.Transparent,
      RadiusRole radius = RadiusRole.Radius2,
      BorderRole focusBorder = BorderRole.Normal,
      BorderRole focusMargin = BorderRole.Large,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      var hover = hoverColor ?? onColor | ColorRole.BlendLow;
      var pressed = pressedColor ?? onColor | ColorRole.BlendNormal;
      var selected = selectedColor ?? selectedOnColor | ColorRole.BlendLow;
      var selectedHover = selectedHoverColor ?? selectedOnColor | ColorRole.BlendAccentLow;
      var selectedPressed = selectedPressedColor ?? selectedOnColor | ColorRole.BlendAccentHigh;
      FocusBorderPosition(
        focusStyle, theme[focusMargin], theme[radius], out var positionProperty, out var radiusProperty
      );
      var background = new HXSolidBoxStyle(
        color: new StatePropertyMap<Color> {
          [State.Disabled] = theme[disabledColor],
          [State.Active | State.Selected] = theme[selectedPressed],
          [State.Hovered | State.Selected] = theme[selectedHover],
          [State.Selected] = theme[selected],
          [State.Active] = theme[pressed],
          [State.Hovered] = theme[hover],
          [State.None] = theme[color]
        },
        position: positionProperty,
        radius: radiusProperty
      ).Bake();
      var focus = FocusOutline(
        theme,
        focusColor: focusColor,
        border: focusBorder,
        radius: radius,
        focusStyle: focusStyle
      );
      return (ref Composition cx, State state) => {
        background(ref cx, state);
        focus(ref cx, state);
      };
    }

    public static Composable<State> InputBox(
      ThemeData theme,
      ColorRole color = ColorRoles.SurfaceContainer,
      ColorRole? hoverColor = ColorRoles.SurfaceContainerHigh,
      ColorRole? pressedColor = null,
      ColorRole? focusedColor = ColorRoles.SurfaceContainerHigh,
      ColorRole errorColor = ColorRoles.ErrorContainer,
      ColorRole disabledColor = ColorRoles.SurfaceContainerLow,
      ColorRole borderColor = ColorRoles.Outline,
      ColorRole? borderHoverColor = null,
      ColorRole? borderPressedColor = null,
      ColorRole borderFocusColor = ColorRoles.Focus,
      ColorRole borderErrorColor = ColorRoles.Error,
      ColorRole borderDisabledColor = ColorRoles.OnSurfaceBlendLow,
      BorderRole border = BorderRole.Small,
      BorderRole borderFocus = BorderRole.Normal,
      BorderRole borderError = BorderRole.Small,
      RadiusRole radius = RadiusRole.Radius2
    ) {
      var hover = hoverColor ?? color;
      var pressed = pressedColor ?? hover;
      var focused = focusedColor ?? color;
      var borderHover = borderHoverColor ?? borderColor;
      var borderPressed = borderPressedColor ?? borderHover;

      return new HXSolidBoxStyle(
        color: new StatePropertyMap<Color> {
          [State.Disabled] = theme[disabledColor],
          [State.Error] = theme[errorColor],
          [State.Focused] = theme[focused],
          [State.Active] = theme[pressed],
          [State.Hovered] = theme[hover],
          [State.None] = theme[color]
        },
        border: new StatePropertyMap<Border> {
          [State.Disabled] = Border.All(theme[border], theme[borderDisabledColor]),
          [State.Error] = Border.All(theme[borderError], theme[borderErrorColor]),
          [State.Focused] = Border.All(theme[borderFocus], theme[borderFocusColor]),
          [State.Active] = Border.All(theme[border], theme[borderPressed]),
          [State.Hovered] = Border.All(theme[border], theme[borderHover]),
          [State.None] = Border.All(theme[border], theme[borderColor])
        },
        radius: BorderRadius.All(theme[radius])
      ).Bake();
    }

    public static Composable<State> Outlined(
      ThemeData theme,
      ColorRole color = ColorRoles.Transparent,
      ColorRole disabledColor = ColorRoles.OnSurfaceBlendLow,
      ColorRole onDisabledColor = ColorRoles.OnSurfaceBlendHigh,
      ColorRole borderColor = ColorRoles.OnSurface | ColorRole.BlendHigh,
      ColorRole? overlayColor = null,
      ColorRole? borderFocusColor = ColorRoles.Focus,
      ColorRole? borderDisabledColor = null,
      BorderRole border = BorderRole.Small,
      BorderRole borderFocus = BorderRole.Normal,
      RadiusRole radius = RadiusRole.Radius2
    ) {
      var overlay = overlayColor ?? ColorRoles.OnSurface;

      return new HXSolidBoxStyle(
        color: new StatePropertyMap<Color> {
          [State.Disabled] = theme[disabledColor],
          [State.Active] = theme.BlendLerpContrast(theme[color], theme[overlay], BlendLevel.Normal),
          [State.Hovered] = theme.BlendLerpContrast(theme[color], theme[overlay], BlendLevel.Low),
          [State.None] = theme[color],
        },
        border: new StatePropertyMap<Border> {
          [State.Disabled] = Border.All(theme[border], theme[borderDisabledColor ?? onDisabledColor]),
          [State.Focused] = Border.All(theme[borderFocus], theme[borderFocusColor ?? borderColor]),
          [State.Active] = Border.All(
            theme[border], theme.BlendOverlay(theme[borderColor], theme[overlay], BlendLevel.Normal)
          ),
          [State.Hovered] = Border.All(
            theme[border], theme.BlendOverlay(theme[borderColor], theme[overlay], BlendLevel.Low)
          ),
          [State.None] = Border.All(theme[border], theme[borderColor])
        },
        radius: new AllStateProperty<BorderRadius>(theme[radius])
      ).Bake();
    }

    public static Composable<State> Toggle(
      ThemeData theme,
      ColorRole colorUnselected = ColorRoles.Secondary,
      ColorRole colorSelected = ColorRoles.Primary,
      ColorRole? selectedOverlay = null,
      ColorRole? unselectedOverlay = null,
      ColorRole disabledColor = ColorRoles.OnSurfaceBlendLow,
      RadiusRole radius = RadiusRole.Radius2,
      BorderRole focusMargin = BorderRole.Large,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      var sOverlay = selectedOverlay ?? colorSelected | ColorRole.On;
      var uOverlay = unselectedOverlay ?? colorUnselected | ColorRole.On;

      FocusBorderPosition(
        focusStyle,
        theme[focusMargin],
        theme[radius],
        out var positionProperty,
        out var radiusProperty
      );

      return new HXSolidBoxStyle(
        color: new StatePropertyMap<Color> {
          [State.Disabled] = theme[disabledColor],

          // Selected States
          [State.Active | State.Selected] = theme.BlendLerp(
            theme[colorSelected],
            theme[sOverlay],
            BlendLevel.AccentHigh
          ),
          [State.Hovered | State.Selected] = theme.BlendLerp(
            theme[colorSelected],
            theme[sOverlay],
            BlendLevel.AccentLow
          ),
          [State.Selected] = theme[colorSelected],

          // Unselected States
          [State.Active] = theme.BlendLerp(
            theme[colorUnselected],
            theme[uOverlay],
            BlendLevel.AccentHigh
          ),
          [State.Hovered] = theme.BlendLerp(
            theme[colorUnselected],
            theme[uOverlay],
            BlendLevel.AccentLow
          ),
          [State.None] = theme[colorUnselected]
        },
        position: positionProperty,
        radius: radiusProperty
      ).Bake();
    }

    public static Composable<State> ToggleFocus(
      ThemeData theme,
      ColorRole colorUnselected = ColorRoles.Transparent,
      ColorRole colorSelected = ColorRoles.Primary,
      ColorRole? selectedOverlay = null,
      ColorRole? unselectedOverlay = null,
      ColorRole? borderFocusColor = ColorRoles.Focus,
      ColorRole disabledColor = ColorRoles.OnSurfaceBlendLow,
      RadiusRole radius = RadiusRole.Radius2,
      BorderRole focusMargin = BorderRole.Large,
      BorderRole borderFocus = BorderRole.Normal,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      var focus = FocusOutline(
        theme,
        focusColor: borderFocusColor ?? ColorRoles.OnPrimary,
        border: borderFocus,
        radius: radius,
        focusStyle: focusStyle,
        focusState: State.Focused
      );
      var background = Toggle(
        theme, colorUnselected: colorUnselected, colorSelected: colorSelected, selectedOverlay: selectedOverlay,
        unselectedOverlay: unselectedOverlay, disabledColor: disabledColor, radius: radius, focusMargin: focusMargin,
        focusStyle: focusStyle
      );

      return (ref Composition cx, State state) => {
        background(cx: ref cx, state);
        focus(cx: ref cx, state);
      };
    }

    public static Composable<State> FilledFocus(
      ThemeData theme,
      ColorRole color = ColorRoles.Primary,
      ColorRole focusColor = ColorRoles.Focus,
      ColorRole? overlayColor = null,
      RadiusRole radius = RadiusRole.Radius2,
      BorderRole focusBorder = BorderRole.Normal,
      BorderRole focusMargin = BorderRole.Large,
      ColorRole disabledColor = ColorRoles.OnSurfaceBlendLow,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      var focus = FocusOutline(
        theme, focusColor: focusColor, border: focusBorder, radius: radius, focusStyle: focusStyle
      );
      var flat = Filled(
        theme, color: color, overlayColor: overlayColor, radius: radius,
        focusMargin: focusMargin, disabledColor: disabledColor, focusStyle: focusStyle
      );
      return (ref Composition cx, State state) => {
        flat(cx: ref cx, state);
        focus(cx: ref cx, state);
      };
    }


    public static Composable<State> SliderTrack(
      ThemeData theme,
      ColorRole color = ColorRoles.SurfaceContainer,
      RadiusRole role = RadiusRole.Radius1 | RadiusRole.Quarter
    ) {
      return new HXSolidBoxStyle(
        color: theme[color],
        radius: BorderRadius.All(theme[role])
      ).Bake();
    }

    public static Composable<State> SliderProgress(
      ThemeData theme,
      ColorRole color = ColorRoles.Primary,
      ColorRole disabledColor = ColorRoles.OnSurfaceBlendLow,
      RadiusRole radius = RadiusRole.Radius1 | RadiusRole.Quarter
    ) {
      return new HXSolidBoxStyle(
        color: new StatePropertyMap<Color> {
          [State.Disabled] = theme[disabledColor],
          [State.None] = theme[color]
        },
        radius: BorderRadius.All(theme[radius])
      ).Bake();
    }

    public static Composable<State> SliderThumb(
      ThemeData theme,
      ColorRole color = ColorRoles.Primary,
      ColorRole overlayColor = ColorRoles.OnPrimary,
      ColorRole focusColor = ColorRoles.Focus,
      ColorRole disabledColor = ColorRoles.OnSurfaceBlendHigh,
      RadiusRole radius = RadiusRole.Radius1,
      BorderRole focusBorder = BorderRole.Normal,
      BorderRole focusMargin = BorderRole.Large
    ) {
      return FilledFocus(
        theme,
        color: color,
        focusColor: focusColor,
        overlayColor: overlayColor,
        radius: radius,
        focusBorder: focusBorder,
        focusMargin: focusMargin,
        disabledColor: disabledColor,
        focusStyle: ButtonFocusStyle.Outdent
      );
    }

    public static Composable<State> CheckboxOutline(
      ThemeData theme,
      ColorRole color = ColorRoles.OnSurfaceContainerHigh | ColorRole.BlendNormal,
      ColorRole activeColor = ColorRoles.OnSurfaceContainerHigh | ColorRole.BlendHigh,
      ColorRole disabledColor = ColorRoles.OnSurfaceBlendLow,
      ColorRole focusColor = ColorRoles.Focus,
      BorderRole border = BorderRole.Small,
      RadiusRole radius = RadiusRole.Radius1 | RadiusRole.Half
    ) {
      return new HXSolidBoxStyle(
        color: Colors.Transparent,
        border: new StatePropertyMap<Border> {
          [State.Disabled] = Border.All(theme[border], theme[disabledColor]),
          [State.Focused] = Border.All(theme[border], theme[focusColor]),
          [State.Active] = Border.All(theme[border], theme[activeColor]),
          [State.Hovered] = Border.All(theme[border], theme[activeColor]),
          [State.None] = Border.All(theme[border], theme[color])
        },
        radius: BorderRadius.All(theme[radius])
      ).Bake();
    }

    public static Composable<State> CheckboxFill(
      ThemeData theme,
      ColorRole activeColor = ColorRoles.Primary,
      ColorRole disabledColor = ColorRoles.OnSurfaceBlendHigh,
      BorderRole borderInset = BorderRole.Small,
      BorderRole selfInset = BorderRole.Normal,
      RadiusRole radius = RadiusRole.Radius1 | RadiusRole.Quarter
    ) {
      return new HXSolidBoxStyle(
        color: new StatePropertyMap<Color> {
          [State.Disabled] = theme[disabledColor],
          [State.Selected] = theme[activeColor],
          [State.None] = Colors.Transparent
        },
        position: StyleLength4.All(theme[borderInset] + theme[selfInset]),
        radius: BorderRadius.All(theme[radius])
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
        border: style.border.ResolveOrDefault(state, Border.None),
        radius: style.radius.ResolveOrDefault(state, BorderRadius.None),
        color: style.color.ResolveOrDefault(state, Colors.Transparent),
        opacity: style.opacity.ResolveOrDefault(state, 1f),
        constraints: style.constraints.ResolveOrDefault(state, BoxConstraints.Initial),
        position: style.position.ResolveOrDefault(state, StyleLength4.Zero),
        absolute: style.absolute.ResolveOrDefault(state, true),
        transition: style.transition.ResolveOrDefault(state, TransitionOptions.Default)
      );
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

    public readonly void RenderContent(ref Composition cx, State state) {
      ApplyColumn(state, cx.boundary);
      background?.Invoke(ref cx, state);
    }

    public readonly void RenderBoundary(ref Composition cx, State state) {
      ApplyColumn(state, cx.boundary);
      using (cx.WriteContext(out var context)) {
        TextStyle.Merge(in context, textStyle, state);
      }
      background?.Invoke(ref cx, state);
    }
  }
}
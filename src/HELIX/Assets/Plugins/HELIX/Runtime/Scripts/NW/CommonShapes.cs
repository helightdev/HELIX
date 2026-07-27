using HELIX.Coloring;
using HELIX.Types;
using UnityEngine;

namespace HELIX.NW {
  public static class CommonShapes {
    public static StateComposable FocusOutline(
      ThemeData theme,
      ColorRole focusColor = ColorRoles.Focus,
      BorderRole border = BorderRole.Normal,
      BorderRole inset = BorderRole.Large,
      RadiusRole radius = RadiusRole.Radius2,
      StateFlag focusState = StateFlag.Focused,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      var outdent = focusStyle == ButtonFocusStyle.Outdent;
      return new DrawSolidBoxStyle(
        radius: new AllStateProperty<BorderRadius>(outdent ? theme[radius] + theme[inset] : theme[radius]),
        border: new StatePropertyMap<Border> {
          [focusState] = Border.All(theme[border], theme[focusColor]), [StateFlag.None] = Border.None
        },
        position: new StatePropertyMap<StyleLength4> { [StateFlag.None] = outdent ? -theme[inset] : 0f, }
      ).Bake();
    }

    public static void FocusBorderPosition(
      ButtonFocusStyle style,
      float focusMargin,
      float radius,
      out StateProperty<StyleLength4> positionProperty,
      out StatePropertyMap<BorderRadius> radiusProperty,
      StateFlag baseState = StateFlag.None
    ) {
      var fMargin = focusMargin;
      var fRadius = Mathf.Max(radius - fMargin, 0f);

      if (style == ButtonFocusStyle.Outdent) {
        fMargin = 0f;
        fRadius = radius;
      }

      if (baseState == StateFlag.None) {
        radiusProperty = new StatePropertyMap<BorderRadius> {
          [StateFlag.Focused] = fRadius, [StateFlag.None] = style == ButtonFocusStyle.IndentReserved ? fRadius : radius
        };
        positionProperty = new StatePropertyMap<StyleLength4> {
          [StateFlag.Focused] = fMargin, [StateFlag.None] = style == ButtonFocusStyle.IndentReserved ? fMargin : 0f
        };
      } else {
        radiusProperty = new StatePropertyMap<BorderRadius> {
          [StateFlag.Focused | baseState] = fRadius,
          [baseState] = style == ButtonFocusStyle.IndentReserved ? fRadius : radius, [StateFlag.None] = radius
        };
        positionProperty = new StatePropertyMap<StyleLength4> {
          [StateFlag.Focused | baseState] = fMargin,
          [StateFlag.None | baseState] = style == ButtonFocusStyle.IndentReserved ? fMargin : 0f
        };
      }
    }

    public static StateComposable Filled(
      ThemeData theme,
      ColorRole color = ColorRoles.Primary,
      ColorRole? onColor = null,
      ColorRole? overlayColor = null,
      ColorRole? hoverColor = null,
      ColorRole? pressedColor = null,
      RadiusRole radius = RadiusRole.Radius2,
      BorderRole focusMargin = BorderRole.Large,
      ColorRole disabledColor = ColorRoles.OnSurfaceDisabledLow,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      var on = onColor ?? color | ColorRole.On;
      var overlay = overlayColor ?? on;
      var hover = hoverColor ?? overlay | ColorRole.BlendLow;
      var pressed = pressedColor ?? overlay | ColorRole.BlendNormal;

      FocusBorderPosition(
        focusStyle,
        theme[focusMargin],
        theme[radius],
        out var positionProperty,
        out var radiusProperty
      );

      return new DrawSolidBoxStyle(
        color: new StatePropertyMap<Color> {
          [StateFlag.Disabled] = theme[disabledColor],
          [StateFlag.Pressed] = Colors.AlphaBlend(theme[color], theme[pressed]),
          [StateFlag.Hovered] = Colors.AlphaBlend(theme[color], theme[hover]), [StateFlag.None] = theme[color],
        },
        position: positionProperty,
        radius: radiusProperty
      ).Bake();
    }

    public static StateComposable Outlined(
      ThemeData theme,
      ColorRole color = ColorRoles.Transparent,
      ColorRole? onColor = null,
      ColorRole disabledColor = ColorRoles.OnSurfaceDisabledLow,
      ColorRole onDisabledColor = ColorRoles.OnSurfaceDisabledHigh,
      ColorRole? overlayColor = null,
      ColorRole? hoverColor = null,
      ColorRole? pressedColor = null,
      ColorRole borderColor = ColorRoles.OnSurface | ColorRole.BlendHigh,
      ColorRole? borderHoverColor = null,
      ColorRole? borderPressedColor = null,
      ColorRole? borderFocusColor = ColorRoles.OnPrimary,
      ColorRole? borderDisabledColor = null,
      BorderRole border = BorderRole.Small,
      BorderRole borderFocus = BorderRole.Normal,
      RadiusRole radius = RadiusRole.Radius2
    ) {
      var on = onColor ?? ColorRoles.OnSurface;
      var overlay = overlayColor ?? on;

      return new DrawSolidBoxStyle(
        color: new StatePropertyMap<Color> {
          [StateFlag.Disabled] = theme[disabledColor],
          [StateFlag.Pressed] = Colors.AlphaBlend(theme[color], theme[pressedColor ?? overlay | ColorRole.BlendNormal]),
          [StateFlag.Hovered] = Colors.AlphaBlend(theme[color], theme[hoverColor ?? overlay | ColorRole.BlendLow]),
          [StateFlag.None] = theme[color],
        },
        border: new StatePropertyMap<Border> {
          [StateFlag.Disabled] = Border.All(theme[border], theme[borderDisabledColor ?? onDisabledColor]),
          [StateFlag.Focused] = Border.All(theme[borderFocus], theme[borderFocusColor ?? borderColor]),
          [StateFlag.Pressed] = Border.All(theme[border], theme[borderPressedColor ?? borderColor]),
          [StateFlag.Hovered] = Border.All(theme[border], theme[borderHoverColor ?? borderColor]),
          [StateFlag.None] = Border.All(theme[border], theme[borderColor])
        },
        radius: new AllStateProperty<BorderRadius>(theme[radius])
      ).Bake();
    }

    public static StateComposable Toggle(
      ThemeData theme,
      ColorRole colorUnselected = ColorRoles.Transparent,
      ColorRole colorSelected = ColorRoles.Primary,
      ColorRole onUnselected = ColorRoles.OnSurface,
      ColorRole onSelected = ColorRoles.OnPrimary,
      ColorRole? selectedOverlay = null,
      ColorRole? selectedHover = null,
      ColorRole? selectedPressed = null,
      ColorRole? unselectedOverlay = null,
      ColorRole? unselectedHover = null,
      ColorRole? unselectedPressed = null,
      ColorRole borderColor = ColorRoles.OnSurface | ColorRole.BlendHigh,
      ColorRole? borderHoverColor = null,
      ColorRole? borderPressedColor = null,
      ColorRole? borderFocusColor = ColorRoles.Focus,
      ColorRole? borderDisabledColor = null,
      ColorRole disabledColor = ColorRoles.OnSurfaceDisabledLow,
      ColorRole onDisabledColor = ColorRoles.OnSurfaceDisabledHigh,
      RadiusRole radius = RadiusRole.Radius2,
      BorderRole focusMargin = BorderRole.Large,
      BorderRole border = BorderRole.Small,
      BorderRole borderFocus = BorderRole.Normal,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      var sOverlay = selectedOverlay ?? onSelected;
      var uOverlay = unselectedOverlay ?? onUnselected;

      FocusBorderPosition(
        focusStyle,
        theme[focusMargin],
        theme[radius],
        out var positionProperty,
        out var radiusProperty,
        StateFlag.Selected
      );

      return new DrawSolidBoxStyle(
        color: new StatePropertyMap<Color> {
          [StateFlag.Disabled] = theme[disabledColor],

          // Selected States
          [StateFlag.Pressed | StateFlag.Selected] = Colors.AlphaBlend(
            theme[colorSelected],
            theme[selectedPressed ?? sOverlay | ColorRole.BlendNormal]
          ),
          [StateFlag.Hovered | StateFlag.Selected] = Colors.AlphaBlend(
            theme[colorSelected],
            theme[selectedHover ?? sOverlay | ColorRole.BlendLow]
          ),
          [StateFlag.Selected] = theme[colorSelected],

          // Unselected States
          [StateFlag.Pressed] = Colors.AlphaBlend(
            theme[colorUnselected],
            theme[unselectedPressed ?? uOverlay | ColorRole.BlendNormal]
          ),
          [StateFlag.Hovered] = Colors.AlphaBlend(
            theme[colorUnselected],
            theme[unselectedHover ?? uOverlay | ColorRole.BlendLow]
          ),
          [StateFlag.None] = theme[colorUnselected]
        },
        border: new StatePropertyMap<Border> {
          [StateFlag.Selected] = Border.All(theme[border], Colors.Transparent),
          [StateFlag.Disabled] = Border.All(theme[border], theme[borderDisabledColor ?? onDisabledColor]),
          [StateFlag.Focused] = Border.All(theme[borderFocus], theme[borderFocusColor ?? borderColor]),
          [StateFlag.Pressed] = Border.All(theme[border], theme[borderPressedColor ?? borderColor]),
          [StateFlag.Hovered] = Border.All(theme[border], theme[borderHoverColor ?? borderColor]),
          [StateFlag.None] = Border.All(theme[border], theme[borderColor])
        },
        position: positionProperty,
        radius: radiusProperty
      ).Bake();
    }

    public static StateComposable ToggleFocus(
      ThemeData theme,
      ColorRole colorUnselected = ColorRoles.Transparent,
      ColorRole colorSelected = ColorRoles.Primary,
      ColorRole onUnselected = ColorRoles.OnSurface,
      ColorRole onSelected = ColorRoles.OnPrimary,
      ColorRole? selectedOverlay = null,
      ColorRole? selectedHover = null,
      ColorRole? selectedPressed = null,
      ColorRole? unselectedOverlay = null,
      ColorRole? unselectedHover = null,
      ColorRole? unselectedPressed = null,
      ColorRole borderColor = ColorRoles.OnSurface | ColorRole.BlendHigh,
      ColorRole? borderHoverColor = null,
      ColorRole? borderPressedColor = null,
      ColorRole? borderFocusColor = ColorRoles.Focus,
      ColorRole? borderDisabledColor = null,
      ColorRole disabledColor = ColorRoles.OnSurfaceDisabledLow,
      ColorRole onDisabledColor = ColorRoles.OnSurfaceDisabledHigh,
      RadiusRole radius = RadiusRole.Radius2,
      BorderRole focusMargin = BorderRole.Large,
      BorderRole border = BorderRole.Small,
      BorderRole borderFocus = BorderRole.Normal,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      var focus = FocusOutline(
        theme,
        focusColor: borderFocusColor ?? ColorRoles.OnPrimary,
        border: borderFocus,
        radius: radius,
        focusStyle: focusStyle,
        focusState: StateFlag.Focused | StateFlag.Selected
      );
      var background = Toggle(
        theme, colorUnselected: colorUnselected, colorSelected: colorSelected, onUnselected: onUnselected,
        onSelected: onSelected, selectedOverlay: selectedOverlay, selectedHover: selectedHover,
        selectedPressed: selectedPressed, unselectedOverlay: unselectedOverlay, unselectedHover: unselectedHover,
        unselectedPressed: unselectedPressed, borderColor: borderColor, borderHoverColor: borderHoverColor,
        borderPressedColor: borderPressedColor, borderFocusColor: borderFocusColor,
        borderDisabledColor: borderDisabledColor, disabledColor: disabledColor, onDisabledColor: onDisabledColor,
        radius: radius, focusMargin: focusMargin, border: border, borderFocus: borderFocus, focusStyle: focusStyle
      );


      return (ref Composition cx, StateFlag state) => {
        background(cx: ref cx, state: state);
        focus(cx: ref cx, state: state);
      };
    }

    public static StateComposable FilledFocus(
      ThemeData theme,
      ColorRole color = ColorRoles.Primary,
      ColorRole focusColor = ColorRoles.Focus,
      ColorRole? onColor = null,
      ColorRole? overlayColor = null,
      ColorRole? hoverColor = null,
      ColorRole? pressedColor = null,
      RadiusRole radius = RadiusRole.Radius2,
      BorderRole focusBorder = BorderRole.Normal,
      BorderRole focusMargin = BorderRole.Large,
      ColorRole disabledColor = ColorRoles.OnSurfaceDisabledLow,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      var focus = FocusOutline(
        theme, focusColor: focusColor, border: focusBorder, radius: radius, focusStyle: focusStyle
      );
      var flat = Filled(
        theme, color: color, onColor: onColor, overlayColor: overlayColor, hoverColor: hoverColor,
        pressedColor: pressedColor, radius: radius, focusMargin: focusMargin, disabledColor: disabledColor,
        focusStyle: focusStyle
      );
      return (ref Composition cx, StateFlag state) => {
        flat(cx: ref cx, state: state);
        focus(cx: ref cx, state: state);
      };
    }


    public static ControlBoxStyle FilledControlBox(
      ThemeData theme,
      ColorRole color = ColorRoles.Primary,
      ColorRole focusColor = ColorRoles.Focus,
      ColorRole? onColor = null,
      ColorRole? overlayColor = null,
      ColorRole? hoverColor = null,
      ColorRole? pressedColor = null,
      RadiusRole radius = RadiusRole.Radius2,
      BorderRole focusBorder = BorderRole.Normal,
      BorderRole focusMargin = BorderRole.Large,
      SpacingRole paddingHorizontal = SpacingRole.Spacing2,
      SpacingRole paddingVertical = SpacingRole.Spacing1,
      ColorRole disabledColor = ColorRoles.OnSurfaceDisabledLow,
      ColorRole onDisabledColor = ColorRoles.OnSurfaceDisabledHigh,
      TextAnchor alignment = TextAnchor.MiddleCenter,
      BoxConstraints? constraints = null,
      StateComposable background = null,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent
    ) {
      background ??= FilledFocus(
        theme, color: color, focusColor: focusColor, onColor: onColor, overlayColor: overlayColor,
        hoverColor: hoverColor, pressedColor: pressedColor, radius: radius, focusBorder: focusBorder,
        focusMargin: focusMargin, disabledColor: disabledColor, focusStyle: focusStyle
      );
      return new ControlBoxStyle(
        background: background,
        alignment: (Alignment)alignment,
        padding: new AllStateProperty<StyleLength4>(
          EdgeInsets.Symmetric(theme[paddingHorizontal], theme[paddingVertical])
        ),
        constraints: new AllStateProperty<BoxConstraints>(constraints ?? BoxConstraints.Initial),
        textStyle: new StatePropertyMap<TextStyle>() {
          [StateFlag.Disabled] = new TextStyle(color: theme[onDisabledColor]),
          [StateFlag.None] = new TextStyle(color: theme[onColor ?? color | ColorRole.On])
        }
      );
    }

    public static ControlBoxStyle OutlinedControlBox(
      ThemeData theme,
      ColorRole color = ColorRoles.Transparent,
      ColorRole onColor = ColorRoles.OnSurface,
      ColorRole disabledColor = ColorRoles.OnSurfaceDisabledLow,
      ColorRole onDisabledColor = ColorRoles.OnSurfaceDisabledHigh,
      ColorRole? overlayColor = null,
      ColorRole? hoverColor = null,
      ColorRole? pressedColor = null,
      ColorRole borderColor = ColorRoles.OnSurface | ColorRole.BlendHigh,
      ColorRole? borderHoverColor = null,
      ColorRole? borderPressedColor = null,
      ColorRole? borderFocusColor = ColorRoles.Focus,
      ColorRole? borderDisabledColor = null,
      BorderRole border = BorderRole.Small,
      BorderRole borderFocus = BorderRole.Normal,
      RadiusRole radius = RadiusRole.Radius2,
      SpacingRole paddingHorizontal = SpacingRole.Spacing2,
      SpacingRole paddingVertical = SpacingRole.Spacing1,
      TextAnchor alignment = TextAnchor.MiddleCenter,
      BoxConstraints? constraints = null,
      StateComposable background = null
    ) {
      background ??= Outlined(
        theme, color: color, onColor: onColor, disabledColor: disabledColor, onDisabledColor: onDisabledColor,
        overlayColor: overlayColor, hoverColor: hoverColor, pressedColor: pressedColor, borderColor: borderColor,
        borderHoverColor: borderHoverColor, borderPressedColor: borderPressedColor, borderFocusColor: borderFocusColor,
        borderDisabledColor: borderDisabledColor, border: border, borderFocus: borderFocus, radius: radius
      );
      return new ControlBoxStyle(
        background: background,
        alignment: (Alignment)alignment,
        padding: new AllStateProperty<StyleLength4>(
          EdgeInsets.Symmetric(theme[paddingHorizontal], theme[paddingVertical])
        ),
        constraints: new AllStateProperty<BoxConstraints>(constraints ?? BoxConstraints.Initial),
        textStyle: new StatePropertyMap<TextStyle>() {
          [StateFlag.Disabled] = new TextStyle(color: theme[onDisabledColor]),
          [StateFlag.None] = new TextStyle(color: theme[onColor])
        }
      );
    }

    public static ControlBoxStyle ToggleControlBox(
      ThemeData theme,
      ColorRole colorUnselected = ColorRoles.Transparent,
      ColorRole colorSelected = ColorRoles.Primary,
      ColorRole onUnselected = ColorRoles.OnSurface,
      ColorRole onSelected = ColorRoles.OnPrimary,
      ColorRole? selectedOverlay = null,
      ColorRole? selectedHover = null,
      ColorRole? selectedPressed = null,
      ColorRole? unselectedOverlay = null,
      ColorRole? unselectedHover = null,
      ColorRole? unselectedPressed = null,
      ColorRole borderColor = ColorRoles.OnSurface | ColorRole.BlendHigh,
      ColorRole? borderHoverColor = null,
      ColorRole? borderPressedColor = null,
      ColorRole? borderFocusColor = ColorRoles.Focus,
      ColorRole? borderDisabledColor = null,
      ColorRole disabledColor = ColorRoles.OnSurfaceDisabledLow,
      ColorRole onDisabledColor = ColorRoles.OnSurfaceDisabledHigh,
      RadiusRole radius = RadiusRole.Radius2,
      BorderRole focusMargin = BorderRole.Large,
      BorderRole border = BorderRole.Small,
      BorderRole borderFocus = BorderRole.Normal,
      ButtonFocusStyle focusStyle = ButtonFocusStyle.Outdent,
      SpacingRole paddingHorizontal = SpacingRole.Spacing2,
      SpacingRole paddingVertical = SpacingRole.Spacing1,
      TextAnchor alignment = TextAnchor.MiddleCenter,
      BoxConstraints? constraints = null,
      StateComposable background = null
    ) {
      background ??= ToggleFocus(
        theme, colorUnselected: colorUnselected, colorSelected: colorSelected, onUnselected: onUnselected,
        onSelected: onSelected, selectedOverlay: selectedOverlay, selectedHover: selectedHover,
        selectedPressed: selectedPressed, unselectedOverlay: unselectedOverlay, unselectedHover: unselectedHover,
        unselectedPressed: unselectedPressed, borderColor: borderColor, borderHoverColor: borderHoverColor,
        borderPressedColor: borderPressedColor, borderFocusColor: borderFocusColor,
        borderDisabledColor: borderDisabledColor, disabledColor: disabledColor, onDisabledColor: onDisabledColor,
        radius: radius, focusMargin: focusMargin, border: border, borderFocus: borderFocus, focusStyle: focusStyle
      );
      return new ControlBoxStyle(
        background: background,
        alignment: (Alignment)alignment,
        padding: new AllStateProperty<StyleLength4>(
          EdgeInsets.Symmetric(theme[paddingHorizontal], theme[paddingVertical])
        ),
        constraints: new AllStateProperty<BoxConstraints>(constraints ?? BoxConstraints.Initial),
        textStyle: new StatePropertyMap<TextStyle>() {
          [StateFlag.Disabled] = new TextStyle(color: theme[onDisabledColor]),
          [StateFlag.Selected] = new TextStyle(color: theme[onSelected]),
          [StateFlag.None] = new TextStyle(color: theme[onUnselected])
        }
      );
    }
  }

  public enum ButtonFocusStyle { Outdent, Indent, IndentReserved }
}
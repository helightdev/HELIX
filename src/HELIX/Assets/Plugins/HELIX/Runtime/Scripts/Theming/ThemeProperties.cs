using HELIX.Compose;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Theming {
  public static class ThemeProperties {
    public static readonly ThemeProperty<RadiusRole> ButtonBaseRadius = new(RadiusRole.Radius2);
    public static readonly ThemeProperty<ButtonFocusStyle> ButtonFocusMode = new(ButtonFocusStyle.Outdent);
    public static readonly ThemeProperty<ColorRole> ButtonFocusColor = new(ColorRoles.Focus);
    public static readonly ThemeProperty<BorderRole> ButtonFocusBorder = new(BorderRole.Normal);
    public static readonly ThemeProperty<BorderRole> ButtonFocusInset = new(BorderRole.Large);
    public static readonly ThemeProperty<SpacingRole> ButtonPaddingHorizontal = new(SpacingRole.Spacing2);
    public static readonly ThemeProperty<SpacingRole> ButtonPaddingVertical = new(SpacingRole.Spacing1);


    public static readonly ThemeProperty<BorderRadius> ButtonRadius =
      new(data => BorderRadius.All(data[ButtonBaseRadius[data]]));
    public static readonly ThemeProperty<StyleLength4> ButtonPadding = new(data => EdgeInsets.Symmetric(
        data[ButtonPaddingHorizontal[data]],
        data[ButtonPaddingVertical[data]]
      )
    );

    public static readonly ThemeProperty<HXControlBoxStyle> ButtonFilled = new(data => CreateButtonFilled(data));
    public static readonly ThemeProperty<HXControlBoxStyle> ButtonOutlined = new(data => CreateButtonOutlined(data));
    public static readonly ThemeProperty<HXControlBoxStyle> ButtonToggle = new(data => CreateButtonToggle(data));
    public static readonly ThemeProperty<HXControlBoxStyle> ButtonGhost = new(data => CreateButtonGhost(data));


    public static readonly ThemeProperty<Composable<State>> DefaultFocusOutline = new(x => HXStyles.FocusOutline(x));

    public static readonly ThemeProperty<Length> TextGap = new(data => data[SpacingRole.Spacing1]);
    public static readonly ThemeProperty<Length> DecoratorColumnGap = new(data => data[SpacingRole.Spacing1]);

    public static readonly ThemeProperty<TextStyle> LabelStyle = new(data => data[TextRole.LabelLarge]);
    public static readonly ThemeProperty<TextStyle> DescriptionStyle = new(data => data[TextRole.LabelMedium]);
    public static readonly ThemeProperty<TextStyle> PrefixStyle = new(data => data[TextRole.BodyMedium]);
    public static readonly ThemeProperty<TextStyle> SuffixStyle = new(data => data[TextRole.BodyMedium]);
    public static readonly ThemeProperty<TextStyle> DecoratorStyle = new(data => data[TextRole.LabelMedium]);

    public static HXControlBoxStyle CreateButtonFilled(
      ThemeData data,
      ColorRole color = ColorRoles.Primary,
      ColorRole? onColor = null
    ) {
      HXStyles.SimpleBlend(
        data, color, onColor,
        out var background,
        out var foreground
      );
      var solid = new HXSolidBoxStyle(
        color: background.Derive(States.Common),
        radius: ButtonRadius[data]
      ).Bake();
      var focus = DefaultFocusOutline[data];

      var textStyle = HXStyles.TextColor(foreground).Derive(States.Common);
      return new HXControlBoxStyle(
        background: (ref Composition cx, State value) => {
          solid(ref cx, value);
          focus(ref cx, value);
        },
        textStyle: textStyle,
        padding: ButtonPadding[data],
        alignment: Alignment.Center
      );
    }

    public static HXControlBoxStyle CreateButtonOutlined(
      ThemeData data,
      ColorRole color = ColorRoles.Transparent,
      ColorRole onColor = ColorRoles.OnSurface,
      ColorRole borderColor = ColorRoles.OnSurface | ColorRole.BlendHigh
    ) {
      HXStyles.SimpleBlend(
        data, color, onColor,
        out var background,
        out var foreground
      );
      HXStyles.SimpleBlend(
        data, borderColor, onColor,
        out var border,
        out _
      );
      var solid = new HXSolidBoxStyle(
        color: background.Derive(States.Common),
        border: StateProperties
          .Func(state => Border.All(1, state.HasFlag(State.Focused) ? data[ButtonFocusColor[data]] : border[state]))
          .Derive(States.CommonFocusable),
        radius: ButtonRadius[data]
      ).Bake();

      var textStyle = HXStyles.TextColor(foreground).Derive(States.Common);
      return new HXControlBoxStyle(
        background: solid,
        textStyle: textStyle,
        padding: ButtonPadding[data],
        alignment: Alignment.Center
      );
    }

    public static HXControlBoxStyle CreateButtonToggle(
      ThemeData data,
      ColorRole colorUnselected = ColorRoles.Secondary,
      ColorRole colorSelected = ColorRoles.Primary,
      ColorRole onUnselected = ColorRoles.OnSecondary,
      ColorRole onSelected = ColorRoles.OnPrimary
    ) {
      HXStyles.SimpleToggleBlend(
        data, colorUnselected, onUnselected, colorSelected, onSelected,
        out var background,
        out var foreground
      );
      var solid = new HXSolidBoxStyle(
        color: background.Derive(States.CommonSelectable),
        radius: ButtonRadius[data]
      ).Bake();
      var focus = DefaultFocusOutline[data];
      var textStyle = HXStyles.TextColor(foreground).Derive(States.CommonSelectable);
      return new HXControlBoxStyle(
        background: (ref Composition cx, State value) => {
          solid(ref cx, value);
          focus(ref cx, value);
        },
        textStyle: textStyle,
        padding: ButtonPadding[data],
        alignment: Alignment.Center
      );
    }

    public static HXControlBoxStyle CreateButtonGhost(
      ThemeData data,
      ColorRole color = ColorRoles.Transparent,
      ColorRole onColor = ColorRoles.OnSurface,
      ColorRole selectedOnColor = ColorRoles.Primary
    ) {
      HXStyles.SimpleToggleBlend(
        data, color, onColor, color, selectedOnColor,
        out var background,
        out var foreground
      );
      var solid = new HXSolidBoxStyle(
        color: background.Derive(States.CommonSelectable),
        radius: ButtonRadius[data]
      ).Bake();
      var focus = DefaultFocusOutline[data];
      var textStyle = HXStyles.TextColor(foreground).Derive(States.CommonSelectable);
      return new HXControlBoxStyle(
        background: (ref Composition cx, State value) => {
          solid(ref cx, value);
          focus(ref cx, value);
        },
        textStyle: textStyle,
        padding: ButtonPadding[data],
        alignment: Alignment.Center
      );
    }
  }
}
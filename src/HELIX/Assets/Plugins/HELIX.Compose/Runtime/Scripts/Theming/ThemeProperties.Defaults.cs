using HELIX.Coloring;
using UnityEngine;
using static HELIX.Compose.BlendLevel;
using static HELIX.Compose.ColorRoles;
using static HELIX.Compose.HXStyles;
using static HELIX.Compose.SpacingRole;
using static HELIX.Compose.StateProperties;
using static HELIX.Compose.States;
using static HELIX.Compose.TextRole;

namespace HELIX.Compose {
  public static partial class ThemeProperties {
    public static HXDecoratorStyle DefaultDecorator(ThemeData data) {
      return new HXDecoratorStyle(
        EdgeInsets.Zero,
        EdgeInsets.Symmetric(0f, data[Spacing1] * 0.25f),
        data[Spacing1],
        data[Spacing1],
        data[Spacing1] * 0.5f
      );
    }

    public static HXControlBoxStyle DefaultButtonFilled(
      ThemeData data,
      ColorRole color = Primary,
      ColorRole? onColor = null
    ) {
      StateBlend(data, color, onColor, out var background, out var foreground);
      var solid = new HXSolidBoxStyle(color: background.Derive(Common), radius: ButtonRadius[data]).Bake();
      var focus = DefaultFocusOutline[data];

      var textStyle = TextColor(foreground).Derive(Common);
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

    public static HXControlBoxStyle DefaultButtonOutlined(
      ThemeData data,
      ColorRole color = Transparent,
      ColorRole onColor = OnSurface,
      ColorRole borderColor = SurfaceContainerHighest,
      BorderRadius? radius = null
    ) {
      StateBlend(data, color, onColor, out var background, out var foreground);
      StateBlend(data, borderColor, onColor, out var border, out _);
      var solid = new HXSolidBoxStyle(
        color: background.Derive(Common),
        border: Func(state => Border.All(1, state.HasFlag(State.Focused) ? data[Focus] : border[state]))
          .Derive(CommonFocusable),
        radius: radius ?? ButtonRadius[data]
      ).Bake();

      var textStyle = TextColor(foreground).Derive(Common);
      return new HXControlBoxStyle(
        background: solid,
        textStyle: textStyle,
        padding: ButtonPadding[data],
        alignment: Alignment.Center
      );
    }

    public static HXControlBoxStyle DefaultCheckbox(
      ThemeData data,
      ColorRole color = Transparent,
      ColorRole onColor = OnSurface,
      ColorRole active = Primary,
      ColorRole borderColor = SurfaceContainerHighest
    ) {
      StateBlend(data, active, onColor, out var checkColor);
      StateBlend(data, borderColor, onColor, out var border);
      var outline = new HXSolidBoxStyle(
        color: data[color],
        border: Func(state =>
          Border.All(1, state.HasFlag(State.Focused) ? data[Focus] : border[state])
        ).Derive(CommonFocusable),
        radius: CheckRadius[data]
      ).Bake();
      var check = new HXSolidBoxStyle(
        color: checkColor.Derive(CommonSelectable),
        opacity: Func(state => state.HasFlag(State.Selected) ? 1f : 0f),
        position: CheckInset[data],
        radius: CheckInnerRadius[data]
      ).Bake();

      return new HXControlBoxStyle(
        constraints: BoxConstraints.Tight(CheckboxSize[data], CheckboxSize[data]),
        background: (ref Composition cx, State state) => {
          outline(ref cx, state);
          check(ref cx, state);
        },
        alignment: Alignment.Center
      );
    }

    public static HXControlBoxStyle DefaultButtonToggle(
      ThemeData data,
      ColorRole colorUnselected = Secondary,
      ColorRole colorSelected = Primary,
      ColorRole onUnselected = OnSecondary,
      ColorRole onSelected = OnPrimary,
      BorderRadius? radius = null
    ) {
      StateBlend(
        data,
        colorUnselected,
        onUnselected,
        colorSelected,
        onSelected,
        out var background,
        out var foreground
      );
      var solid = new HXSolidBoxStyle(
        color: background.Derive(CommonSelectable),
        radius: radius ?? ButtonRadius[data]
      ).Bake();
      var focus = DefaultFocusOutline[data];
      var textStyle = TextColor(foreground).Derive(CommonSelectable);
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

    public static HXControlBoxStyle DefaultButtonGhost(
      ThemeData data,
      ColorRole color = Transparent,
      ColorRole onColor = OnSurface,
      ColorRole selectedOnColor = Primary
    ) {
      StateBlend(data, color, onColor, color, selectedOnColor, out var background, out var foreground);
      var solid = new HXSolidBoxStyle(
        color: background.Derive(CommonSelectable),
        radius: ButtonRadius[data]
      ).Bake();
      var focus = DefaultFocusOutline(data, focusStyle: ButtonFocusStyle.Indent);
      var textStyle = TextColor(foreground).Derive(CommonSelectable);
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

    public static Composable<State> DefaultInputBox(
      ThemeData data,
      ColorRole color = SurfaceContainer,
      ColorRole onColor = OnSurfaceContainerHigh,
      ColorRole borderColor = SurfaceContainerHighest
    ) {
      StateBlend(data, color, onColor, out var background, out var foreground);
      StateBlend(data, borderColor, onColor, out var border);
      var solid = new HXSolidBoxStyle(
        color: background.Derive(Common),
        border: Func(state =>
          Border.All(1, state.HasFlag(State.Focused) ? data[Focus] : border[state])
        ).Derive(CommonFocusable),
        radius: InputBoxRadius[data]
      ).Bake();

      //var textStyle = HXStyles.TextColor(foreground).Derive(States.Common);
      return solid;
    }

    public static HXControlBoxStyle DefaultInputField(
      ThemeData data,
      ColorRole color = SurfaceContainer,
      ColorRole onColor = OnSurfaceContainerHigh,
      ColorRole borderColor = SurfaceContainerHighest,
      bool useButtonPadding = false,
      BorderRadius? radius = null
    ) {
      StateBlend(data, color, onColor, out var background, out var foreground);
      StateBlend(data, borderColor, onColor, out var border);
      var solid = new HXSolidBoxStyle(
        color: background.Derive(Common),
        border: Func(state =>
          Border.All(1, state.HasFlag(State.Focused) ? data[Focus] : border[state])
        ).Derive(CommonFocusable),
        radius: radius ?? InputBoxRadius[data]
      ).Bake();

      return new HXControlBoxStyle(
        margin: InputFieldMargin[data],
        padding: useButtonPadding ? ButtonPadding[data] : InputFieldPadding[data],
        constraints: InputFieldConstraints[data],
        textStyle: TextColor(foreground).Derive(Common),
        alignment: Alignment.CenterLeft,
        background: solid
      );
    }

    public static PopupMenuStyle DefaultDropdownButton(ThemeData data) {
      return DefaultPopupMenu(data, DefaultInputField(data, useButtonPadding: true), true);
    }

    public static PopupMenuStyle DefaultMenuButton(ThemeData data) {
      return DefaultPopupMenu(data, DefaultButtonOutlined(data), false);
    }

    public static SegmentedChoiceStyle DefaultSegmentedChoice(ThemeData data) {
      var radius = ButtonRadius[data];
      return new SegmentedChoiceStyle(
        DefaultButtonToggle(
          data,
          radius: BorderRadius.Only(
            radius.topLeft,
            bottomLeft: radius.bottomLeft
          )
        ),
        DefaultButtonToggle(data, radius: BorderRadius.None),
        DefaultButtonToggle(
          data,
          radius: BorderRadius.Only(
            topRight: radius.topRight,
            bottomRight: radius.bottomRight
          )
        ),
        gap: data[SpacingRole.None]
      );
    }

    public static SpinboxChoiceStyle DefaultChoiceSpinbox(ThemeData data) {
      var radius = ButtonRadius[data];
      return new SpinboxChoiceStyle(
        DefaultInputField(data, useButtonPadding: true, radius: BorderRadius.None),
        DefaultButtonOutlined(
          data,
          radius: BorderRadius.Only(
            radius.topLeft,
            bottomLeft: radius.bottomLeft
          )
        ),
        DefaultButtonOutlined(
          data,
          radius: BorderRadius.Only(
            topRight: radius.topRight,
            bottomRight: radius.bottomRight
          )
        ),
        DefaultButtonOutlined(
          data,
          radius: BorderRadius.Only(
            radius.topLeft,
            radius.topRight
          )
        ),
        DefaultButtonOutlined(
          data,
          radius: BorderRadius.Only(
            bottomLeft: radius.bottomLeft,
            bottomRight: radius.bottomRight
          )
        ),
        gap: data[Spacing1],
        wrap: false
      );
    }

    private static PopupMenuStyle DefaultPopupMenu(
      ThemeData data,
      HXControlBoxStyle button,
      bool matchAnchorWidth
    ) {
      var panelBackground = new HXSolidBoxStyle(
        Border.All(data[BorderRole.Normal], data[Outline]),
        InputBoxRadius[data],
        data[SurfaceContainer]
      ).Bake();
      var panel = new HXControlBoxStyle(
        EdgeInsets.All(data[Spacing1]),
        constraints: BoxConstraints.Null,
        textStyle: new TextStyle(color: data[OnSurface]),
        alignment: Alignment.CenterLeft,
        background: panelBackground
      );

      var itemBase = DefaultButtonGhost(data, SurfaceContainer);
      var item = new HXControlBoxStyle(
        itemBase.padding,
        itemBase.margin,
        Alignment.CenterLeft,
        BoxConstraints.Min(new StyleLength2(0f, data[BodyMedium].lineHeight)),
        itemBase.textStyle,
        itemBase.background
      );
      var heading = data[LabelSmall].style;
      heading.color = data[OnSurfaceVariant];

      var iconColor = data.ContrastLerp(Surface, OnSurfaceContainer, High);
      return new PopupMenuStyle(
        button,
        panel,
        item,
        heading,
        EdgeInsets.Symmetric(data[Spacing2], data[Spacing1]),
        data[Outline],
        iconColor,
        data[BorderRole.Normal],
        data[Spacing1] * 0.75f,
        new Vector2(0f, data[Spacing1]),
        new Vector2(Mathf.Lerp(data[Spacing1], data[Spacing2], 0.25f), 0f),
        matchAnchorWidth
      );
    }

    public static SliderStyle DefaultSlider(
      ThemeData data,
      ColorRole color = Primary,
      ColorRole onColor = OnPrimary,
      ColorRole trackColor = SurfaceContainer,
      bool useProgress = true
    ) {
      StateBlend(data, color, onColor, out var thumbBlend);
      StateBlend(data, trackColor, null, out var trackBlend);

      var thumbSolid = new HXSolidBoxStyle(
        color: thumbBlend.Derive(Common),
        radius: SliderThumbRadius[data]
      ).Bake();
      var thumbFocus = SliderThumbFocusOutline[data];
      var trackSolid = new HXSolidBoxStyle(
        radius: SliderTrackRadius[data],
        color: trackBlend.Derive(EnabledDisabledError)
      ).Bake();
      var progressSolid = new HXSolidBoxStyle(
        radius: SliderTrackRadius[data],
        color: useProgress ? thumbBlend.Derive(EnabledDisabledError) : Colors.Transparent
      ).Bake();

      return new SliderStyle(
        new HXControlBoxStyle(
          StyleLength4.Zero,
          alignment: Alignment.Center,
          constraints: BoxConstraints.Min(
            new StyleLength2(data.GetTypographyTokenRef(BodyMedium).lineHeight)
          )
        ),
        trackSolid,
        progressSolid,
        (ref Composition cx, State value) => {
          thumbSolid(ref cx, value);
          thumbFocus(ref cx, value);
        }
      );
    }

    public static SliderStyle DefaultBoxSlider(
      ThemeData data,
      ColorRole color = Primary,
      ColorRole onColor = OnPrimary,
      ColorRole trackColor = SurfaceContainer,
      bool useProgress = true
    ) {
      StateBlend(data, color, onColor, out var thumbBlend);
      StateBlend(data, trackColor, null, out var trackBlend);

      var thumbSolid = new HXSolidBoxStyle(
        color: thumbBlend.Derive(Common),
        radius: SliderThumbRadius[data]
      ).Bake();
      var thumbFocus = SliderThumbFocusOutline[data];
      var trackSolid = new HXSolidBoxStyle(
        radius: SliderThumbRadius[data],
        color: trackBlend.Derive(EnabledDisabledError)
      ).Bake();
      var progressSolid = new HXSolidBoxStyle(
        radius: SliderThumbRadius[data],
        color: useProgress ? thumbBlend.Derive(EnabledDisabledError) : Colors.Transparent
      ).Bake();

      return new SliderStyle(
        new HXControlBoxStyle(
          StyleLength4.Zero,
          alignment: Alignment.Center,
          constraints: BoxConstraints.Min(
            new StyleLength2(data.GetTypographyTokenRef(BodyMedium).lineHeight)
          )
        ),
        trackSolid,
        progressSolid,
        (ref Composition cx, State value) => {
          thumbSolid(ref cx, value);
          thumbFocus(ref cx, value);
        },
        thumbSize: 12,
        trackSize: 12,
        hideWhenThumbCoversTrack: true
      );
    }
  }
}
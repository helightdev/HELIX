using HELIX.Coloring;
using HELIX.Compose;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;
using static HELIX.Theming.BlendLevel;
using static HELIX.Theming.ColorRoles;
using static HELIX.Theming.HXStyles;
using static HELIX.Theming.RadiusRole;
using static HELIX.Theming.SpacingRole;
using static HELIX.Theming.TextRole;

namespace HELIX.Theming {
  public static partial class ThemeProperties {
    // Colors
    public static readonly ThemeProperty<Color>
      RoleDisabledHighProvider = new(data => data.ContrastLerp(Surface, OnSurface, High)),
      RoleDisabledLowProvider = new(data => data.ContrastLerp(Surface, OnSurface, Low));

    public static readonly ThemeProperty<TextSelectionStyle> TextSelectionStyle = new(data =>
      new TextSelectionStyle {
        cursor = data[OnSurface], selection = data[Focus].WithOpacity(0.4f), type = TextSelectionStyleType.Custom
      }
    );


    // Border Radii
    public static readonly ThemeProperty<BorderRadius>
      ButtonRadius = new(data => BorderRadius.All(data[Radius2])),
      SliderTrackRadius = new(data => BorderRadius.All(data[Quarter | Radius1])),
      SliderThumbRadius = new(data => BorderRadius.All(data[QuarterHalf | Radius1])),
      CheckRadius = new(data => BorderRadius.All(data[Radius1])),
      CheckInnerRadius = new(data => BorderRadius.All(data[Radius1 | Half])),
      InputBoxRadius = new(data => BorderRadius.All(data[Radius2]));

    // Insets
    public static readonly ThemeProperty<StyleLength4>
      ButtonPadding = new(data => EdgeInsets.Symmetric(data[Spacing2], data[Spacing1])),
      CheckInset = new(data => data[BorderRole.Large]),
      InputFieldPadding = new(data => EdgeInsets.Symmetric(data[Spacing1], data[Spacing1])),
      InputFieldMargin = new(data => EdgeInsets.Zero);

    public static readonly ThemeProperty<BoxConstraints>
      InputFieldConstraints = new(data => BoxConstraints.Min(new StyleLength2(data[BodyMedium].lineHeight)));

    // Lengths
    public static readonly ThemeProperty<Length>
      TextGap = new(data => data[Spacing1]),
      DecoratorColumnGap = new(data => data[Spacing1]),
      CompanionFieldWidth = new(_ => 80f),
      CheckboxSize = new(data => data.GetTypographyTokenRef(BodyMedium).lineHeight * 0.66f),
      ChevronSize = new(data => data.GetTypographyTokenRef(BodyMedium).lineHeight * 0.33f);

    public static readonly ThemeProperty<HXDecoratorStyle> Decorator = new(data => DefaultDecorator(data));

    // State Composables
    public static readonly ThemeProperty<Composable<State>>
      DefaultFocusOutline = new(data => DefaultFocusOutline(data)),
      SliderThumbFocusOutline = new(data => DefaultFocusOutline(data, radius: Radius1)),
      InputBox = new(data => DefaultInputBox(data));

    // Labels
    public static readonly ThemeProperty<TextStyle>
      LabelStyle = new(data => data[LabelLarge]),
      DescriptionStyle = new(data => data[LabelMedium]),
      PrefixStyle = new(data => data[BodyMedium]),
      SuffixStyle = new(data => data[BodyMedium]),
      DecoratorStyle = new(data => data[LabelMedium]);

    // Control Boxes
    public static readonly ThemeProperty<HXControlBoxStyle>
      ButtonFilled = new(data => DefaultButtonFilled(data)),
      ButtonOutlined = new(data => DefaultButtonOutlined(data)),
      ButtonToggle = new(data => DefaultButtonToggle(data)),
      ButtonGhost = new(data => DefaultButtonGhost(data)),
      Checkbox = new(data => DefaultCheckbox(data)),
      TextField = new(data => DefaultInputField(data));

    public static readonly ThemeProperty<PopupMenuStyle>
      DropdownButton = new(data => DefaultDropdownButton(data)),
      MenuButton = new(data => DefaultMenuButton(data));

    public static readonly ThemeProperty<SegmentedChoiceStyle>
      SegmentedChoice = new(data => DefaultSegmentedChoice(data));
    public static readonly ThemeProperty<SpinboxChoiceStyle>
      ChoiceSpinbox = new(data => DefaultChoiceSpinbox(data));

    // Special Components
    public static readonly ThemeProperty<SliderStyle> Slider = new(data => DefaultSlider(data));
    public static readonly ThemeProperty<SliderStyle>
      Scroller = new(data => DefaultBoxSlider(data, useProgress: false));
  }
}

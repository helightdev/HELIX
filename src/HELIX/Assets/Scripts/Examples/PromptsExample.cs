using System.Linq;
using HELIX.Coloring;
using HELIX.Widgets;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Prompts;
using HELIX.Widgets.Prompts.Kenny;
using HELIX.Widgets.Scrolling;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Axis = HELIX.Types.Axis;

namespace Examples {
  public class PromptsExample : StatefulWidget<PromptsExample> {
    public override State<PromptsExample> CreateState() {
      return new PromptsExampleState();
    }
  }

  public class PromptsExampleState : State<PromptsExample> {
    private bool _isOutline = false;
    private bool _isAlternative = false;
    private bool _isColored = false;
    private bool _isIcon = false;

    private IPromptProvider _provider = new KennyPromptProvider();

    public override Widget Build(BuildContext context) {
      var prompts = new KennyKeyboardMousePromptLayerProvider();
      return new HColumn {
        new HRow {
          new HButton(
            HButtonVariant.TwoState,
            selected: _isOutline,
            onClick: SetState(() => _isOutline = !_isOutline)
          ) { new HText("Outline") },
          new HButton(
            HButtonVariant.TwoState,
            selected: _isAlternative,
            onClick: SetState(() => _isAlternative = !_isAlternative)
          ) { new HText("Alternative") },
          new HButton(
            HButtonVariant.TwoState,
            selected: _isColored,
            onClick: SetState(() => _isColored = !_isColored)
          ) { new HText("Colored") },
          new HButton(
            HButtonVariant.TwoState,
            selected: _isIcon,
            onClick: SetState(() => _isIcon = !_isIcon)
          ) { new HText("Icon") }
        },
        new HScrollView {
          new HRow {
            new HButton(
              onClick: () => HelixInputController.Instance.SetValue(
                new InputConfiguration(InputDeviceType.KeyboardMouse, GamepadVariant.Generic)
              ),
              child: new HText("Keyboard&Mouse")
            ),

            new HButton(
              onClick: () => HelixInputController.Instance.SetValue(
                new InputConfiguration(InputDeviceType.Gamepad, GamepadVariant.Xbox)
              ),
              child: new HText("XBox")
            ),

            new HButton(
              onClick: () => HelixInputController.Instance.SetValue(
                new InputConfiguration(InputDeviceType.Gamepad, GamepadVariant.PlayStation)
              ),
              child: new HText("PlayStation")
            ),
          },
          new HRow {
            new HPrompt("Player/Move"),
            new HPrompt("Player/Look"),
            new HPrompt("Player/Attack")
          }.WithModifier(new BackgroundStyleModifier(Colors.Grey)),

          BuildPromptsFor(context, new KennyKeyboardMousePromptLayerProvider()),
          BuildPromptsFor(context, new KennyPlaystationSeriesPromptLayerProvider()),
          BuildPromptsFor(context, new KennyXboxSeriesPromptLayerProvider()),
          BuildPromptsFor(context, new KennySteamControllerPromptLayerProvider()),
          BuildPromptsFor(context, new KennySteamDeckPromptLayerProvider()),
          BuildPromptsFor(context, new KennyNintendoSwitch2PromptLayerProvider())
        }.Fill()
      };
    }

    public Widget BuildPromptsFor(BuildContext context, KennyResourcePromptLayerProvider layerProvider) {
      var variant = KennyVariant.None;
      if (_isOutline) variant |= KennyVariant.Outline;
      if (_isAlternative) variant |= KennyVariant.Alternative;
      if (_isColored) variant |= KennyVariant.Color;
      if (_isIcon) variant |= KennyVariant.Icon;
      layerProvider.Variant = variant;

      var widgets = layerProvider.Mapping.Keys.Select<string, Widget>(x => {
          if (layerProvider.TryResolvePromptLayer(context, x, out var layers)) {
            return new HSubstanceBox(substances: layers).Size(32, 32);
          }

          return new HText($"{x} (not found)");
        }
      ).ToArray();
      return new HBox(background: Colors.Grey) {
        new HColumn(crossAxisAlign: Align.Stretch) {
          new HText(layerProvider.GetType().Name).Heading(context),
          new HFlex(Axis.Horizontal, wrap: true, children: widgets).Fill()
        }
      };
    }
  }
}
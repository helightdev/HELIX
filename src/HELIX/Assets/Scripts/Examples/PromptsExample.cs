using System.Linq;
using HELIX.Coloring;
using HELIX.Widgets;
using HELIX.Widgets.Prompts;
using HELIX.Widgets.Scrolling;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
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

    public override Widget Build(BuildContext context) {
      var prompts = new KennyKeyboardMousePromptProvider();
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
          BuildPromptsFor(context, new KennyKeyboardMousePromptProvider()),
          BuildPromptsFor(context, new KennyPlaystationSeriesPromptProvider()),
          BuildPromptsFor(context, new KennyXboxSeriesPromptProvider()),
          BuildPromptsFor(context, new KennySteamControllerPromptProvider()),
          BuildPromptsFor(context, new KennySteamDeckPromptProvider()),
          BuildPromptsFor(context, new KennyNintendoSwitch2PromptProvider())
        }.Fill()
      };
    }

    public Widget BuildPromptsFor(BuildContext context, KennyResourcePromptProvider provider) {
      var variant = KennyVariant.None;
      if (_isOutline) variant |= KennyVariant.Outline;
      if (_isAlternative) variant |= KennyVariant.Alternative;
      if (_isColored) variant |= KennyVariant.Color;
      if (_isIcon) variant |= KennyVariant.Icon;
      provider.Variant = variant;

      var widgets = provider.Mapping.Keys.Select<string, Widget>(x => {
          if (provider.TryResolvePrompt(context, x, out var layers)) {
            return new HSubstanceBox(substances: layers).Size(32, 32);
          }

          return new HText($"{x} (not found)");
        }
      ).ToArray();
      return new HBox(background: Colors.Grey) {
        new HColumn(crossAxisAlign: Align.Stretch) {
          new HText(provider.GetType().Name).Heading(context),
          new HFlex(Axis.Horizontal, wrap: true, children: widgets).Fill()
        }
      };
    }
  }
}
using HELIX.Coloring.Material;
using HELIX.Widgets;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Controllers;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Substances;
using UnityEngine.UIElements;

namespace Examples {
  public class StateSubstanceExample : StatefulWidget<StateSubstanceExample> {
    public override State<StateSubstanceExample> CreateState() {
      return new StateSubstanceExampleState();
    }
  }

  public class StateSubstanceExampleState : State<StateSubstanceExample> {
    private readonly SubstanceLayers _boxLayers = new Substance[] {
      new BoxSubstance {
        background = new WidgetStatePropertyMap<BackgroundStyle> {
          [WidgetState.Pressed] = new BackgroundStyle { color = MaterialColors.Red },
          [WidgetState.None] = new BackgroundStyle { color = MaterialColors.Blue }
        }
      },
      new BoxSubstance {
        opacity = new WidgetStatePropertyMap<float> {
          [WidgetState.Hovered] = 0.75f,
          [WidgetState.Focused] = 0.25f,
          [WidgetState.None] = 0f
        },
        background = new BackgroundStyle { color = MaterialColors.Green }
      }
    };

    private ButtonController _buttonController;
    private int _clicks;
    private WidgetStateController _stateController;

    public override void InitState() {
      base.InitState();
      _stateController = AddDisposable(new WidgetStateController());
      _buttonController = AddDisposable(
        new ButtonController(_stateController) {
          onClick = () => {
            _clicks++;
            SetState();
          }
        }
      );
    }

    public override Widget Build(BuildContext context) {
      return new HColumn(gap: 16f, crossAxisAlign: Align.Stretch) {
        new HText("Shared state/controller mechanics").Heading(context),
        new HRow(gap: 8f) {
          new HButton(controller: _buttonController) {
            new HText("Primary Controlled")
          },
          new HButton(HButtonVariant.Outline, _buttonController) {
            new HText("Secondary Controlled")
          }
        },
        new HText($"Shared click count: {_clicks}").Body(context),
        new HSubstanceBox(_stateController, _boxLayers).Size(height: 72),
        new HText("Hover / focus / press the controlled buttons to drive this state").Caption(context)
      }.Margin(16);
    }
  }
}
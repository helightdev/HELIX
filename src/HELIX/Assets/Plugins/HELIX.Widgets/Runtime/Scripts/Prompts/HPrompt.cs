using System.Collections.Generic;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Theme;
using UnityEngine.InputSystem;

namespace HELIX.Widgets.Prompts {
  public class HPrompt : StatefulWidget<HPrompt> {
    private readonly IPromptProvider _provider;
    private readonly InputAction _action;

    public HPrompt(
      InputAction action,
      IPromptProvider provider = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      _provider = provider;
      _action = action;
    }

    public HPrompt(
      string actionName,
      IPromptProvider provider = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      _provider = provider;
      _action = InputSystem.actions.FindAction(actionName);
    }

    public override State<HPrompt> CreateState() {
      return new HPromptState();
    }

    private class HPromptState : State<HPrompt> {
      public override Widget Build(BuildContext context) {
        var configuration = HelixInputController.Instance.Value;

        var provider = widget._provider ?? PrimitiveTheme.PromptProvider.Get(context);
        var found = provider.TryResolvePrompt(context, widget._action, configuration, out var layers);

        return new HSubstanceBox(
          substances: layers
        ).Size(64, 64);
      }
    }
  }
}
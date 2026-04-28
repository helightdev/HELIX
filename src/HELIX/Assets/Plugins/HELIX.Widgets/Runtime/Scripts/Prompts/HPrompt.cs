using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal;
using HELIX.Widgets.Universal.Styles;
using HELIX.Widgets.Universal.Theme;
using UnityEngine.InputSystem;

namespace HELIX.Widgets.Prompts {
  public class HPrompt : StatefulWidget<HPrompt> {
    private readonly InputAction _action;
    private readonly HPromptStyle _style;

    public HPrompt(
      InputAction action,
      HPromptStyle style = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      _style = style;
      _action = action;
    }

    public HPrompt(
      string actionName,
      HPromptStyle style = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      _style = style;
      _action = InputSystem.actions.FindAction(actionName);
    }

    public override State<HPrompt> CreateState() {
      return new HPromptState();
    }

    private class HPromptState : State<HPrompt> {
      public override Widget Build(BuildContext context) {
        var configuration = HelixInputController.Instance.Value;

        var style = widget._style ?? PrimitiveTheme.Prompt.Get(context);
        var found = style.provider.TryResolvePrompt(context, widget._action, configuration, out var layers);

        var modifiers = WidgetStateProperties.Func(state => new ModifierSet {
            new SizeModifier(style.constraints.ResolveOrDefault(state, BoxConstraints.Initial)),
          }
        );

        return new HSubstanceBox(
          substances: layers,
          boxModifiers: modifiers
        );
      }
    }
  }
}
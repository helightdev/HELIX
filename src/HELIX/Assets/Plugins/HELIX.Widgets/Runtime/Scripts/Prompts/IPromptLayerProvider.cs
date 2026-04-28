using HELIX.Widgets.Universal;
using UnityEngine.InputSystem;

namespace HELIX.Widgets.Prompts {
  public interface IPromptLayerProvider {
    bool TryResolvePromptLayer(BuildContext context, string bindingPath, out SubstanceLayers layers);
  }

  public interface IPromptProvider {
    bool TryResolvePrompt(
      BuildContext context,
      InputAction action,
      InputConfiguration configuration,
      out SubstanceLayers layers
    );
  }
}
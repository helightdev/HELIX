using HELIX.Context;
using HELIX.UI;
using HELIX.UI.DebugOverlay;
using UnityEngine;

namespace HELIX.Boot {
  /// <summary>Owns per-frame collection of HELIX screen-space and world-space debug overlays.</summary>
  [Managed(typeof(ApplicationScope))]
  public partial class DebugOverlayService : MonoBehaviour {
    [Inject] private GuiService _gui;

    [Ticker]
    private void CollectOverlays() {
      var overlay = _gui.debugOverlay;
      overlay.BeginFrame();
      new CollectScreenOverlayEvent(overlay).Raise();
      overlay.SetCamera(_gui.cameraProvider.GetGuiReferenceCamera());
      new CollectWorldOverlayEvent(overlay).Raise();
    }
  }
}

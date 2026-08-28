using HELIX.Context;
using HELIX.UI;
using HELIX.UI.CameraOverlays;
using UnityEngine;

namespace HELIX.Boot {
  /// <summary>Owns per-frame collection of HELIX camera overlays.</summary>
  [Managed(typeof(ApplicationScope))]
  public partial class CameraOverlayService : MonoBehaviour {
    [Inject] private GuiService _gui;

    [Ticker]
    private void CollectOverlays() {
      var overlay = _gui.cameraOverlays;
      overlay.SetCamera(_gui.cameraProvider.GetGuiReferenceCamera());
      new CollectCameraOverlayEvent(overlay).Raise();
    }
  }
}

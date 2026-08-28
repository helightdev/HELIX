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
      var screen = _gui.screenCameraOverlays;
      var world = _gui.worldCameraOverlays;
      world.SetCamera(_gui.cameraProvider.GetGuiReferenceCamera());
      new CollectScreenCameraOverlayEvent(screen).Raise();
      new CollectWorldCameraOverlayEvent(world).Raise();
    }
  }
}

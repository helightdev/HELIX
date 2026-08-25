using HELIX.Context;
using HELIX.UI.DebugOverlay;
using UnityEngine;

/// <summary>Live examples for the per-frame screen and world overlay collection events.</summary>
[Managed(typeof(ApplicationScope))]
public partial class ExampleOverlays : MonoBehaviour {
  [EventHandler]
  private void OnCollectScreenOverlay(CollectScreenOverlayEvent evt) {
    evt.overlay.Section("HELIX", order: -100, subtitle: "Per-frame screen overlay")
      .Field("Frame", Time.frameCount)
      .Field("Time", Time.unscaledTime, 2, "s")
      .Field("Time scale", Time.timeScale, 2);

    var camera = Camera.main;
    if (camera) evt.overlay.Section("Camera", subtitle: camera.name)
      .Field("Position", camera.transform.position, 1, "m")
      .Field("FOV", camera.fieldOfView, 1, "°", new Color(.55f, .85f, 1f));
  }

  [EventHandler]
  private void OnCollectWorldOverlay(CollectWorldOverlayEvent evt) {
    var camera = Camera.main;
    if (!camera) return;
    var position = Vector3.zero;
    evt.overlay.Box("helix.overlay.example", "HELIX World", position, "Five metres ahead")
      .Field("Position", position, 1, "m")
      .Field("Distance", Vector3.Distance(camera.transform.position, position), 1, "m")
      .Field("Frame", Time.frameCount);
  }
}
using HELIX;
using HELIX.Context;
using HELIX.Compose;
using HELIX.Prose;
using HELIX.UI.CameraOverlays;
using UnityEngine;

/// <summary>Live examples for prose-authored screen and world camera-overlay boxes.</summary>
[Managed(typeof(ApplicationScope))]
public partial class ExampleCameraOverlays : MonoBehaviour {
  private Camera _namedCamera;
  private string _cameraName;

  [EventHandler]
  private void OnCollectCameraOverlay(CollectCameraOverlayEvent evt) {
    var prose = evt.prose;
    using (prose.Box("helix.overlay.example.screen", CameraOverlayPosition.Screen(10, 10, -100))) {
      prose.WriteSectionHeader("HELIX");
      prose.WriteParagraph("Per-frame camera overlay");
      prose.Property("Frame", Time.frameCount, Datatypes.Int);
      prose.Property("Time", Time.unscaledTime, Datatypes.Float);
      prose.Property("Time scale", Time.timeScale, Datatypes.Float);
      prose.Content(ExtraContent);
    }

    var camera = Camera.main;
    if (!camera) return;
    if (!object.ReferenceEquals(_namedCamera, camera)) {
      _namedCamera = camera;
      _cameraName = camera.name;
    }
    using (prose.Box("helix.overlay.example.camera", CameraOverlayPosition.Screen(10, 170))) {
      prose.WriteSectionHeader("Camera");
      prose.WriteParagraph(_cameraName);
      prose.Property("Position", camera.transform.position, Datatypes.Vector3);
      using (prose.Property()) {
        prose.Push(new CameraOverlayColor(new Color(.55f, .85f, 1f)));
        using (prose.PropertyKey()) prose.Write("FOV");
        using (prose.PropertyValue()) prose.Write(camera.fieldOfView, Datatypes.Float);
      }
    }

    var position = Vector3.zero;
    using (prose.Box("helix.overlay.example.world", CameraOverlayPosition.World(position))) {
      prose.WriteSectionHeader("HELIX World");
      prose.WriteParagraph("World-positioned box");
      prose.Property("Position", position, Datatypes.Vector3);
      prose.Property("Distance", Vector3.Distance(camera.transform.position, position), Datatypes.Float);
    }
  }

  private static void ExtraContent(ref Composition cx) => cx.Text("Arbitrary composable content");
}

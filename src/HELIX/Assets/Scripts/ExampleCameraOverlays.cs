using HELIX;
using HELIX.Context;
using HELIX.Compose;
using HELIX.Prose;
using HELIX.UI.CameraOverlays;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>Live examples for prose-authored screen and world camera-overlay boxes.</summary>
[Managed(typeof(ApplicationScope))]
public partial class ExampleCameraOverlays : MonoBehaviour {
  private ScreenCameraOverlayEntry _screen, _camera;
  private WorldCameraOverlayEntry _world;
  private Camera _namedCamera;
  private string _cameraName;

  [FormerlySerializedAs("enabledCameraOverlay")] public bool enabledCamera = true;

  [Hook]
  private void OnDispose() {
    _screen?.Dispose();
    _camera?.Dispose();
    _world?.Dispose();
  }

  [EventHandler]
  private void OnCollectScreenCameraOverlay(CollectScreenCameraOverlayEvent evt) {
    if (_screen == null) {
      _screen = evt.overlays.Subscribe(
        "helix.overlay.example.screen", new ScreenCameraOverlayPosition(10, 10, -100), WriteScreen
      );
      _camera = evt.overlays.Subscribe(
        "helix.overlay.example.camera", new ScreenCameraOverlayPosition(10, 170), WriteCamera, HasCamera
      );
    }

    var camera = Camera.main;
    if (camera && !object.ReferenceEquals(_namedCamera, camera)) {
      _namedCamera = camera;
      _cameraName = camera.name;
    }
  }

  [EventHandler]
  private void OnCollectWorldCameraOverlay(CollectWorldCameraOverlayEvent evt) {
    if (_world != null) return;
    _world = evt.overlays.Subscribe(
      "helix.overlay.example.world", Vector3.zero, WriteWorld, HasCamera
    );
    _world.Track(transform, Vector3.up * 2f);
  }

  private bool HasCamera() => _namedCamera && enabledCamera;

  private void WriteScreen(IProseWriter prose) {
    prose.WriteSectionHeader("HELIX");
    prose.WriteParagraph("Per-frame camera overlay");
    prose.Property("Frame", Time.frameCount, Datatypes.Int);
    prose.Property("Time", Time.unscaledTime, Datatypes.Float);
    prose.Property("Time scale", Time.timeScale, Datatypes.Float);
    prose.Content(ExtraContent);
  }

  private void WriteCamera(IProseWriter prose) {
    var camera = _namedCamera;
    prose.WriteSectionHeader("Camera");
    prose.WriteParagraph(_cameraName);
    prose.Property("Position", camera.transform.position, Datatypes.Vector3);
    using (prose.Property()) {
      prose.Push(new CameraOverlayColor(new Color(.55f, .85f, 1f)));
      using (prose.PropertyKey()) prose.Write("FOV");
      using (prose.PropertyValue()) prose.Write(camera.fieldOfView, Datatypes.Float);
    }
  }

  private void WriteWorld(IProseWriter prose) {
    var position = _world.ResolvedPosition;
    prose.WriteSectionHeader("HELIX World");
    prose.WriteParagraph("Transform-tracked box");
    prose.Property("Position", position, Datatypes.Vector3);
    prose.Property("Distance", Vector3.Distance(_namedCamera.transform.position, position), Datatypes.Float);
  }

  [ComposableDelegate]
  private static void _ExtraContent(ref Composition cx) => cx.Text("Arbitrary composable content");
}

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Tests {
  public sealed class FormSystemTestWindow : EditorWindow {
    private WidgetHostElement _host;

    private void CreateGUI() {
      _host = new WidgetHostElement {
        Buildable = new FormSmokeWidget().ToBuildable()
      };
      _host.style.flexGrow = 1;
      rootVisualElement.Add(_host);
    }

    private void OnDisable() {
      _host?.Dispose();
      _host = null;
    }

    [MenuItem("Window/HELIX/Tests/Form System Smoke Test", false, 1100)]
    private static void ShowWindow() {
      var window = GetWindow<FormSystemTestWindow>();
      window.titleContent = new GUIContent("Form System Smoke Test");
      window.Show();
    }
  }
}
#endif

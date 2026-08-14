using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX {

  [RequireComponent(typeof(PanelRenderer))]
  [ExecuteAlways]
  public class HelixPanel : MonoBehaviour {
    public HelixGUI gui;

    private PanelRenderer _panelRenderer;
    private VisualElement _rootElement;
    private HelixGUI _current;

    public void Awake() {
      _panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable() {
      _panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable() {
      _panelRenderer.UnregisterUIReloadCallback(OnUIReload);
      DestroyGui();
    }

    private void OnDestroy() {
      DestroyGui();
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement, int version) {
      _rootElement = rootElement;
      Recreate();
    }

    private void Recreate() {
      if (_rootElement == null) {
        Debug.LogWarning($"Root element is null, cannot recreate ui");
        return;
      }
      DestroyGui();
      if (!gui) {
        Debug.LogWarning($"No gui assigned, cannot recreate ui");
        return;
      }

      _rootElement.Clear();
      _current = gui;
      _current.CreateGui(this);
    }

    private void DestroyGui() {
      if (_current == null) return;
      _current.DestroyGui(this);
      _current = null;
    }
  }

  public abstract class HelixGUI : ScriptableObject {
    public abstract void CreateGui(HelixPanel panel);
    public virtual void DestroyGui(HelixPanel panel) {}
  }
}
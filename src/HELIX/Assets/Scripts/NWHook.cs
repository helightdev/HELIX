using System;
using HELIX.Examples;
using UnityEngine;
using UnityEngine.UIElements;

namespace DefaultNamespace {
  [RequireComponent(typeof(PanelRenderer))]
  [ExecuteAlways]
  public class NWHook : MonoBehaviour {

    private PanelRenderer _panelRenderer;
    private VisualElement _rootElement;

    public void Awake() {
      _panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable() {
      _panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable() {
      _panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement, int version) {
      _rootElement = rootElement;
      Debug.Log($"Reloaded ui with version {version}");
      Recreate();
    }

    private void Recreate() {
      if (_rootElement == null) {
        Debug.LogWarning($"Root element is null, cannot recreate ui");
        return;
      }
      _rootElement.Clear();
      _rootElement.Add(new NwSystemExampleElement());
      Debug.Log($"Recreate ui");
    }
  }
}
using System;
using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Context;
using HELIX.Extensions;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace HELIX.UI {
  [Managed(typeof(ApplicationScope))]
  public partial class HelixPanel {
    [Resource(Source.Resources, "Settings/PanelSettings.asset", false)]
    public PanelSettings settings;

    private PanelRenderer _renderer;
    private GameObject _panelBackingObject;
    private VisualElement _rootElement;

    public HelixGuiHost host;

    [Hook]
    public void OnInit() {
      if (!settings) {
        Debug.LogWarning($"Panel Settings not found, loading defaults");
        settings = ScriptableObject.CreateInstance<PanelSettings>();
      }

      _panelBackingObject = new GameObject("Helix Panel");
      Object.DontDestroyOnLoad(_panelBackingObject);
      _renderer = _panelBackingObject.AddComponent<PanelRenderer>();
      _renderer.RegisterUIReloadCallback(OnUIReload);
      _renderer.panelSettings = settings;

      host = new HelixGuiHost();
    }

    [Hook]
    public void OnDispose() {
      if (!_renderer) return;
      _renderer.UnregisterUIReloadCallback(OnUIReload);
      DestroyGui();
      Object.Destroy(_renderer.gameObject);
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
      _rootElement.Clear();
      _rootElement.Add(host);
    }

    private void DestroyGui() {
      host.Dispose();
      host = null;
    }
  }

  [UxmlElement(visibility = LibraryVisibility.Hidden)]
  public partial class HelixGuiHost : BoundaryVisualElement {
    public NavigationController navigation = new();
    public OverlayController overlays = new();
    public NavigationGraph graph = NavigationGraph.Builder("/home").Build();

    public HelixGuiHost() {
      this.Fill();
    }

    public override void Compose(ref Composition cx) {
      using (cx.OverlayHost(overlays).With(Flex.Fill())) {
        cx.NavigationHost(graph, navigation).With(Flex.Fill());
      }
    }

    public override void Dispose() {
      base.Dispose();
    }
  }

  public abstract class HelixGUI : ScriptableObject {
    public abstract void CreateGui(HelixPanel panel);
    public virtual void DestroyGui(HelixPanel panel) { }
  }
}
using HELIX.Compose;
using HELIX.Context;
using HELIX.Extensions;
using HELIX.Theming;
using HELIX.UI.Prompts;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace HELIX.UI {
  [Managed(typeof(ApplicationScope))]
  public partial class GuiService {
    [Resource(Source.Resources, "Settings/PanelSettings.asset", false)]
    public PanelSettings settings;

    [Inject(required: false)]
    public ThemeData theme;

    private PanelRenderer _renderer;
    private GameObject _panelBackingObject;
    private VisualElement _rootElement;

    public HXGuiHost host;

    public IGuiCameraProvider cameraProvider = new MainGuiCameraProvider();
    public readonly NavigationController navigation = new();
    public readonly OverlayController overlays = new();
    public readonly HelixInputController inputController = new(InputConfiguration.Default);

    [Hook]
    public void OnInit() {
      if (!settings) {
        Debug.LogWarning($"Panel Settings not found, loading defaults");
        settings = ScriptableObject.CreateInstance<PanelSettings>();
      }

      if (theme == null) {
        Debug.LogWarning($"ThemeData not found, loading defaults");
        theme = HXThemes.DefaultDark;
      }

      _panelBackingObject = new GameObject("Helix Panel");
      Object.DontDestroyOnLoad(_panelBackingObject);
      _renderer = _panelBackingObject.AddComponent<PanelRenderer>();
      _renderer.RegisterUIReloadCallback(OnUIReload);
      _renderer.panelSettings = settings;
    }

    [Hook]
    public void OnDispose() {
      if (!_renderer) return;
      _renderer.UnregisterUIReloadCallback(OnUIReload);
      host?.Dispose();
      host = null;
      Object.Destroy(_renderer.gameObject);
    }

    [EventHandler]
    public void OnScopeActivate(ScopeActivateEvent evt) {
      if (evt.Scope.scope is not ApplicationScope) return;
      RebuildNavigation();
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
      RebuildNavigation();

      _rootElement.Clear();
      _rootElement.Stretched();
      host = new HXGuiHost(this);
      _rootElement.Add(host);
      Debug.Log($"Recreated Helix UI in root element: {_rootElement.name}");
    }

    public void RebuildNavigation() {
      var evt = new BuildNavigationEvent(this, NavigationGraph.Builder("/"));
      Evt.Raise(ref evt);
      if (!evt.builder.IsInitialRouteSet) {
        evt.builder.Route(
          evt.builder.InitialRoutePath,
          NavigationPage.Build(static (ref Composition cx, NavigationContextData value) => {
              cx.Text("Missing default navigation route.");
            }
          ).Build()
        );
      }
      navigation.SetGraph(evt.builder.Build(), evt.preserveStack);
    }
  }

  public interface IGuiCameraProvider {
    public Camera GetGuiReferenceCamera();
  }

  public class MainGuiCameraProvider : IGuiCameraProvider {
    public Camera GetGuiReferenceCamera() => Camera.main;
  }

  public struct BuildNavigationEvent : Evt<BuildNavigationEvent> {
    public readonly GuiService panel;
    public readonly NavigationGraphBuilder builder;
    public readonly bool preserveStack;

    public BuildNavigationEvent(GuiService panel, NavigationGraphBuilder builder) {
      this.panel = panel;
      this.builder = builder;
      preserveStack = true;
    }
  }
}
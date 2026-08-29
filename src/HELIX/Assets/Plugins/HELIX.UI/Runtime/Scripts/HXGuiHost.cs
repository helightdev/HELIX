using HELIX.Compose;
using HELIX.UI.Prompts;
using HELIX.UI.Console;
using HELIX.UI.CameraOverlays;
using UnityEngine.UIElements;

namespace HELIX.UI {
  [UxmlElement(visibility = LibraryVisibility.Hidden)]
  [Mixable]
  [CustomBoundaryElement(constructor: false)]
  public partial class HXGuiHost : VisualElement {
    public GuiService service;
    private CommandConsoleElement _console;

    public HXGuiHost(GuiService service) : this() {
      this.service = service;
    }

    public HXGuiHost() {
      this.Stretched();
      RegisterCallback<AttachToPanelEvent>(_ => AttachBoundary());
      RegisterCallback<DetachFromPanelEvent>(_ => DetachBoundary());
      PostConstruct();
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      using (cx.WriteContext(out var context)) {
        ThemeData.Key[context] = service.theme;
        HelixInputController.Key[context] = service.inputController;

        ref var textStyle = ref service.theme.body.medium.style;
        TextStyle.Key[context] = textStyle;
        textStyle.Apply(this);
      }

      using (cx.OverlayHost(service.overlays).With(Flex.Fill())) {
        cx.NavigationHost(service.navigation).With(Flex.Fill());
      }

      CameraOverlayComposition.Compose(ref cx, service.screenCameraOverlays, service.worldCameraOverlays);
      _console = CameraOverlayComposition.ComposeConsole(ref cx, service.commandSystem);
    }

    public void ToggleCommandConsole() => _console?.Toggle();
  }

  internal static class CameraOverlayComposition {
    private static readonly ushort OverlayId = CompositionId.GetTypeId(nameof(CameraOverlayElement));
    private static readonly ushort ConsoleId = CompositionId.GetTypeId(nameof(CommandConsoleElement));

    public static void Compose(
      ref Composition cx, ScreenCameraOverlayController screen, WorldCameraOverlayController world
    ) {
      if (!cx.AUTHORING.RequireTracked<CameraOverlayElement>(OverlayId, out var element, out _))
        element = new CameraOverlayElement(screen, world);
      cx.AUTHORING.YieldElement(ref cx, element);
    }

    public static CommandConsoleElement ComposeConsole(ref Composition cx, ICommandSystem system) {
      if (!cx.AUTHORING.RequireComposable<CommandConsoleElement>(ConsoleId, out var element, out _))
        element = new CommandConsoleElement { PackedId = cx.AUTHORING.id.packed };
      element.Bind(system);
      cx.AUTHORING.YieldBoundary(ref cx, element);
      return element;
    }
  }
}

using HELIX.Compose;
using HELIX.Extensions;
using HELIX.Theming;
using HELIX.Types;
using HELIX.UI.Prompts;
using HELIX.UI.Console;
using HELIX.UI.DebugOverlay;
using UnityEngine.UIElements;

namespace HELIX.UI {
  [UxmlElement(visibility = LibraryVisibility.Hidden)]
  [EnableMixins]
  [CustomBoundaryElement(constructor: false)]
  public partial class HXGuiHost : VisualElement {
    public GuiService panel;
    private CommandConsoleElement _console;

    public HXGuiHost(GuiService panel) : this() {
      this.panel = panel;
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
        ThemeData.Key[context] = panel.theme;
        HelixInputController.Key[context] = panel.inputController;

        ref var textStyle = ref panel.theme.body.medium.style;
        TextStyle.Key[context] = textStyle;
        textStyle.Apply(this);
      }

      using (cx.OverlayHost(panel.overlays).With(Flex.Fill())) {
        cx.NavigationHost(panel.navigation).With(Flex.Fill());
      }

      DebugOverlayComposition.Compose(ref cx, panel.debugOverlay);
      _console = DebugOverlayComposition.ComposeConsole(ref cx, panel.commandSystem);
    }

    public void ToggleCommandConsole() => _console?.Toggle();
  }

  internal static class DebugOverlayComposition {
    private static readonly ushort OverlayId = CompositionId.GetTypeId(nameof(DebugOverlayElement));
    private static readonly ushort ConsoleId = CompositionId.GetTypeId(nameof(CommandConsoleElement));

    public static void Compose(ref Composition cx, DebugOverlayController controller) {
      if (!cx.AUTHORING.RequireTracked<DebugOverlayElement>(OverlayId, out var element, out _))
        element = new DebugOverlayElement(controller);
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

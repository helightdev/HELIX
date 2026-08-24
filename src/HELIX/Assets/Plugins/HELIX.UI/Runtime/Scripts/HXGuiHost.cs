using HELIX.Compose;
using HELIX.Extensions;
using HELIX.Theming;
using HELIX.Types;
using HELIX.UI.Prompts;
using UnityEngine.UIElements;

namespace HELIX.UI {
  [UxmlElement(visibility = LibraryVisibility.Hidden)]
  public partial class HXGuiHost : BoundaryVisualElement {
    public GuiService panel;

    public HXGuiHost(GuiService panel) {
      this.panel = panel;
      this.Stretched();
    }

    public HXGuiHost() {
      this.Stretched();
    }

    public override void Compose(ref Composition cx) {
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
    }
  }
}
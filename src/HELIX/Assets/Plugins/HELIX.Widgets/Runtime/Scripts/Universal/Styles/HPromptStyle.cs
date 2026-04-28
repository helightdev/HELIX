using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Prompts;

namespace HELIX.Widgets.Universal.Styles {
  public class HPromptStyle : DiagnosticableBase {
    public IPromptProvider provider;
    public WidgetStateProperty<BoxConstraints> constraints = WidgetStateProperties.Never<BoxConstraints>();

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new DiagnosticsProperty<object>("provider", provider));
      properties.Add(new DiagnosticsProperty<object>("constraints", constraints));
    }
  }
}
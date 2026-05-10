using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Error {
  public class ErrorDescription : DiagnosticsProperty<string> {
    public ErrorDescription(string message)
      : base(
        null,
        message,
        showName: false,
        style: DiagnosticsTreeStyle.Flat,
        level: DiagnosticLevel.Info
      ) { }
  }
}
using HELIX.Diagnostics.Formatting;

namespace HELIX.Diagnostics.Error {
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
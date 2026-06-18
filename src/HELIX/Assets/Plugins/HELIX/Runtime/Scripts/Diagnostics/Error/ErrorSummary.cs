using HELIX.Diagnostics.Formatting;

namespace HELIX.Diagnostics.Error {
  public sealed class ErrorSummary : DiagnosticsProperty<string> {
    public ErrorSummary(string message)
      : base(
        null,
        message,
        showName: false,
        style: DiagnosticsTreeStyle.SingleLine,
        level: DiagnosticLevel.Summary
      ) { }
  }
}
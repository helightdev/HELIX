using HELIX.Diagnostics.Formatting;

namespace HELIX.Diagnostics.Error {
  public sealed class ErrorHint : DiagnosticsProperty<string> {
    public ErrorHint(string message)
      : base(
        null,
        message,
        showName: false,
        style: DiagnosticsTreeStyle.Flat,
        level: DiagnosticLevel.Hint
      ) { }
  }
}
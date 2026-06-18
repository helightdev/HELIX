using HELIX.Diagnostics.Formatting;

namespace HELIX.Diagnostics.Error {
  public sealed class ErrorSpacer : DiagnosticsProperty<string> {
    public ErrorSpacer()
      : base(
        null,
        string.Empty,
        showName: false,
        style: DiagnosticsTreeStyle.Flat,
        level: DiagnosticLevel.Info
      ) { }

    public override string ToDescription(TextTreeConfiguration parentConfiguration = null) {
      return string.Empty;
    }
  }
}
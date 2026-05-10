using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Error {
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
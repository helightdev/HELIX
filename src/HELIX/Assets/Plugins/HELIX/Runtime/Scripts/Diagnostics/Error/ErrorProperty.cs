using HELIX.Diagnostics.Formatting;

namespace HELIX.Diagnostics.Error {
  public sealed class ErrorProperty : DiagnosticsProperty<object> {
    public ErrorProperty(string name, object value)
      : base(
        name,
        value,
        style: DiagnosticsTreeStyle.ErrorProperty,
        level: DiagnosticLevel.Info
      ) { }
  }
}
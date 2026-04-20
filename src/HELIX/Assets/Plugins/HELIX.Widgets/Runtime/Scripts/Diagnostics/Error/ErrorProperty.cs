using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Error {
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
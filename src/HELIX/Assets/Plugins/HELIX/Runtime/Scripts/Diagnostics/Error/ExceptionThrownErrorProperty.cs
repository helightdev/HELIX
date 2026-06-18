using System;
using HELIX.Diagnostics.Formatting;

namespace HELIX.Diagnostics.Error {
  public class ExceptionThrownErrorProperty : DiagnosticsProperty<Exception> {
    public ExceptionThrownErrorProperty(Exception exception) : base(
      "The following exception was thrown",
      exception,
      style: DiagnosticsTreeStyle.ErrorProperty,
      level: DiagnosticLevel.Info
    ) { }

    public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
      return ValueTyped != null ? ValueTyped.ToString() : "None";
    }
  }
}
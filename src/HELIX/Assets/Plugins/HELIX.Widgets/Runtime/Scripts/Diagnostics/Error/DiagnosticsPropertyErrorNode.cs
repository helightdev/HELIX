using System;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Error {
    public sealed class DiagnosticsPropertyErrorNode : DiagnosticsProperty<string> {

        public DiagnosticsPropertyErrorNode(string name, Exception exception)
            : base(
                name,
                exception == null ? "EXCEPTION" : $"EXCEPTION ({exception.GetType().Name})",
                style: DiagnosticsTreeStyle.ErrorProperty,
                level: DiagnosticLevel.Error
            ) {
            ExceptionObject = exception;
        }

        public Exception ExceptionObject { get; }

    }
}
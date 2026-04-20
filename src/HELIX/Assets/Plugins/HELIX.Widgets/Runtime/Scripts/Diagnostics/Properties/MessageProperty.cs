using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Properties {
    public sealed class MessageProperty : DiagnosticsProperty<object> {

        public MessageProperty(
            string name,
            string message,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine,
            DiagnosticLevel level = DiagnosticLevel.Info
        )
            : base(name, null, message, style: style, level: level) { }

    }
}
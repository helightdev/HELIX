using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Properties {
    public sealed class KeyProperty : DiagnosticsProperty<Key> {

        public KeyProperty(
            Key value,
            string name = "key",
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine,
            DiagnosticLevel level = DiagnosticLevel.Info
        ) : base(name, value, style: style, level: level) { }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            return ValueTyped.ToString();
        }

    }
}
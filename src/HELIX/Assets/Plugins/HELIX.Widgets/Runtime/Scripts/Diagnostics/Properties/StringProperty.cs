using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Properties {
    public class StringProperty : DiagnosticsProperty<string> {

        public StringProperty(
            string name,
            string value,
            string description = null,
            string tooltip = null,
            bool showName = true,
            object defaultValue = null,
            bool quoted = true,
            string ifEmpty = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine,
            DiagnosticLevel level = DiagnosticLevel.Info
        )
            : base(
                name,
                value,
                description,
                null,
                ifEmpty,
                showName,
                true,
                defaultValue,
                tooltip,
                false,
                null,
                false,
                true,
                true,
                style,
                level
            ) {
            Quoted = quoted;
        }

        public bool Quoted { get; }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            var text = ToDescriptionSource();
            if (parentConfiguration != null && !parentConfiguration.LineBreakProperties && text != null)
                text = text.Replace("\n", "\\n");

            if (Quoted && text != null) {
                if (IfEmpty != null && text.Length == 0) return IfEmpty;
                return "\"" + text + "\"";
            }

            return text ?? "null";
        }

        private string ToDescriptionSource() {
            return ValueTyped;
        }

    }
}
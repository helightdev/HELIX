using System.Globalization;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Properties {
    public sealed class IntProperty : DiagnosticsProperty<int?> {
        public IntProperty(
            string name,
            int? value,
            string ifNull = null,
            string unit = null,
            bool showName = true,
            object defaultValue = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine,
            DiagnosticLevel level = DiagnosticLevel.Info
        )
            : base(
                name,
                value,
                null,
                ifNull,
                null,
                showName,
                true,
                defaultValue,
                null,
                false,
                null,
                false,
                true,
                true,
                style,
                level
            ) {
            Unit = unit;
        }

        public string Unit { get; }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            if (!ValueTyped.HasValue) return "null";
            return Unit != null
                ? ValueTyped.Value.ToString(CultureInfo.InvariantCulture) + Unit
                : ValueTyped.Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
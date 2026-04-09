using System;
using System.Globalization;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Properties {
    public sealed class PercentProperty : DiagnosticsProperty<double?> {
        public PercentProperty(
            string name,
            double? fraction,
            string ifNull = null,
            bool showName = true,
            string tooltip = null,
            string unit = null,
            DiagnosticLevel level = DiagnosticLevel.Info
        )
            : base(
                name,
                fraction,
                null,
                ifNull,
                null,
                showName,
                true,
                null,
                tooltip,
                false,
                null,
                false,
                true,
                true,
                DiagnosticsTreeStyle.SingleLine,
                level
            ) {
            Unit = unit;
        }

        public string Unit { get; }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            if (!ValueTyped.HasValue) return "null";
            var v = NumberToString();
            return Unit != null ? v + " " + Unit : v;
        }

        private string NumberToString() {
            var clamped = Math.Max(0.0, Math.Min(1.0, ValueTyped.Value));
            return (clamped * 100.0).ToString("0.0", CultureInfo.InvariantCulture) + "%";
        }
    }
}
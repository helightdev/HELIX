using System;
using System.Globalization;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Properties {
    public sealed class DoubleProperty : DiagnosticsProperty<double?> {
        public DoubleProperty(
            string name,
            double? value,
            string ifNull = null,
            string unit = null,
            string tooltip = null,
            object defaultValue = null,
            bool showName = true,
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
                tooltip,
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
            var n = DebugFormatDouble(ValueTyped.Value);
            return Unit != null ? n + Unit : n;
        }

        internal static string DebugFormatDouble(double value) {
            if (double.IsNaN(value) || double.IsInfinity(value)) return value.ToString(CultureInfo.InvariantCulture);

            var rounded = Math.Round(value);
            if (Math.Abs(value - rounded) < 0.0000001) return ((long)rounded).ToString(CultureInfo.InvariantCulture);

            return value.ToString("0.0###############", CultureInfo.InvariantCulture);
        }
    }
}
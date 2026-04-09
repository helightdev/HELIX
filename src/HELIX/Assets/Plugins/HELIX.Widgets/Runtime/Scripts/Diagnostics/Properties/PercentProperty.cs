using System.Globalization;
using HELIX.Widgets.Diagnostics.Formatting;
using UnityEngine;

namespace HELIX.Widgets.Diagnostics.Properties {
    public sealed class PercentProperty : DiagnosticsProperty<float?> {
        public PercentProperty(
            string name,
            float? fraction,
            string ifNull = null,
            bool showName = true,
            string tooltip = null,
            string unit = null,
            object defaultValue = null,
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
                defaultValue,
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

        public override DiagnosticLevel Level {
            get {
                if (ValueTyped.HasValue && DefaultValue != null &&
                    Mathf.Approximately(ValueTyped.GetValueOrDefault(), (float)DefaultValue))
                    return DiagnosticLevel.Fine;
                return base.Level;
            }
        }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            if (!ValueTyped.HasValue) return "null";
            var v = NumberToString();
            return Unit != null ? v + " " + Unit : v;
        }

        private string NumberToString() {
            var clamped = Mathf.Max(0.0f, Mathf.Min(1.0f, ValueTyped.GetValueOrDefault()));
            return (clamped * 100.0).ToString("0.0", CultureInfo.InvariantCulture) + "%";
        }
    }
}
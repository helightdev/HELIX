using System.Globalization;
using HELIX.Widgets.Diagnostics.Formatting;
using Unity.Mathematics;
using UnityEngine;

namespace HELIX.Widgets.Diagnostics.Properties {
    public sealed class FloatProperty : DiagnosticsProperty<float?> {

        public FloatProperty(
            string name,
            float? value,
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

        public override DiagnosticLevel Level {
            get {
                if (DefaultValue != null && ValueTyped.HasValue &&
                    Mathf.Approximately((float)DefaultValue, ValueTyped.Value)) return DiagnosticLevel.Fine;

                return base.Level;
            }
        }

        public string Unit { get; }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            if (!ValueTyped.HasValue) return "null";
            var n = DebugFormatFloat(ValueTyped.Value);
            return Unit != null ? n + Unit : n;
        }

        internal static string DebugFormatFloat(float value) {
            if (float.IsNaN(value) || float.IsInfinity(value)) return value.ToString(CultureInfo.InvariantCulture);

            var rounded = math.round(value);
            if (Mathf.Approximately(value, rounded)) return ((int)rounded).ToString(CultureInfo.InvariantCulture);
            return value.ToString("0.0###############", CultureInfo.InvariantCulture);
        }

    }
}
using System;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Properties {
    public abstract class NumProperty<T> : DiagnosticsProperty<T> where T : struct, IFormattable {

        protected NumProperty(
            string name,
            T? value,
            string ifNull = null,
            string unit = null,
            bool showName = true,
            object defaultValue = null,
            string tooltip = null,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.SingleLine,
            DiagnosticLevel level = DiagnosticLevel.Info
        )
            : base(
                name,
                value.GetValueOrDefault(),
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
            HasValue = value.HasValue;
            Unit = unit;
        }

        protected bool HasValue { get; }
        public string Unit { get; }
        public abstract string NumberToString();

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            if (!HasValue) return "null";
            return Unit != null ? NumberToString() + Unit : NumberToString();
        }

    }
}
using System;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Properties {
    public sealed class ObjectFlagProperty<T> : DiagnosticsProperty<T> {
        public ObjectFlagProperty(
            string name,
            T value,
            string ifPresent = null,
            string ifNull = null,
            bool showName = false,
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
                null,
                null,
                false,
                null,
                false,
                true,
                true,
                DiagnosticsTreeStyle.SingleLine,
                level
            ) {
            IfPresent = ifPresent;
            if (ifPresent == null && ifNull == null)
                throw new ArgumentException("At least one of ifPresent or ifNull must be provided.");
        }

        public string IfPresent { get; }

        public override DiagnosticLevel Level {
            get {
                if (ValueTyped != null && IfPresent == null) return DiagnosticLevel.Hidden;
                if (ValueTyped == null && IfNull == null) return DiagnosticLevel.Hidden;
                return base.Level;
            }
        }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            if (ValueTyped != null) {
                if (IfPresent != null) return IfPresent;
            } else if (IfNull != null) return IfNull;

            return base.ValueToString(parentConfiguration);
        }
    }
}
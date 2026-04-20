using System;
using HELIX.Widgets.Diagnostics.Formatting;

namespace HELIX.Widgets.Diagnostics.Properties {
    public sealed class FlagProperty : DiagnosticsProperty<bool?> {

        public FlagProperty(
            string name,
            bool? value,
            string ifTrue = null,
            string ifFalse = null,
            bool showName = false,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.Info
        )
            : base(
                name,
                value,
                null,
                null,
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
                DiagnosticsTreeStyle.SingleLine,
                level
            ) {
            IfTrue = ifTrue;
            IfFalse = ifFalse;
            if (ifTrue == null && ifFalse == null)
                throw new ArgumentException("At least one of ifTrue or ifFalse must be provided.");
        }

        public string IfTrue { get; }
        public string IfFalse { get; }

        public override DiagnosticLevel Level {
            get {
                if (ValueTyped == true && IfTrue == null) return DiagnosticLevel.Hidden;
                if (ValueTyped == false && IfFalse == null) return DiagnosticLevel.Hidden;
                return base.Level;
            }
        }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            if (ValueTyped == true && IfTrue != null) return IfTrue;
            if (ValueTyped == false && IfFalse != null) return IfFalse;
            return base.ValueToString(parentConfiguration);
        }

    }
}
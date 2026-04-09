using System;
using HELIX.Widgets.Diagnostics.Formatting;
using JetBrains.Annotations;

namespace HELIX.Widgets.Diagnostics.Properties {
    public sealed class EnumProperty<T> : DiagnosticsProperty<T> where T : Enum {
        public EnumProperty(
            string name,
            [CanBeNull] T value,
            object defaultValue = null,
            DiagnosticLevel level = DiagnosticLevel.Info
        )
            : base(
                name,
                value,
                null,
                null,
                null,
                true,
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
            ) { }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            return ValueTyped?.ToString() ?? "null";
        }
    }
}
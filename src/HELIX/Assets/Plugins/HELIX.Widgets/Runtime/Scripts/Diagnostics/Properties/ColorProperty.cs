using HELIX.Coloring;
using HELIX.Widgets.Diagnostics.Formatting;
using UnityEngine;

namespace HELIX.Widgets.Diagnostics.Properties {
    public class ColorProperty : DiagnosticsProperty<Color> {
        public ColorProperty(
            string name,
            Color value,
            string ifNull = null,
            bool showName = true,
            object defaultValue = null,
            string tooltip = null,
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
            ) { }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            return ValueTyped.a == 0 ? "transparent" : ValueTyped.ToHex();
        }
    }
}
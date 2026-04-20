using HELIX.Widgets.Diagnostics.Formatting;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Diagnostics.Properties {
    public class StyleValueProperty<T> : DiagnosticsProperty<IStyleValue<T>> {

        public StyleValueProperty(
            string name,
            IStyleValue<T> value,
            string ifNull = null,
            string ifInitial = null,
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
            ) {
            IfInitial = ifInitial;
        }

        public string IfInitial { get; set; }

        public override DiagnosticLevel Level {
            get {
                if (ValueTyped != null) {
                    if (ValueTyped.keyword is StyleKeyword.Initial or StyleKeyword.Null && IfInitial == null)
                        return DiagnosticLevel.Hidden;
                } else if (IfNull == null) {
                    return DiagnosticLevel.Hidden;
                }

                return base.Level;
            }
        }

        public virtual DiagnosticsNode UnwrapStyleValueNode(T value) {
            return value switch {
                Length length => length.unit == LengthUnit.Percent
                    ? new PercentProperty("length", length.value / 100f, showName: false)
                    : new FloatProperty("length", length.value, showName: false, unit: "px"),
                Color color => new ColorProperty("color", color, showName: false),
                BackgroundSize s => new FormattingProperty<BackgroundSize>(
                    "value",
                    s,
                    x => "BackgroundSize" + x,
                    showName: false
                ),
                BackgroundRepeat r => new FormattingProperty<BackgroundRepeat>(
                    "value",
                    r,
                    x => "BackgroundRepeat" + x,
                    showName: false
                ),
                BackgroundPosition p => new FormattingProperty<BackgroundPosition>(
                    "value",
                    p,
                    x => "BackgroundPosition" + x,
                    showName: false
                ),
                float f => new FloatProperty("float", f, showName: false),
                _ => new DiagnosticsProperty<T>("value", value, showName: false)
            };
        }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            switch (ValueTyped.keyword) {
                case StyleKeyword.Null: return "<null>";
                case StyleKeyword.Auto: return "<auto>";
                case StyleKeyword.None: return "<none>";
                case StyleKeyword.Initial: return "<initial>";
                case StyleKeyword.Undefined:
                default:
                    return UnwrapStyleValueNode(ValueTyped.value)
                        .ToDescription(parentConfiguration);
            }
        }

    }
}
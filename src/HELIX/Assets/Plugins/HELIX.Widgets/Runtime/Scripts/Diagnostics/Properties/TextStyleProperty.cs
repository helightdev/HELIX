using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Diagnostics.Formatting;
using HELIX.Widgets.Universal.Styles;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Diagnostics.Properties
{
    public class TextStyleProperty : DiagnosticsProperty<TextStyle> {
        public TextStyleProperty(
            string name,
            TextStyle value,
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

        public override DiagnosticLevel Level {
            get {
                if (ValueTyped == null && IfNull == null) return DiagnosticLevel.Hidden;
                return base.Level;
            }
        }

        public override List<DiagnosticsNode> GetProperties() {
            var list = base.GetProperties();
            if (ValueTyped == null) return list;
            list.Add(new StyleValueProperty<Font>("font", ValueTyped.font));
            list.Add(new StyleValueProperty<Length>("fontSize", ValueTyped.fontSize));
            list.Add(new StyleValueProperty<Color>("color", ValueTyped.color));
            list.Add(new StyleValueProperty<TextAnchor>("align", ValueTyped.align));
            list.Add(new StyleValueProperty<FontStyle>("style", ValueTyped.style));
            list.Add(new StyleValueProperty<WhiteSpace>("wrap", ValueTyped.wrap));
            list.Add(new StyleValueProperty<Color>("outlineColor", ValueTyped.outlineColor));
            list.Add(new StyleValueProperty<float>("outlineWidth", ValueTyped.outlineWidth));
            list.Add(new StyleValueProperty<Length>("letterSpacing", ValueTyped.letterSpacing));
            list.Add(new StyleValueProperty<Length>("wordSpacing", ValueTyped.wordSpacing));
            list.Add(new StyleValueProperty<Length>("paragraphSpacing", ValueTyped.paragraphSpacing));
            list.Add(new StyleValueProperty<TextOverflow>("overflow", ValueTyped.overflow));
            list.Add(new StyleValueProperty<TextOverflowPosition>("overflowPosition", ValueTyped.overflowPosition));
            list.Add(new StyleValueProperty<TextShadow>("shadow", ValueTyped.shadow));
            list.Add(new StyleValueProperty<TextAutoSize>("autoSize", ValueTyped.autoSize));
            list.Add(new StyleValueProperty<TextGeneratorType>("generator", ValueTyped.generator));
            return list;
        }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            return "TextStyle";
        }
    }
}
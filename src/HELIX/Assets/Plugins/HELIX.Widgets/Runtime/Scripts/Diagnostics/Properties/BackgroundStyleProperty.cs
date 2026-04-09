using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Diagnostics.Formatting;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Diagnostics.Properties
{
    public class BackgroundStyleProperty : DiagnosticsProperty<BackgroundStyle> {
        public BackgroundStyleProperty(
            string name,
            BackgroundStyle value,
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
            list.Add(new StyleValueProperty<Color>("color", ValueTyped.color));
            list.Add(new StyleValueProperty<Background>("image", ValueTyped.image));
            list.Add(new StyleValueProperty<BackgroundSize>("fit", ValueTyped.fit));
            list.Add(new StyleValueProperty<BackgroundRepeat>("repeat", ValueTyped.repeat));
            list.Add(new StyleValueProperty<Color>("imageTintColor", ValueTyped.imageTintColor));
            list.Add(new StyleValueProperty<BackgroundPosition>("x", ValueTyped.x));
            list.Add(new StyleValueProperty<BackgroundPosition>("y", ValueTyped.y));
            list.Add(new StyleValueProperty<int4>("slice", ValueTyped.slice));
            list.Add(new StyleValueProperty<float>("sliceScale", ValueTyped.sliceScale));
            list.Add(new StyleValueProperty<SliceType>("sliceType", ValueTyped.sliceType));
            return list;
        }

        public override string ValueToString(TextTreeConfiguration parentConfiguration = null) {
            return "BackgroundStyle";
        }
    }
}
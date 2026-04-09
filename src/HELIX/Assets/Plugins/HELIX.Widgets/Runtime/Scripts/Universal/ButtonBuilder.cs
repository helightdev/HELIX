using System;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class ButtonBuilder : Widget {
        private static readonly FocusModifier _defaultFocus = new(true, PickingMode.Position, 0) { isFallback = true };
        public Alignment alignment = Alignment.Center;

        public BuildFunction<WidgetState> builder;
        public bool enabled = true;
        public Action onClick;
        public bool selected = false;

        public ButtonBuilder() {
            modifiers.Add(_defaultFocus);
        }

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new GenericButton());
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<Alignment>("alignment", alignment, defaultValue: Alignment.Center));
            properties.Add(new FlagProperty("enabled", enabled, ifTrue: "Enabled", ifFalse: "Disabled"));
            properties.Add(new FlagProperty("selected", selected, ifTrue: "Selected"));
        }
    }
}
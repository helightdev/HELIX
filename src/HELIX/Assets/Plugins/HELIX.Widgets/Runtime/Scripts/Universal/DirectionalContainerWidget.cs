using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public abstract class DirectionalContainerWidget : MultiChildWidget {
        public Align crossAxisAlign = Align.Center;
        public float gap = 0f;
        public Justify mainAxisAlign = Justify.FlexStart;
        public bool reverse = false;

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new EnumProperty<Align>("crossAxisAlign", crossAxisAlign, defaultValue: Align.Center));
            properties.Add(new EnumProperty<Justify>("mainAxisAlign", mainAxisAlign, defaultValue: Justify.FlexStart));
            properties.Add(new FloatProperty("gap", gap, defaultValue: 0f));
            properties.Add(new FlagProperty("reverse", reverse, ifTrue: "Reverse"));
        }
    }
}
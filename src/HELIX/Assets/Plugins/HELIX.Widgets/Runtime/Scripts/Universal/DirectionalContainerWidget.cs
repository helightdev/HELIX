using System.Collections.Generic;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public abstract class DirectionalContainerWidget : MultiChildWidget {
        public readonly Align crossAxisAlign;
        public readonly float gap;
        public readonly Justify mainAxisAlign;
        public readonly bool reverse;

        protected DirectionalContainerWidget(
            Justify mainAxisAlign = Justify.FlexStart,
            Align crossAxisAlign = Align.Center,
            float gap = 0f,
            bool reverse = false,
            IReadOnlyList<Widget> children = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(children, key, constants) {
            this.crossAxisAlign = crossAxisAlign;
            this.gap = gap;
            this.mainAxisAlign = mainAxisAlign;
            this.reverse = reverse;
            
            DefaultModifiers(ModifierSet.DefaultFlexFill, modifiers);
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new EnumProperty<Align>("crossAxisAlign", crossAxisAlign, defaultValue: Align.Center));
            properties.Add(new EnumProperty<Justify>("mainAxisAlign", mainAxisAlign, defaultValue: Justify.FlexStart));
            properties.Add(new FloatProperty("gap", gap, defaultValue: 0f));
            properties.Add(new FlagProperty("reverse", reverse, ifTrue: "Reverse"));
        }
    }
}
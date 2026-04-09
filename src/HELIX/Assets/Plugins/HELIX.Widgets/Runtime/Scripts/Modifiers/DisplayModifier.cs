using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class DisplayModifier : Modifier {
        public static readonly DisplayModifier Visible = new(true);
        public static readonly DisplayModifier Hidden = new(false);
        public readonly bool visible;

        public DisplayModifier(bool visible) {
            this.visible = visible;
        }

        public override void Apply(VisualElement element) {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public override void Reset(VisualElement element) {
            element.style.display = StyleKeyword.Initial;
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not DisplayModifier prev) return true;
            return visible != prev.visible;
        }

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            base.FillModifierProperties(properties);
            properties.Add(new FlagProperty("visible", visible, ifTrue: "Visible", ifFalse: "Hidden"));
        }

        protected override string FindConstantName() {
            return visible ? nameof(Visible) : nameof(Hidden);
        }
    }
}
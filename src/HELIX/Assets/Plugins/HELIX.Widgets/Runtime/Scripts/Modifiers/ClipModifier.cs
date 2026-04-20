using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class ClipModifier : Modifier {

        public static readonly ClipModifier Clip = new(true);
        public static readonly ClipModifier None = new(false);
        public readonly bool enabled;

        public ClipModifier(bool enabled) {
            this.enabled = enabled;
        }

        public override void Apply(VisualElement element) {
            element.style.overflow = enabled ? Overflow.Hidden : Overflow.Visible;
        }

        public override void Reset(VisualElement element) {
            element.style.overflow = StyleKeyword.Initial;
        }

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            base.FillModifierProperties(properties);
            properties.Add(new FlagProperty("enabled", enabled, "Clip", "None"));
        }

        protected override string FindConstantName() {
            return enabled ? nameof(Clip) : nameof(None);
        }

    }
}
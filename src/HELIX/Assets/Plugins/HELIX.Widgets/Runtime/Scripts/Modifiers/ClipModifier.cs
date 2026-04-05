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
    }
}
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class VisibilityModifier : Modifier {
        public static readonly VisibilityModifier Visible = new(true);
        public static readonly VisibilityModifier Hidden = new(false);
        public readonly bool visible;

        public VisibilityModifier(bool visible) {
            this.visible = visible;
        }

        public override void Apply(VisualElement element) {
            element.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        public override void Reset(VisualElement element) {
            element.style.visibility = StyleKeyword.Initial;
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not VisibilityModifier prev) return true;
            return visible != prev.visible;
        }
    }
}
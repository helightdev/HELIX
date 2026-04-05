using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class TextStyleModifier : Modifier {
        public readonly TextStyle style;

        public TextStyleModifier(TextStyle style) {
            this.style = style;
        }

        public override void Apply(VisualElement element) {
            (style ?? TextStyle.Default).Apply(element);
        }

        public override void Reset(VisualElement element) {
            TextStyle.Default.Apply(element);
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not TextStyleModifier prev) return true;
            return !Equals(style, prev.style);
        }

        public static TextStyleModifier Of(TextStyle style) {
            return new TextStyleModifier(style);
        }
    }
}
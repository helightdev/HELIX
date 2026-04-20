using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Universal.Styles;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class BackgroundStyleModifier : Modifier {
        public readonly BackgroundStyle style;

        public BackgroundStyleModifier(BackgroundStyle style) {
            this.style = style;
        }

        public override void Apply(VisualElement element) {
            (style ?? BackgroundStyle.Default).Apply(element);
        }

        public override void Reset(VisualElement element) {
            BackgroundStyle.Default.Apply(element);
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not BackgroundStyleModifier prev) return true;
            return !Equals(style, prev.style);
        }

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            base.FillModifierProperties(properties);
            properties.Add(new BackgroundStyleProperty("style", style, showName: false));
        }

        public static BackgroundStyleModifier Of(BackgroundStyle style) {
            return new BackgroundStyleModifier(style);
        }
    }
}
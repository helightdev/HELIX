using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class SpacingModifier : Modifier {
        public static readonly SpacingModifier Initial = new(StyleLength4.Initial, StyleLength4.Initial);
        public static readonly SpacingModifier None = new(StyleLength4.Initial, StyleLength4.Initial);
        public static readonly SpacingModifier NoPadding = new(StyleLength4.Zero, StyleLength4.Initial);
        public static readonly SpacingModifier NoMargin = new(StyleLength4.Zero, StyleLength4.Initial);
        public readonly StyleLength4 margin;
        public readonly StyleLength4 padding;

        public SpacingModifier(StyleLength4 padding, StyleLength4 margin) {
            this.padding = padding;
            this.margin = margin;
        }

        public SpacingModifier() {
            padding = StyleLength4.Initial;
            margin = StyleLength4.Initial;
        }

        public override void Apply(VisualElement element) {
            element.style.paddingLeft = padding.l;
            element.style.paddingTop = padding.t;
            element.style.paddingRight = padding.r;
            element.style.paddingBottom = padding.b;
            element.style.marginLeft = margin.l;
            element.style.marginTop = margin.t;
            element.style.marginRight = margin.r;
            element.style.marginBottom = margin.b;
        }

        public override void Reset(VisualElement element) {
            element.style.paddingLeft = StyleKeyword.Initial;
            element.style.paddingTop = StyleKeyword.Initial;
            element.style.paddingRight = StyleKeyword.Initial;
            element.style.paddingBottom = StyleKeyword.Initial;
            element.style.marginLeft = StyleKeyword.Initial;
            element.style.marginTop = StyleKeyword.Initial;
            element.style.marginRight = StyleKeyword.Initial;
            element.style.marginBottom = StyleKeyword.Initial;
        }

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            base.FillModifierProperties(properties);
            properties.Add(new DiagnosticsProperty<StyleLength4>("padding", padding));
            properties.Add(new DiagnosticsProperty<StyleLength4>("margin", margin));
        }

        protected override string FindConstantName() {
            if (DeepEquals(Initial)) return nameof(Initial);
            if (DeepEquals(None)) return nameof(None);
            if (DeepEquals(NoPadding)) return nameof(NoPadding);
            if (DeepEquals(NoMargin)) return nameof(NoMargin);
            return null;
        }

        public static SpacingModifier Padding(StyleLength4 padding) {
            return new SpacingModifier(padding, StyleLength4.Initial);
        }

        public static SpacingModifier Margin(StyleLength4 margin) {
            return new SpacingModifier(StyleLength4.Initial, margin);
        }

        public static SpacingModifier Of(StyleLength4? padding = null, StyleLength4? margin = null) {
            return new SpacingModifier(padding ?? StyleLength4.Initial, margin ?? StyleLength4.Initial);
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not SpacingModifier prev) return true;
            return !padding.Equals(prev.padding) || !margin.Equals(prev.margin);
        }
    }
}
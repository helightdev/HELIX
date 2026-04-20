using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class PaddingModifier : Modifier {
        public static readonly PaddingModifier Initial = new(StyleLength4.Initial);
        public static readonly PaddingModifier Zero = new(StyleLength4.Zero);

        public readonly StyleLength4 padding;

        public PaddingModifier(StyleLength4 padding) {
            this.padding = padding;
        }

        public override void Apply(VisualElement element) {
            element.style.paddingLeft = padding.l;
            element.style.paddingTop = padding.t;
            element.style.paddingRight = padding.r;
            element.style.paddingBottom = padding.b;
        }

        public override void Reset(VisualElement element) {
            element.style.paddingLeft = StyleKeyword.Initial;
            element.style.paddingTop = StyleKeyword.Initial;
            element.style.paddingRight = StyleKeyword.Initial;
            element.style.paddingBottom = StyleKeyword.Initial;
        }

        public override bool HasChanged(Modifier previous) {
            return !(previous is PaddingModifier prev) || !padding.Equals(prev.padding);
        }

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            base.FillModifierProperties(properties);
            properties.Add(new DiagnosticsProperty<StyleLength4>("padding", padding, showName: false));
        }

        protected override string FindConstantName() {
            if (DeepEquals(Initial)) return nameof(Initial);
            if (DeepEquals(Zero)) return nameof(Zero);
            return null;
        }

        public static PaddingModifier Of(StyleLength4 margin) {
            return new PaddingModifier(margin);
        }

        public static PaddingModifier Only(
            StyleLength? left = null,
            StyleLength? top = null,
            StyleLength? right = null,
            StyleLength? bottom = null
        ) {
            return new PaddingModifier(
                new StyleLength4(
                    left ?? StyleKeyword.Initial,
                    top ?? StyleKeyword.Initial,
                    right ?? StyleKeyword.Initial,
                    bottom ?? StyleKeyword.Initial
                )
            );
        }
    }
}
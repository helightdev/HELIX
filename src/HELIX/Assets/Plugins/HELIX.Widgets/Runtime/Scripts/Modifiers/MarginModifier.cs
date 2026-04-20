using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class MarginModifier : Modifier {

        public static readonly MarginModifier Initial = new(StyleLength4.Initial);
        public static readonly MarginModifier Zero = new(StyleLength4.Zero);

        public readonly StyleLength4 margin;

        public MarginModifier(StyleLength4 margin) {
            this.margin = margin;
        }

        public override void Apply(VisualElement element) {
            element.style.marginLeft = margin.l;
            element.style.marginTop = margin.t;
            element.style.marginRight = margin.r;
            element.style.marginBottom = margin.b;
        }

        public override void Reset(VisualElement element) {
            element.style.marginLeft = StyleKeyword.Initial;
            element.style.marginTop = StyleKeyword.Initial;
            element.style.marginRight = StyleKeyword.Initial;
            element.style.marginBottom = StyleKeyword.Initial;
        }

        public override bool HasChanged(Modifier previous) {
            return !(previous is MarginModifier prev) || !margin.Equals(prev.margin);
        }

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            base.FillModifierProperties(properties);
            properties.Add(new DiagnosticsProperty<StyleLength4>("margin", margin, showName: false));
        }

        protected override string FindConstantName() {
            if (DeepEquals(Initial)) return nameof(Initial);
            if (DeepEquals(Zero)) return nameof(Zero);
            return null;
        }

        public static MarginModifier Of(StyleLength4 margin) {
            return new MarginModifier(margin);
        }

        public static MarginModifier Only(
            StyleLength? left = null,
            StyleLength? top = null,
            StyleLength? right = null,
            StyleLength? bottom = null
        ) {
            return new MarginModifier(
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
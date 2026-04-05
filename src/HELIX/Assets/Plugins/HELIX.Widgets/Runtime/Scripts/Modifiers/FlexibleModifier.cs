using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class FlexibleModifier : Modifier {
        public static readonly FlexibleModifier Expand = new(1f, 1f, Align.Auto);
        public static readonly FlexibleModifier Shrink = new(0f, 1f, Align.Auto);
        public static readonly FlexibleModifier Tight = new(0f, 0f, Align.Auto);
        public static readonly FlexibleModifier Fill = new(1f, 1f, Align.Stretch);
        public static readonly FlexibleModifier TightStretch = new(0f, 0f, Align.Stretch);
        public readonly StyleFloat grow;
        public readonly Align selfCrossAxisAlign;
        public readonly StyleFloat shrink;

        public FlexibleModifier(StyleFloat grow, StyleFloat shrink, Align selfCrossAxisAlign) {
            this.selfCrossAxisAlign = selfCrossAxisAlign;
            this.grow = grow;
            this.shrink = shrink;
        }

        public FlexibleModifier() {
            selfCrossAxisAlign = Align.Auto;
            grow = 0;
            shrink = 1;
        }

        public override void Apply(VisualElement element) {
            element.style.alignSelf = selfCrossAxisAlign;
            element.style.flexGrow = grow;
            element.style.flexShrink = shrink;
        }

        public override void Reset(VisualElement element) {
            element.style.alignSelf = StyleKeyword.Initial;
            element.style.flexGrow = StyleKeyword.Initial;
            element.style.flexShrink = StyleKeyword.Initial;
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not FlexibleModifier prev) return true;
            return grow != prev.grow || shrink != prev.shrink || selfCrossAxisAlign != prev.selfCrossAxisAlign;
        }

        public static FlexibleModifier Of(StyleFloat grow, StyleFloat shrink, Align selfCrossAxisAlign = Align.Auto) {
            return new FlexibleModifier(grow, shrink, selfCrossAxisAlign);
        }
    }
}
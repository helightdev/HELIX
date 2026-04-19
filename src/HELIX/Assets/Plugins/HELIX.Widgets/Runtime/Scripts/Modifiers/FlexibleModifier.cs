using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class FlexibleModifier : Modifier {
        public static readonly FlexibleModifier Expand = new(1f, 1f, Align.Auto);
        public static readonly FlexibleModifier Shrink = new(0f, 1f, Align.Auto);
        public static readonly FlexibleModifier Tight = new(0f, 0f, Align.Auto);
        public static readonly FlexibleModifier Fill = new(1f, 1f, Align.Stretch);
        public static readonly FlexibleModifier TightStretch = new(0f, 0f, Align.Stretch);
        public readonly StyleFloat grow;
        public readonly StyleEnum<Align> selfCrossAxisAlign;
        public readonly StyleFloat shrink;
        public bool isImplicit;

        public FlexibleModifier(StyleFloat grow, StyleFloat shrink, StyleEnum<Align> selfCrossAxisAlign) {
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
            var parent = BuildContext.GetDirectParent(element);
            if (isImplicit && parent is IPreferExplicitFlex) {
                element.style.flexGrow = 0;
                element.style.flexShrink = 0;
                element.style.alignSelf = Align.Auto;
                return;
            }
            
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
            if (isImplicit) return true;
            return grow != prev.grow || shrink != prev.shrink || selfCrossAxisAlign != prev.selfCrossAxisAlign;
        }

        public static FlexibleModifier Of(
            StyleFloat grow,
            StyleFloat shrink,
            StyleEnum<Align>? selfCrossAxisAlign = null
        ) {
            return new FlexibleModifier(grow, shrink, selfCrossAxisAlign ?? Align.Auto);
        }

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            properties.Add(new StyleValueProperty<Align>("alignSelf", selfCrossAxisAlign));
            properties.Add(new StyleValueProperty<float>("grow", grow));
            properties.Add(new StyleValueProperty<float>("shrink", shrink));
            properties.Add(new FlagProperty("isImplicit", isImplicit, ifTrue: "Implicit"));
        }

        protected override string FindConstantName() {
            if (DeepEquals(Expand)) return nameof(Expand);
            if (DeepEquals(Shrink)) return nameof(Shrink);
            if (DeepEquals(Tight)) return nameof(Tight);
            if (DeepEquals(Fill)) return nameof(Fill);
            if (DeepEquals(TightStretch)) return nameof(TightStretch);
            return null;
        }
    }
}
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class PositionModifier : Modifier {
        public static readonly PositionModifier Stretch = new(StyleLength4.Zero, Position.Absolute);
        public static readonly PositionModifier None = new(StyleLength4.Initial, Position.Relative);
        public readonly StyleLength4 pos;
        public readonly Position type;
        public bool isStackingOnly;

        public PositionModifier(StyleLength4 pos, Position type) {
            this.pos = pos;
            this.type = type;
        }

        public PositionModifier() {
            pos = StyleLength4.Initial;
            type = Position.Relative;
        }

        public override void Apply(VisualElement element) {
            if (isStackingOnly) {
                var parent = BuildContext.GetDirectParent(element);
                if (parent is not IPreferStacking) return;
            }

            element.style.position = type;
            element.style.left = pos.l;
            element.style.top = pos.t;
            element.style.right = pos.r;
            element.style.bottom = pos.b;
        }

        public override void Reset(VisualElement element) {
            element.style.position = StyleKeyword.Initial;
            element.style.left = StyleKeyword.Initial;
            element.style.top = StyleKeyword.Initial;
            element.style.right = StyleKeyword.Initial;
            element.style.bottom = StyleKeyword.Initial;
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not PositionModifier prev) return true;
            return type != prev.type || !pos.Equals(prev.pos) || isStackingOnly != prev.isStackingOnly;
        }

        public static PositionModifier Absolute(StyleLength4 offset) {
            return new PositionModifier(offset, Position.Absolute);
        }

        public static PositionModifier Relative(StyleLength4 offset) {
            return new PositionModifier(offset, Position.Relative);
        }

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            properties.Add(new EnumProperty<Position>("type", type, Position.Relative, showName: false));
            properties.Add(
                new DiagnosticsProperty<StyleLength4>(
                    "pos",
                    pos,
                    defaultValue: StyleLength4.Initial,
                    showName: false
                )
            );
        }

        protected override string FindConstantName() {
            return type switch {
                Position.Absolute when pos.Equals(StyleLength4.Zero)    => nameof(Stretch),
                Position.Relative when pos.Equals(StyleLength4.Initial) => nameof(None),
                _                                                       => null
            };
        }
    }
}
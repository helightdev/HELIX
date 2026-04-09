using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class OffsetModifier : Modifier {
        public static readonly OffsetModifier Stretch = new(StyleLength4.Zero, Position.Absolute);
        public static readonly OffsetModifier None = new(StyleLength4.Initial, Position.Relative);
        public readonly StyleLength4 offset;
        public readonly Position offsetType;

        public OffsetModifier(StyleLength4 offset, Position offsetType) {
            this.offset = offset;
            this.offsetType = offsetType;
        }

        public OffsetModifier() {
            offset = StyleLength4.Initial;
            offsetType = Position.Relative;
        }

        public override void Apply(VisualElement element) {
            element.style.position = offsetType;
            element.style.left = offset.l;
            element.style.top = offset.t;
            element.style.right = offset.r;
            element.style.bottom = offset.b;
        }

        public override void Reset(VisualElement element) {
            element.style.position = StyleKeyword.Initial;
            element.style.left = StyleKeyword.Initial;
            element.style.top = StyleKeyword.Initial;
            element.style.right = StyleKeyword.Initial;
            element.style.bottom = StyleKeyword.Initial;
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not OffsetModifier prev) return true;
            return offsetType != prev.offsetType || !offset.Equals(prev.offset);
        }

        public static OffsetModifier Absolute(StyleLength4 offset) {
            return new OffsetModifier(offset, Position.Absolute);
        }

        public static OffsetModifier Relative(StyleLength4 offset) {
            return new OffsetModifier(offset, Position.Relative);
        }

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            properties.Add(
                new EnumProperty<Position>("type", offsetType, defaultValue: Position.Relative, showName: false)
            );
            properties.Add(
                new DiagnosticsProperty<StyleLength4>(
                    "offset",
                    offset,
                    defaultValue: StyleLength4.Initial,
                    showName: false
                )
            );
        }

        protected override string FindConstantName() {
            return offsetType switch {
                Position.Absolute when offset.Equals(StyleLength4.Zero)    => nameof(Stretch),
                Position.Relative when offset.Equals(StyleLength4.Initial) => nameof(None),
                _                                                          => null
            };
        }
    }
}
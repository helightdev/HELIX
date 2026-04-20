using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public static class ModifierFallbacks {

        public static readonly FlexibleModifier ImplicitFlexFill = new(1f, 1f, Align.Stretch) {
            isFallback = true,
            isImplicit = true
        };

        public static readonly PositionModifier StackingStretch = new(new StyleLength4(0), Position.Absolute) {
            isFallback = true,
            isStackingOnly = true
        };

        public static readonly FlexibleModifier FlexFill = new(1f, 1f, Align.Stretch) { isFallback = true };
        public static readonly FlexibleModifier FlexExpand = new(1f, 1f, Align.Auto) { isFallback = true };
        public static readonly FlexibleModifier FlexTight = new(0f, 0f, Align.Auto) { isFallback = true };
        public static readonly FlexibleModifier FlexTightStretch = new(0f, 0f, Align.Stretch) { isFallback = true };
        public static readonly PositionModifier PosStretch = new(new StyleLength4(0), Position.Absolute) { isFallback = true };

        public static readonly PaddingModifier PaddingZero = new(StyleLength4.Zero) { isFallback = true };
        public static readonly MarginModifier MarginZero = new(StyleLength4.Zero) { isFallback = true };

    }
}
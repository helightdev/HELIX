using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public static class ModifierFallbacks {
        public static FlexibleModifier ImplicitFlexFill =new(1f, 1f, Align.Stretch) {
            isFallback = true,
            isImplicit = true
        };
        public static FlexibleModifier FlexFill = new(1f, 1f, Align.Stretch) { isFallback = true };
        public static FlexibleModifier FlexExpand = new(1f, 1f, Align.Auto) { isFallback = true };
        public static FlexibleModifier FlexTight = new(0f, 0f, Align.Auto) { isFallback = true };
        public static FlexibleModifier FlexTightStretch = new(0f, 0f, Align.Stretch) { isFallback = true };
        public static PositionModifier PosStretch = new(new StyleLength4(0), Position.Absolute) { isFallback = true };
    }
}
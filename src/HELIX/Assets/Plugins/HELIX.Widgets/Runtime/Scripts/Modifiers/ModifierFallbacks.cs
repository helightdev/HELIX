using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public static class ModifierFallbacks {
        public static FlexibleModifier FlexFill = new(1f, 1f, Align.Stretch) { isFallback = true };
        public static FlexibleModifier Expand = new(1f, 1f, Align.Auto) { isFallback = true };
        public static FlexibleModifier Tight = new(0f, 0f, Align.Auto) { isFallback = true };
        public static FlexibleModifier TightStretch = new(0f, 0f, Align.Stretch) { isFallback = true };
        public static OffsetModifier Stretch = new(new StyleLength4(0), Position.Absolute);
    }
}
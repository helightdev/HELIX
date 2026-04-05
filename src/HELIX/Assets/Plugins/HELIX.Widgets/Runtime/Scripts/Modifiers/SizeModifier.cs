using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class SizeModifier : Modifier {
        public static readonly SizeModifier None = new(
            StyleLength2.Initial,
            StyleLength2.Initial,
            StyleLength2.Initial
        );

        public readonly StyleLength2 maxSize;
        public readonly StyleLength2 minSize;
        public readonly StyleLength2 size;

        public SizeModifier(StyleLength2 size, StyleLength2 minSize, StyleLength2 maxSize) {
            this.size = size;
            this.minSize = minSize;
            this.maxSize = maxSize;
        }

        public SizeModifier() {
            size = StyleLength2.Initial;
            minSize = StyleLength2.Initial;
            maxSize = StyleLength2.Initial;
        }

        public override void Apply(VisualElement element) {
            element.style.width = size.w;
            element.style.height = size.h;
            element.style.minWidth = minSize.w;
            element.style.minHeight = minSize.h;
            element.style.maxWidth = maxSize.w;
            element.style.maxHeight = maxSize.h;
        }

        public override void Reset(VisualElement element) {
            element.style.width = StyleKeyword.Initial;
            element.style.height = StyleKeyword.Initial;
            element.style.minWidth = StyleKeyword.Initial;
            element.style.minHeight = StyleKeyword.Initial;
            element.style.maxWidth = StyleKeyword.Initial;
            element.style.maxHeight = StyleKeyword.Initial;
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not SizeModifier prev) return true;
            return !size.Equals(prev.size) || !minSize.Equals(prev.minSize) || !maxSize.Equals(prev.maxSize);
        }

        public static SizeModifier Of(StyleLength width, StyleLength height) {
            return new SizeModifier(
                new StyleLength2(width, height),
                StyleLength2.Initial,
                StyleLength2.Initial
            );
        }

        public static SizeModifier Of(BoxConstraints constraints) {
            return new SizeModifier(
                constraints.preferred,
                constraints.min,
                constraints.max
            );
        }
    }
}
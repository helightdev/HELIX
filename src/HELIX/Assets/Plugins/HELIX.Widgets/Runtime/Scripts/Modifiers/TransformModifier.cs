using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class TransformModifier : Modifier {
        public static readonly TransformModifier None = new(
            StyleKeyword.Initial,
            StyleKeyword.Initial,
            StyleKeyword.Initial
        );

        public readonly StyleRotate rotate;
        public readonly StyleScale scale;
        public readonly StyleTranslate translate;

        public TransformModifier(StyleTranslate translate, StyleRotate rotate, StyleScale scale) {
            this.translate = translate;
            this.rotate = rotate;
            this.scale = scale;
        }

        public TransformModifier() {
            translate = StyleKeyword.Initial;
            rotate = StyleKeyword.Initial;
            scale = StyleKeyword.Initial;
        }

        public override void Apply(VisualElement element) {
            element.style.translate = translate;
            element.style.rotate = rotate;
            element.style.scale = scale;
        }

        public override void Reset(VisualElement element) {
            element.style.translate = StyleKeyword.Initial;
            element.style.rotate = StyleKeyword.Initial;
            element.style.scale = StyleKeyword.Initial;
        }
    }
}
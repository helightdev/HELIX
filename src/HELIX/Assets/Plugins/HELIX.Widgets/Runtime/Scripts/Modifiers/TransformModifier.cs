using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class TransformModifier : Modifier {

        public static readonly TransformModifier None = new(
            StyleKeyword.Initial,
            StyleKeyword.Initial,
            StyleKeyword.Initial
        );

        public static readonly TransformModifier Identity = new(Vector3.zero, Quaternion.identity, Vector3.one);

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

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            base.FillModifierProperties(properties);
            properties.Add(new StyleValueProperty<Translate>("translate", translate));
            properties.Add(new StyleValueProperty<Rotate>("rotate", rotate));
            properties.Add(new StyleValueProperty<Scale>("scale", scale));
        }

        protected override string FindConstantName() {
            if (ReferenceEquals(this, None)) return nameof(None);
            if (ReferenceEquals(this, Identity)) return nameof(Identity);
            return null;
        }

        public static TransformModifier Scale(StyleScale scale) {
            return new TransformModifier(StyleKeyword.Initial, StyleKeyword.Initial, scale);
        }

        public static TransformModifier Rotate(StyleRotate rotate) {
            return new TransformModifier(StyleKeyword.Initial, rotate, StyleKeyword.Initial);
        }

        public static TransformModifier Translate(StyleTranslate translate) {
            return new TransformModifier(translate, StyleKeyword.Initial, StyleKeyword.Initial);
        }

        public static TransformModifier Of(
            StyleTranslate? translate = null,
            StyleRotate? rotate = null,
            StyleScale? scale = null
        ) {
            return new TransformModifier(
                translate ?? StyleKeyword.Initial,
                rotate ?? StyleKeyword.Initial,
                scale ?? StyleKeyword.Initial
            );
        }

    }
}
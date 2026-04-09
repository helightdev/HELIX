using HELIX.Widgets.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class BoxShadow : SingleChildWidget {
        public float blurRadius = 4f;
        public Vector4 borderRadius = new(0, 0, 0, 0);
        public EasingMode easingFunction = EasingMode.Linear;
        public Vector2 offset = Vector2.zero;
        public Color shadowColor = new(0, 0, 0, 0.25f);
        public float spreadRadius;
        public float transitionDuration = 0.1f;

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new BoxShadowElement());
        }
    }
}
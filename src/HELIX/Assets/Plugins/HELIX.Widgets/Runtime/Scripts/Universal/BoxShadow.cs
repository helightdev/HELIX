using HELIX.Widgets.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class BoxShadow : Widget {
        public float blurRadius = 4f;
        public Vector4 borderRadius = new(0, 0, 0, 0);
        public Widget child;
        public Vector2 offset = Vector2.zero;
        public Color shadowColor = new(0, 0, 0, 0.25f);
        public float spreadRadius;
        public EasingMode easingFunction = EasingMode.Linear;
        public float transitionDuration = 0.1f;

        public override IWidgetElement CreateElement() {
            var element = new BoxShadowElement();
            element.RegisterCallbackOnce<AttachToPanelEvent>(_ => element.Reconcile(this));
            return element;
        }
    }
}
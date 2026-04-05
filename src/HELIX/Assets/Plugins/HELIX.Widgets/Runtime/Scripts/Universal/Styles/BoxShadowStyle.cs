using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal.Styles {
    public class BoxShadowStyle {
        public float blurRadius = 4f;
        public Vector4 borderRadius = new(0, 0, 0, 0);
        public Vector2 offset = Vector2.zero;
        public Color shadowColor = new(0, 0, 0, 0.25f);
        public float spreadRadius;
        public EasingMode easingFunction = EasingMode.Linear;
        public TimeValue transitionDuration = 0.1f;
    }
}
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class HBoxShadow : SingleChildWidget {
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

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new FloatProperty("blurRadius", blurRadius));
            properties.Add(new DiagnosticsProperty<Vector4>("borderRadius", borderRadius));
            properties.Add(new EnumProperty<EasingMode>("easingFunction", easingFunction));
            properties.Add(new DiagnosticsProperty<Vector2>("offset", offset));
            properties.Add(new ColorProperty("shadowColor", shadowColor));
            properties.Add(new FloatProperty("spreadRadius", spreadRadius, defaultValue: 0f));
            properties.Add(new FloatProperty("transitionDuration", transitionDuration, defaultValue: 0.1f));
        }
    }
}
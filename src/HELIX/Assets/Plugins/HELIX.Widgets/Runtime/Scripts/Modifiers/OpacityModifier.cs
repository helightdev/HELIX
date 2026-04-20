using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class OpacityModifier : Modifier {

        public static readonly OpacityModifier Opaque = new(1f);
        public static readonly OpacityModifier Transparent = new(0f);
        public readonly float opacity;

        public OpacityModifier(float opacity) {
            this.opacity = opacity;
        }

        public override void Apply(VisualElement element) {
            element.style.opacity = opacity;
        }

        public override void Reset(VisualElement element) {
            element.style.opacity = StyleKeyword.Initial;
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not OpacityModifier prev) return true;
            return !Mathf.Approximately(opacity, prev.opacity);
        }

        public static OpacityModifier Of(float opacity) {
            if (Mathf.Approximately(opacity, 1f)) return Opaque;
            if (Mathf.Approximately(opacity, 0f)) return Transparent;
            return new OpacityModifier(opacity);
        }

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            base.FillModifierProperties(properties);
            properties.Add(new PercentProperty("opacity", opacity, showName: false));
        }

        protected override string FindConstantName() {
            if (Mathf.Approximately(opacity, 1f)) return nameof(Opaque);
            if (Mathf.Approximately(opacity, 0f)) return nameof(Transparent);
            return null;
        }

    }
}
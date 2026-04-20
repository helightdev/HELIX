using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class BorderModifier : Modifier {
        public static readonly BorderModifier None = new(Border.None, BorderRadius.None);

        public readonly Border border;
        public readonly BorderRadius radius;

        public BorderModifier(Border border, BorderRadius radius) {
            this.border = border;
            this.radius = radius;
        }

        public override void Apply(VisualElement element) {
            border.Apply(element);
            radius.Apply(element);
        }

        public override void Reset(VisualElement element) {
            Border.None.Apply(element);
            BorderRadius.None.Apply(element);
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not BorderModifier prev) return true;
            return !Equals(border, prev.border);
        }

        protected override string FindConstantName() {
            return Equals(border, Border.None) ? nameof(None) : null;
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<Border>("border", border, showName: false));
        }

        public static BorderModifier Of(Border? border = null, BorderRadius? radius = null) {
            return new BorderModifier(border ?? Border.None, radius ?? BorderRadius.None);
        }
    }
}
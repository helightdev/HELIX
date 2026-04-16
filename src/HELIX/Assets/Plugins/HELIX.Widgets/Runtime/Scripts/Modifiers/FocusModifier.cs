using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class FocusModifier : Modifier {
        public static readonly FocusModifier Focusable = new(true, PickingMode.Position, 0, false);
        public static readonly FocusModifier FocusableNoTab = new(true, PickingMode.Position, -1, false);
        public static readonly FocusModifier Ignore = new(false, PickingMode.Ignore, -1, false);
        public static readonly FocusModifier None = new(false, PickingMode.Position, -1, false);
        public readonly bool focusable;
        public readonly PickingMode pickingMode;
        public readonly int tabIndex;
        public readonly bool delegatesFocus;

        public FocusModifier(bool focusable, PickingMode pickingMode, int tabIndex, bool delegatesFocus) {
            this.focusable = focusable;
            this.pickingMode = pickingMode;
            this.tabIndex = tabIndex;
            this.delegatesFocus = delegatesFocus;
        }

        public override void Apply(VisualElement element) {
            element.focusable = focusable;
            element.pickingMode = pickingMode;
            element.tabIndex = tabIndex;
            element.delegatesFocus = delegatesFocus;
        }

        public override void Reset(VisualElement element) {
            element.focusable = false;
            element.pickingMode = PickingMode.Position;
            element.tabIndex = -1;
            element.delegatesFocus = false;
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not FocusModifier prev) return true;
            return focusable != prev.focusable || pickingMode != prev.pickingMode ||
                   tabIndex != prev.tabIndex || delegatesFocus != prev.delegatesFocus;
        }

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            base.FillModifierProperties(properties);
            properties.Add(new FlagProperty("focusable", focusable, ifTrue: "Focusable", ifFalse: "Not Focusable"));
            properties.Add(new EnumProperty<PickingMode>("pickingMode", pickingMode));
            properties.Add(new IntProperty("tabIndex", tabIndex));
            properties.Add(
                new FlagProperty(
                    "delegatesFocus",
                    delegatesFocus,
                    ifTrue: "Delegates Focus"
                )
            );
        }

        protected override string FindConstantName() {
            if (DeepEquals(Focusable)) return nameof(Focusable);
            if (ReferenceEquals(this, FocusableNoTab)) return nameof(FocusableNoTab);
            if (DeepEquals(Ignore)) return nameof(Ignore);
            if (DeepEquals(None)) return nameof(None);
            return null;
        }

        public static FocusModifier Of(
            int tabIndex = 0,
            bool focusable = true,
            PickingMode mode = PickingMode.Position,
            bool delegatesFocus = false
        ) {
            return new FocusModifier(focusable, mode, tabIndex, delegatesFocus);
        }
    }
}
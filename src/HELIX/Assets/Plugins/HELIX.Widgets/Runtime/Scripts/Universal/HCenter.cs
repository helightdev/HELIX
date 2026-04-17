using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets.Universal {
    public class HCenter : SingleChildWidget {
        public HCenter() {
            AddModifier(ModifierFallbacks.FlexFill);
            AddModifier(ModifierFallbacks.StackingStretch);
        }

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new CenterElement());
        }
    }
}
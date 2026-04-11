using HELIX.Types;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets.Universal {
    public class HAlign : SingleChildWidget {
        public Alignment alignment;

        public HAlign() {
            modifiers.Add(ModifierFallbacks.FlexFill);
        }

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new FlexAlignElement());
        }
    }
}
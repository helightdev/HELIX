using HELIX.Types;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets.Universal {
    public class FlexAlign : SingleChildWidget {
        public Alignment alignment;

        public FlexAlign() {
            modifiers.Add(ModifierFallbacks.Fill);
        }

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new FlexAlignElement());
        }
    }
}
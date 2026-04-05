using HELIX.Types;
using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
    public class FlexAlign : Widget {
        public Alignment alignment;
        public Widget child;

        public override IWidgetElement CreateElement() {
            var align = new FlexAlignElement();
            align.Reconcile(this);
            return align;
        }
    }
}
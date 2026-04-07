using HELIX.Types;
using HELIX.Widgets.Elements;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class FlexAlign : Widget {
        public Alignment alignment;
        public Widget child;

        public override IWidgetElement CreateElement() {
            var align = new FlexAlignElement();
            align.RegisterCallbackOnce<AttachToPanelEvent>(_ => align.Reconcile(this));
            return align;
        }
    }
}
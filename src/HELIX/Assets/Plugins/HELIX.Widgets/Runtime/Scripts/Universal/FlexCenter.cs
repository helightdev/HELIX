using HELIX.Widgets.Elements;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class FlexCenter : Widget {
        public Widget child;

        public override IWidgetElement CreateElement() {
            var element = new CenterElement();
            element.RegisterCallbackOnce<AttachToPanelEvent>(_ => element.Reconcile(this));
            return element;
        }
    }
}
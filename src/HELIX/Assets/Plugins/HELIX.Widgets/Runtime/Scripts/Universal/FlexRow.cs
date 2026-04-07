using HELIX.Widgets.Elements;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class FlexRow : DirectionalContainerWidget {
        public override IWidgetElement CreateElement() {
            var element = new RowElement();
            element.RegisterCallbackOnce<AttachToPanelEvent>(_ => element.Reconcile(this));
            return element;
        }
    }
}
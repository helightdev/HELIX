using HELIX.Widgets.Elements;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
    public class FlexColumn : DirectionalContainerWidget {
        public override IWidgetElement CreateElement() {
            var element = new ColumnElement();
            element.RegisterCallbackOnce<AttachToPanelEvent>(_ => element.Reconcile(this));
            return element;
        }
    }
}
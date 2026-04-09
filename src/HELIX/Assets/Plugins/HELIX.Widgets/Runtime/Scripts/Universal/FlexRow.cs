using HELIX.Widgets.Elements;
using UnityEngine;

namespace HELIX.Widgets.Universal {
    public class FlexRow : DirectionalContainerWidget {
        public override IWidgetElement CreateElement() {
            return ReconcileInto(new RowElement());
        }
    }
}
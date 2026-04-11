using HELIX.Widgets.Elements;
using UnityEngine;

namespace HELIX.Widgets.Universal {
    public class HRow : DirectionalContainerWidget {
        public override IWidgetElement CreateElement() {
            return ReconcileInto(new RowElement());
        }
    }
}
using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
    public class HRow : DirectionalContainerWidget {
        public override IWidgetElement CreateElement() {
            return ReconcileInto(new RowElement());
        }
    }
}
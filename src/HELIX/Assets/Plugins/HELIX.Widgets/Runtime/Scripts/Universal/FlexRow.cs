using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
    public class FlexRow : DirectionalContainerWidget {
        public override IWidgetElement CreateElement() {
            return ReconcileInto(new RowElement());
        }
    }
}
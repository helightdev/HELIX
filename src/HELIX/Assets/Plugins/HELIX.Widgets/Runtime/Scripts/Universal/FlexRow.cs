using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
    public class FlexRow : DirectionalContainerWidget {
        public override IWidgetElement CreateElement() {
            var row = new RowElement();
            row.Reconcile(this);
            return row;
        }
    }
}
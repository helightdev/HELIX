using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
    public class FlexColumn : DirectionalContainerWidget {
        public override IWidgetElement CreateElement() {
            var row = new ColumnElement();
            row.Reconcile(this);
            return row;
        }
    }
}
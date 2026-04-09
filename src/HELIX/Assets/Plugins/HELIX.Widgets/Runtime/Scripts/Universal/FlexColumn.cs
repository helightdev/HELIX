using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
    public class FlexColumn : DirectionalContainerWidget {
        public override IWidgetElement CreateElement() {
            return ReconcileInto(new ColumnElement());
        }
    }
}
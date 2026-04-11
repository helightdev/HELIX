using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
    public class HColumn : DirectionalContainerWidget {
        public override IWidgetElement CreateElement() {
            return ReconcileInto(new ColumnElement());
        }
    }
}
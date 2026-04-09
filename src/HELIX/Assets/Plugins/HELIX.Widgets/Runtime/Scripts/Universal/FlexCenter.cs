using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
    public class FlexCenter : SingleChildWidget {
        public override IWidgetElement CreateElement() {
            return ReconcileInto(new CenterElement());
        }
    }
}
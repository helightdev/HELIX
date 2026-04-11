using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
    public class HCenter : SingleChildWidget {
        public override IWidgetElement CreateElement() {
            return ReconcileInto(new CenterElement());
        }
    }
}
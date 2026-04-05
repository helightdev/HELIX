using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
    public class FlexCenter : Widget {
        public Widget child;

        public override IWidgetElement CreateElement() {
            var center = new CenterElement();
            center.Reconcile(this);
            return center;
        }
    }
}
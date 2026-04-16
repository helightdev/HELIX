namespace HELIX.Widgets.Universal {
    public class HSlider : StatefulWidget<HSlider> {
        public override State<HSlider> CreateState() {
            return new HSliderState();
        }
    }

    public class HSliderState : State<HSlider> {
        public override Widget Build(BuildContext context) {
            return new HBox();
        }
    }
    
}
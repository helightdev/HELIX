namespace HELIX.Widgets.Universal {
    public class HStatefulBuilder : StatefulWidget<HStatefulBuilder> {
        public BuildFunction<State<HStatefulBuilder>> builder;

        public HStatefulBuilder(BuildFunction<State<HStatefulBuilder>> builder) {
            this.builder = builder;
        }

        public override State<HStatefulBuilder> CreateState() {
            return new HStatefulBuilderState();
        }
    }

    public class HStatefulBuilderState : State<HStatefulBuilder> {
        public override Widget Build(BuildContext context) {
            return widget.builder(context, this);
        }
    }
}
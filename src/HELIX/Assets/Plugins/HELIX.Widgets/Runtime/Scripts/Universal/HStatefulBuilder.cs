using System.Collections.Generic;

namespace HELIX.Widgets.Universal {
    public class HStatefulBuilder : StatefulWidget<HStatefulBuilder> {
        public readonly BuildFunction<State<HStatefulBuilder>> builder;

        public HStatefulBuilder(
            BuildFunction<State<HStatefulBuilder>> builder,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(key, constants, modifiers) {
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
using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Controllers;

namespace HELIX.Widgets.Universal {
    public class HSubstanceBox : StatefulWidget<HSubstanceBox> {
        public WidgetStateController controller;
        public SubstanceLayers substances;
        public BuildFunction<WidgetState> builder;
        public WidgetStateProperty<Alignment> alignment = Alignment.Center;
        public Key boxKey;
        public WidgetStateProperty<ModifierSet> boxModifiers = WidgetStateProperties.Never<ModifierSet>();

        public override State<HSubstanceBox> CreateState() {
            return new HSubstanceBoxState();
        }
    }

    public class HSubstanceBoxState : State<HSubstanceBox> {
        public override Widget Build(BuildContext context) {
            var state = widget.controller?.Value ?? WidgetState.None;

            var shapes = new List<Widget>(widget.substances.Count + 1);
            foreach (var shape in widget.substances) {
                var childWidget = shape.Build(context, state);
                childWidget.constants = new object[] { state };
                childWidget.AddModifier(ModifierFallbacks.PosStretch);
                shapes.Add(childWidget);
            }

            if (widget.builder != null) shapes.Add(widget.builder(context, state));

            var currentAlignment = widget.alignment.ResolveOrDefault(state, Alignment.Center);
            Alignment.AlignmentHelper.ToColumnAlignment(
                currentAlignment.Quantize(),
                out var mainAxis,
                out var crossAxis
            );

            var column = new HStack {
                key = widget.boxKey,
                children = shapes,
                mainAxisAlign = mainAxis,
                crossAxisAlign = crossAxis,
                Modifiers = widget.boxModifiers.ResolveOrDefault(state, ModifierSet.Empty)
            };
            return column;
        }
    }
}
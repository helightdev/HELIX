using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Controllers;

namespace HELIX.Widgets.Universal {
    public class HSubstanceBox : StatefulWidget<HSubstanceBox> {
        public readonly WidgetStateProperty<Alignment> alignment;
        public readonly Key boxKey;
        public readonly WidgetStateProperty<ModifierSet> boxModifiers;
        public readonly BuildFunction<WidgetState> builder;
        public readonly WidgetStateController controller;
        public readonly SubstanceLayers substances;

        public HSubstanceBox(
            WidgetStateController controller = null,
            SubstanceLayers substances = default,
            BuildFunction<WidgetState> builder = null,
            Key boxKey = default,
            WidgetStateProperty<Alignment> alignment = null,
            WidgetStateProperty<ModifierSet> boxModifiers = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(key, constants, modifiers) {
            this.controller = controller;
            this.substances = substances;
            this.builder = builder;
            this.boxKey = boxKey;
            this.alignment = alignment ?? Alignment.Center;
            this.boxModifiers = boxModifiers ?? WidgetStateProperties.Never<ModifierSet>();
        }

        public override State<HSubstanceBox> CreateState() {
            return new HSubstanceBoxState();
        }
    }

    public class HSubstanceBoxState : State<HSubstanceBox> {
        public override Widget Build(BuildContext context) {
            var state = widget.controller?.Value ?? WidgetState.None;

            var widgetList = new WidgetList(widget.substances.Count + 1);
            foreach (var shape in widget.substances) {
                var candidate = shape.Build(context, state);
                if (candidate == null) continue;
                var previousCount = widgetList.Count;
                widgetList.Add(candidate);
                for (var i = previousCount; i < widgetList.Count; i++) {
                    var childWidget = widgetList[i];
                    childWidget.constants = new object[] { state, shape };
                    childWidget.AddModifier(ModifierFallbacks.PosStretch);
                }
            }

            if (widget.builder != null) widgetList.Add(widget.builder(context, state));

            var currentAlignment = widget.alignment.ResolveOrDefault(state, Alignment.Center);
            Alignment.AlignmentHelper.ToColumnAlignment(
                currentAlignment.Quantize(),
                out var mainAxis,
                out var crossAxis
            );

            var column = new HStack(
                key: widget.boxKey,
                children: widgetList,
                mainAxisAlign: mainAxis,
                crossAxisAlign: crossAxis,
                modifiers: widget.boxModifiers.ResolveOrDefault(state, ModifierSet.Empty)
            );
            return column;
        }
    }
}
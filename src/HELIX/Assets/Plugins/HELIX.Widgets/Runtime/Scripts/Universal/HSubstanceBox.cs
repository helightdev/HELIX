using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Controllers;

namespace HELIX.Widgets.Universal {
  /// <summary>
  /// A widget that materializes <see cref="SubstanceLayers"/>.
  /// </summary>
  public class HSubstanceBox : StatefulWidget<HSubstanceBox> {
    public readonly WidgetStateProperty<Alignment> alignment;
    public readonly Key boxKey;
    public readonly WidgetStateProperty<ModifierSet> boxModifiers;
    public readonly BuildFunction<WidgetState> builder;
    public readonly WidgetStateController controller;
    public readonly SubstanceLayers substances;

    /// <summary>
    /// Creates a widget that materializes <see cref="SubstanceLayers"/>.
    /// </summary>
    /// <param name="controller">The controller used to retrieve the current <see cref="WidgetState"/>.</param>
    /// <param name="substances">The substances to be materialized.</param>
    /// <param name="builder">A function that builds the child widget based on the current <see cref="WidgetState"/>.</param>
    /// <param name="boxKey">The key to be used by the inner stack.</param>
    /// <param name="alignment">The alignment of the inner stack.</param>
    /// <param name="boxModifiers">The modifiers to be applied to the inner stack.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <seealso cref="Substance"/>
    /// <seealso cref="SubstanceLayers"/>
    /// <inheritdoc/>
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

      // ReSharper disable once ForCanBeConvertedToForeach
      for (var si = 0; si < widget.substances.Count; si++) {
        var shape = widget.substances[si];
        var candidate = shape.Build(context, state);
        if (candidate == null) continue;
        var previousCount = widgetList.Count;
        widgetList.Add(candidate);
        var constants = new object[] { state, shape };
        for (var i = previousCount; i < widgetList.Count; i++) {
          var childWidget = widgetList[i];
          childWidget.constants = constants;
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
using System.Collections.Generic;
using HELIX.Widgets.Signals;

namespace HELIX.Widgets.Universal {
  public class HStatefulBuilder : StatefulWidget<HStatefulBuilder> {
    public readonly BuildFunction<State<HStatefulBuilder>> builder;

    /// <summary>
    /// Creates an inline wrapper around <see cref="StatefulWidget{T}"/>.
    /// </summary>
    /// <param name="builder">A wrapper around <see cref="State{T}.Build"/></param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <remarks>
    /// Allows the usage of <see cref="Signal"/>s in the <paramref name="builder"/>.
    /// </remarks>
    /// <inheritdoc/>
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
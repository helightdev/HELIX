using System.Collections.Generic;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets.Universal {
  /// <summary>
  /// A widget that tries to center its child using flex layouting.
  /// </summary>
  public class HCenter : SingleChildWidget {

    /// <summary>
    /// Creates a widget that tries to center its child using flex layouting.
    /// </summary>
    /// <param name="child">The child widget to center.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <seealso cref="IPreferExplicitFlex"/>
    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    /// <seealso cref="ModifierFallbacks.StackingStretch"/>
    /// <inheritdoc/>
    public HCenter(
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(child, key, constants) {
      DefaultModifiers(ModifierSet.DefaultFlexFillAndStacking, modifiers);
    }

    public override IWidgetElement CreateElement() {
      return ReconcileInto(new CenterElement());
    }
  }
}
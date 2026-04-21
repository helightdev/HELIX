using System.Collections.Generic;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
  /// <summary>
  /// Represents a vertically oriented flexible container widget.
  /// </summary>
  public class HColumn : DirectionalContainerWidget {
    /// <summary>
    /// Create a vertically oriented flexible container widget.
    /// </summary>
    /// <param name="crossAxisAlign">The alignment along the cross-axis.</param>
    /// <param name="mainAxisAlign">The alignment along the main axis.</param>
    /// <param name="gap">A fixed amount of spacing on the main axis between children.</param>
    /// <param name="reverse">Whether to reverse the order of the children.</param>
    /// <param name="children">The children of this container.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <seealso cref="IPreferExplicitFlex"/>
    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    public HColumn(
      Justify mainAxisAlign = Justify.FlexStart,
      Align crossAxisAlign = Align.Center,
      float gap = 0,
      bool reverse = false,
      IReadOnlyList<Widget> children = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(mainAxisAlign, crossAxisAlign, gap, reverse, children, key, constants, modifiers) { }

    public override IWidgetElement CreateElement() {
      return ReconcileInto(new ColumnElement());
    }
  }
}
using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets.Universal {

  /// <summary>
  /// A widget that aligns its child to the specified alignment.
  /// </summary>
  public class HAlign : SingleChildWidget {
    public readonly Alignment alignment;

    /// <summary>
    /// Creates a widget that aligns its child to the specified alignment.
    /// </summary>
    /// <param name="alignment">The non-quantized alignment to apply.</param>
    /// <param name="child">The child widget to align.</param>
    /// <param name="key">Passed on to <see cref="Widget.key"/>.</param>
    /// <param name="constants">Passed on to <see cref="Widget.constants"/>.</param>
    /// <param name="modifiers">Passed on to <see cref="Widget.modifiers"/>.</param>
    /// <seealso cref="IPreferExplicitFlex"/>
    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    /// <seealso cref="ModifierFallbacks.StackingStretch"/>
    /// <inheritdoc/>
    public HAlign(
      Alignment alignment,
      Widget child = null,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(child, key, constants) {
      this.alignment = alignment;

      DefaultModifiers(ModifierSet.DefaultFlexFillAndStacking, modifiers);
    }

    public override IWidgetElement CreateElement() {
      return ReconcileInto(new FlexAlignElement());
    }
  }
}
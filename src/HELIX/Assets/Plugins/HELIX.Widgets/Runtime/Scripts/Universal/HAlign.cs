using System.Collections.Generic;
using HELIX.Types;
using HELIX.Widgets.Elements;

namespace HELIX.Widgets.Universal {
  public class HAlign : SingleChildWidget {
    public readonly Alignment alignment;

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
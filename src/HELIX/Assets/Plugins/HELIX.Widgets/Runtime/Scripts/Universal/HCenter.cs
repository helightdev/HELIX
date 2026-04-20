using System.Collections.Generic;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;

namespace HELIX.Widgets.Universal {
  public class HCenter : SingleChildWidget {
    public HCenter() {
      AddModifier(ModifierFallbacks.FlexFill);
      AddModifier(ModifierFallbacks.StackingStretch);
    }

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
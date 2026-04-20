using System.Collections.Generic;
using HELIX.Widgets.Elements;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal {
  public class HRow : DirectionalContainerWidget {
    public HRow(
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
      return ReconcileInto(new RowElement());
    }
  }
}
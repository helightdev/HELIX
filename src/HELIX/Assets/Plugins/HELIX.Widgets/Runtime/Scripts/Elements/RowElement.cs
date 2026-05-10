using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
  [UxmlElement]
  public partial class RowElement : DirectionalContainerElement {
    protected override FlexDirection GetFlexDirection(bool reverse) {
      return reverse ? FlexDirection.RowReverse : FlexDirection.Row;
    }

    protected override Axis GetAxis() {
      return Axis.Horizontal;
    }
  }
}
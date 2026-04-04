using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Layout {
    [UxmlElement]
    public partial class ColumnElement : DirectionalContainerElement {
        protected override FlexDirection GetFlexDirection(bool reverse) {
            return reverse ? FlexDirection.ColumnReverse : FlexDirection.Column;
        }

        protected override Axis GetAxis() {
            return Axis.Vertical;
        }
    }
}
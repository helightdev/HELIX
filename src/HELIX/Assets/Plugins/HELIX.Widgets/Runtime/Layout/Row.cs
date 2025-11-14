using System.Collections.Generic;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Layout {
    
    [UxmlElement]
    public partial class Row : DirectionalContainer {
        protected override FlexDirection GetFlexDirection(bool reverse) {
            return reverse ? FlexDirection.RowReverse : FlexDirection.Row;
        }

        protected override Axis GetAxis() {
            return Axis.Horizontal;
        }
    }
}
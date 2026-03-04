using HELIX.Extensions;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Layout {
    [UxmlElement]
    public partial class Center : SingleChildContainerWidget {
        public Center() {
            this.FlexContainer(mainAxisAlign: Justify.Center, crossAxisAlign: Align.Center);
        }
    }
}
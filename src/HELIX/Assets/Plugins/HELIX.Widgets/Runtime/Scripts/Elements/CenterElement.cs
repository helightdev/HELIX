using HELIX.Extensions;
using HELIX.Widgets.Universal;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
  [UxmlElement]
  public partial class CenterElement : SingleChildWidgetBaseElement<HCenter>, IPreferExplicitFlex {
    public CenterElement() {
      this.FlexContainer(mainAxisAlign: Justify.Center, crossAxisAlign: Align.Center);
    }
  }
}
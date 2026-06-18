using System.Collections.Generic;
using HELIX.Widgets;
using UnityEngine.UIElements;

namespace Examples {
  public class HostedWidgetExamples : StatelessWidget<HostedWidgetExamples> {
    public override Widget Build(BuildContext context) {
      return new DropdownField(
        new List<string>() { "Option 1", "Option 2", "Option 3", "Option 4", "Option 5" },
        "Option 1"
      ) {
        label = "Dropdown Field"
      };
    }
  }
}
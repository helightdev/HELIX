using System.Collections.Generic;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
  public class HostedWidget : WrappingBaseWidget<HostedWidget, VisualElement> {
    public readonly VisualElement element;

    public HostedWidget(
      VisualElement element,
      Key key = default,
      object[] constants = null,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, constants, modifiers) {
      this.element = element;
    }

    public override VisualElement Create() {
      if (element.parent != null) element.RemoveFromHierarchy();
      return element;
    }

    public override void Apply(HostedWidget previous, VisualElement target) { }

    public override bool CanReconcile(HostedWidget previous, VisualElement target) {
      return target == element;
    }
  }
}
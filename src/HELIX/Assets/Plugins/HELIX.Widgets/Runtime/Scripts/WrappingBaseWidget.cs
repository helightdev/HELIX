using HELIX.Extensions;
using HELIX.Widgets.Elements;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
  public abstract class WrappingBaseWidget<S, T> : Widget, IUserDataWidget<S, T>
    where T : VisualElement where S : WrappingBaseWidget<S, T> {
    public abstract void Apply(S previous, T element);
    public abstract T Create();

    public override IWidgetElement CreateElement() {
      var element = Create();
      var descriptive = new UserDataWidgetElement<S, T> {
        Element = element,
        Descriptor = this
      };
      element.userData = descriptive;
      element.RegisterCallback<AttachToPanelEvent>(_ => descriptive.HierarchyDepth = element.GetDepth());
      element.RegisterCallbackOnce<AttachToPanelEvent>(_ => {
          Apply(null, element);
          Modifier.ApplyDelta(null, this, element);
        }
      );
      return descriptive;
    }
  }
}
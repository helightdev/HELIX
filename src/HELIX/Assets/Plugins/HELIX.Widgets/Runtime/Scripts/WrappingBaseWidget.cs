using HELIX.Extensions;
using HELIX.Widgets.Elements;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class WrappingBaseWidget<T, S> : Widget where T : VisualElement where S : WrappingBaseWidget<T, S> {
        public abstract T Create();
        public abstract void Apply(S previous, T element);

        public override IWidgetElement CreateElement() {
            var element = Create();
            var descriptive = new UserDataElement {
                Element = element,
                Descriptor = this
            };
            element.userData = descriptive;
            element.RegisterCallback<AttachToPanelEvent>(_ => descriptive.HierarchyDepth = element.GetDepth());
            Apply(null, element);
            Modifier.ApplyDelta(null, this, element);
            return descriptive;
        }

        public class UserDataElement : IWidgetElement {
            public VisualElement Element { get; set; }
            public Widget Descriptor { get; set; }
            public int HierarchyDepth { get; set; }

            public bool Reconcile(Widget updated) {
                if (updated is not WrappingBaseWidget<T, S> wd) return false;
                if (Element is not T element) return false;
                wd.Apply(Descriptor as S, element);
                Modifier.ApplyDelta(Descriptor, updated, element);
                Descriptor = updated;
                return true;
            }
        }
    }
}
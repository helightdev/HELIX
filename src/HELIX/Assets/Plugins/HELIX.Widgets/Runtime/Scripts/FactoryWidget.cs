using System;
using HELIX.Extensions;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public class FactoryWidget<T> : Widget where T : VisualElement {
        public Func<T> creator;
        public Action<T> updater;

        public override IWidgetElement CreateElement() {
            if (creator == null)
                throw new InvalidOperationException(
                    "FactoryDescriptor requires a builder function to create an element."
                );
            var element = creator();
            var descriptive = new UserDataElement {
                Element = element,
                Descriptor = this
            };
            element.userData = descriptive;
            element.RegisterCallback<AttachToPanelEvent>(_ => descriptive.HierarchyDepth = element.GetDepth());
            updater?.Invoke(element);
            Modifier.ApplyDelta(null, this, element);
            return descriptive;
        }

        public class UserDataElement : IWidgetElement {
            public VisualElement Element { get; set; }
            public Widget Descriptor { get; set; }
            public int HierarchyDepth { get; set; }

            public bool CanReconcile(Widget updated) {
                return updated is FactoryWidget<T> && Element is T;
            }

            public bool Reconcile(Widget updated) {
                if (updated is not FactoryWidget<T> fd) return false;
                if (fd.updater == null) return false;
                if (Element is not T element) return false;
                fd.updater(element);
                Modifier.ApplyDelta(Descriptor, updated, element);
                Descriptor = updated;
                return true;
            }
        }
    }
}
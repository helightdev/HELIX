using HELIX.Extensions;
using HELIX.Widgets.Descriptors;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Layout {
    [UxmlElement]
    public partial class CenterElement : SingleChildContainerElement, IWidgetElement {
        public CenterElement() {
            this.FlexContainer(mainAxisAlign: Justify.Center, crossAxisAlign: Align.Center);
        }

        public VisualElement Element => this;
        public Widget Descriptor { get; set; }

        public bool Reconcile(Widget updated) {
            if (updated is not FlexCenter fa) return false;
            DefaultReconciler.ReconcileSingleDirect(this, fa.child);
            Modifier.ApplyDelta(Descriptor, updated, this);
            Descriptor = updated;
            return true;
        }
    }

    public abstract class SingleChildWidgetHostImage<T> : SingleChildContainerElement, IWidgetElement
        where T : Widget {
        
        public VisualElement Element => this;
        public Widget Descriptor { get; private set; }

        public bool Reconcile(Widget updated) {
            if (updated is not T widget) return false;
            var previous = Descriptor as T;
            Apply(previous, widget);
            DefaultReconciler.ReconcileSingleDirect(this, GetChild(widget));
            Modifier.ApplyDelta(Descriptor, updated, this);
            Descriptor = updated;
            return true;
        }
        
        public abstract Widget GetChild(T widget);
        
        public virtual void Apply(T previous, T widget) {}
    }
}
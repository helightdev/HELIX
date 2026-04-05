using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
    public abstract class SingleChildWidgetHostElement<T> : SingleChildContainerElement, IWidgetElement
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

        public virtual void Apply(T previous, T widget) { }
    }
}
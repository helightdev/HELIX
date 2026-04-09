using System.Collections.Generic;
using HELIX.Abstractions;
using HELIX.Widgets.Diagnostics;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
    public abstract class SuperSingleChildWidgetBaseElement<T> : WidgetBaseElement<T>, ISingleChildContainer
        where T : Widget {
        public virtual VisualElement Child {
            get {
                if (contentContainer.childCount == 0) return null;
                return contentContainer.ElementAt(0);
            }
            set {
                if (contentContainer.childCount > 0) contentContainer.Clear();

                if (value != null) contentContainer.Add(value);
            }
        }

        public override bool CanReconcile(Widget updated) {
            return updated is T;
        }

        public override bool Reconcile(Widget updated) {
            if (updated is not T widget) return false;
            var previous = TypedDescriptor;
            Apply(previous, widget);
            Modifier.ApplyDelta(previous, updated, this);
            try {
                Descriptor = updated;
                var child = GetChildFromWidget(previous, widget);
                DefaultReconciler.ReconcileSingle(this, child, this);
            } catch {
                Descriptor = previous;
                throw;
            }

            return true;
        }

        public virtual void Apply(T previous, T widget) { }
        protected abstract Widget GetChildFromWidget(T previous, T widget);

        public override List<DiagnosticsNode> DebugDescribeChildren() {
            var list = new List<DiagnosticsNode>();
            var child = DefaultReconciler.ExpandElement(Child);
            if (child == null) return list;
            list.Add(child.ToDiagnosticsNodeSafe());
            return list;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using HELIX.Abstractions;
using HELIX.Widgets.Diagnostics;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
    public abstract class MultiChildWidgetBaseElement<T> : WidgetBaseElement<T>, IMultiChildContainer,
        IWidgetElementCollection where T : MultiChildWidget {
        public virtual IEnumerable<VisualElement> Childs {
            get => Children();
            set {
                Clear();
                if (value == null) return;
                foreach (var child in value) Add(child);
            }
        }

        public virtual void LoadWidgetElements(List<IWidgetElement> elements) {
            new HierarchyDescriptionCollection(contentContainer).LoadWidgetElements(elements);
        }

        public virtual void UpdateWidgetElements(IWidgetElement[] result, ReconcilerCollectionDelta[] deltas) {
            new HierarchyDescriptionCollection(contentContainer).UpdateWidgetElements(result, deltas);
        }

        public override bool CanReconcile(Widget updated) {
            return updated is T;
        }

        public override bool Reconcile(Widget updated) {
            if (updated is not T widget) return false;
            var previous = TypedDescriptor;
            Apply(previous, widget);
            Modifier.ApplyDelta(Descriptor, updated, this);
            Descriptor = updated;
            Reconciler.ReconcileCollection(this, widget.children, this);
            return true;
        }

        public virtual void Apply(T previous, T widget) { }

        public override List<DiagnosticsNode> DebugDescribeChildren() {
            return new HierarchyDescriptionCollection(contentContainer).Elements
                .Select(x => x.ToDiagnosticsNodeSafe())
                .ToList();
        }
    }
    
    public interface IPreferExplicitFlex {}
}
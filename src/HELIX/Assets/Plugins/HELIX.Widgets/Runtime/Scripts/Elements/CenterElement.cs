using HELIX.Extensions;
using HELIX.Widgets.Universal;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
    [UxmlElement]
    public partial class CenterElement : SingleChildContainerElement, IWidgetElement {
        public CenterElement() {
            this.FlexContainer(mainAxisAlign: Justify.Center, crossAxisAlign: Align.Center);
        }

        public VisualElement Element => this;
        public Widget Descriptor { get; set; }

        public bool CanReconcile(Widget updated) {
            return updated is FlexCenter;
        }

        public bool Reconcile(Widget updated) {
            if (updated is not FlexCenter fa) return false;
            DefaultReconciler.ReconcileSingleDirect(this, fa.child);
            Modifier.ApplyDelta(Descriptor, updated, this);
            Descriptor = updated;
            return true;
        }
    }
}
using System.Diagnostics;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace HELIX.Widgets.Descriptors {
    public abstract class BaseHostWidgetElement : BaseElement {
        public Widget Hosted { get; set; }

        public void ReconcileHost() {
            HierarchyModificationBarrier.Run(() => {
                    try {
                        DefaultReconciler.ReconcileSingleDirect(this, Hosted); //
                    } catch (System.Exception e) { Debug.LogError($"Error reconciling host widget: {e}"); }
                }
            );
        }
    }

    public class HostWidgetElement : BaseHostWidgetElement {
        protected override void OnAttached(AttachToPanelEvent evt) {
            base.OnAttached(evt);
            var stopwatch = Stopwatch.StartNew();
            ReconcileHost();
            stopwatch.Stop();
            Debug.Log($"Reconciled host widget in {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
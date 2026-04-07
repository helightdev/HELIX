using System;
using System.Diagnostics;
using HELIX.Widgets.Elements;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace HELIX.Widgets {
    public abstract class BaseHostWidgetElement : BaseElement {
        public Widget Hosted { get; set; }

        public void ReconcileHost() {
            ModificationBarrier.Run(() => {
                    try {
                        DefaultReconciler.ReconcileSingleDirect(this, Hosted); //
                    } catch (System.Exception e) { Debug.LogError($"Error reconciling host widget: {e}"); }
                }
            );
        }
    }

    public class HostWidgetElement : BaseHostWidgetElement {
        private bool _hasState;
        
        protected override void OnAttached(AttachToPanelEvent evt) {
            base.OnAttached(evt);
            try {
                var stopwatch = Stopwatch.StartNew();
                Profiler.BeginSample("HostWidgetElement.ReconcileHost");
                ReconcileHost();
                Profiler.EndSample();
                stopwatch.Stop();
                _hasState = true;
            } catch (System.Exception e) {
                Debug.LogError($"Disposing after error while reconciling host widget on attach: {e}");
                ModificationBarrier.Run(Clear);
            }
        }

        protected override void OnDetached(DetachFromPanelEvent evt) {
            if (ModificationBarrier.InsideModification || !_hasState) {
                return;
            } 
            // TODO: Maybe move into a dispose, need to test if this breaks things
            
            _hasState = false;
            userData = null;
            ModificationBarrier.Run(Clear);
        }
    }
}
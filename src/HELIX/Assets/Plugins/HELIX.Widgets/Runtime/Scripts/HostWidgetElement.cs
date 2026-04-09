using System;
using HELIX.Widgets.Elements;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public class HostWidgetElement : BuildingWidgetBaseElement<HostWidgetElement.RootWidget> {
        private bool _hasState;

        public HostWidgetElement() {
            Descriptor = RootWidget.Instance;
        }

        public IBuildable Buildable { get; set; }

        protected override void OnAttached(AttachToPanelEvent evt) {
            base.OnAttached(evt);
            ModificationBarrier.RunRebuild(this);

            // try {
            //     var stopwatch = Stopwatch.StartNew();
            //     Profiler.BeginSample("HostWidgetElement.ReconcileHost");
            //     ReconcileHost();
            //     Profiler.EndSample();
            //     stopwatch.Stop();
            //     _hasState = true;
            // } catch (System.Exception e) {
            //     Debug.LogError($"Disposing after error while reconciling host widget on attach: {e}");
            //     ModificationBarrier.Run(Clear);
            // }
        }

        protected override void OnDetached(DetachFromPanelEvent evt) {
            if (ModificationBarrier.InsideModification || !_hasState) return;
            // TODO: Maybe move into a dispose, need to test if this breaks things

            _hasState = false;
            userData = null;
            ModificationBarrier.Run(Clear);
        }

        protected override IBuildable GetBuildableForWidget(RootWidget previous, RootWidget widget) {
            return Buildable;
        }

        public class RootWidget : Widget {
            public static readonly RootWidget Instance = new();

            private RootWidget() { }

            public override IWidgetElement CreateElement() {
                throw new NotImplementedException();
            }

            public override string GetWidgetName() {
                return "<root>";
            }
        }
    }
}
using System;
using HELIX.Widgets.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public class HostWidgetElement : BuildingWidgetBaseElement<HostWidgetElement.WidgetType> {
        private bool _hasState;

        public HostWidgetElement() {
            Descriptor = RootWidget.Instance;
        }

        public IBuildable Buildable { get; set; }
        

        protected override void OnAttached(AttachToPanelEvent evt) {
            base.OnAttached(evt);
            
            Debug.Log("HostWidgetElement attached to panel, determining context...");
            
            var nearestWidget = GetFirstAncestorOfType<IWidgetElement>();
            if (nearestWidget == null) {
                Descriptor = RootWidget.Instance;
                ParentContext = null;
            } else {
                ParentContext = nearestWidget;
                Descriptor = GapWidget.Instance;
            }
            
            
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
            ParentContext = null;
            Descriptor = null;
            ModificationBarrier.Run(Clear);
        }

        protected override IBuildable GetBuildableForWidget(WidgetType previous, WidgetType widget) {
            return Buildable;
        }

        public abstract class WidgetType : Widget {
            
            public override IWidgetElement CreateElement() {
                throw new NotImplementedException();
            }
        } 

        public class RootWidget : WidgetType {
            public static readonly RootWidget Instance = new();

            private RootWidget() { }
            
            public override string GetWidgetName() {
                return "<root>";
            }
        }

        public class GapWidget : WidgetType {
            public static readonly GapWidget Instance = new();

            private GapWidget() { }

            public override string GetWidgetName() {
                return "<gap>";
            }
        }
    }
}
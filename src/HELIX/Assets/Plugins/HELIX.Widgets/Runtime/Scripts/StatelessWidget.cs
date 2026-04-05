using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class StatelessWidget<T> : Widget where T : StatelessWidget<T> {
        public abstract Widget Build(BuildContext context);

        public override IWidgetElement CreateElement() {
            var element = new StatelessWidgetElement { Descriptor = this };
            return element;
        }

        public class StatelessWidgetElement : BaseHostWidgetElement, IWidgetElement {
            public VisualElement Element => this;
            public Widget Descriptor { get; set; }
            public bool firstPaint = true;

            public bool Reconcile(Widget updated) {
                if (updated is not T widget) return false;
                if (parent == null)
                    throw new InvalidOperationException(
                        "StatelessWidget's element must be attached to the hierarchy before it can be reconciled."
                    );
                try {
                    ModificationBarrier.RemoveRebuild(this);
                    Hosted = widget.Build(new BuildContext(this));
                    ReconcileHost();
                } catch (Exception e) { Debug.LogError($"Error building widget: {e}"); }

                try {
                    Modifier.ApplyDelta(firstPaint ? null : Descriptor, widget, this); //
                } catch (Exception e) { Debug.LogError($"Error applying delta: {e}"); }

                Descriptor = widget;
                firstPaint = false;
                return true;
            }

            protected override void OnAttached(AttachToPanelEvent evt) {
                base.OnAttached(evt);
                if (Descriptor != null) Reconcile(Descriptor);
            }

            protected override void OnThemeUpdated() {
                if (Descriptor != null) { ModificationBarrier.RunRebuild(this); }
            }
        }
    }
}
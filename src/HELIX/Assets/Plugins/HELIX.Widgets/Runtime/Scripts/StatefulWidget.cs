using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class StatefulWidget<T> : Widget where T : StatefulWidget<T> {
        public abstract WidgetState<T> CreateState();

        public override IWidgetElement CreateElement() {
            var element = new StatefulWidgetElement { Descriptor = this };
            return element;
        }

        public class StatefulWidgetElement : BaseHostWidgetElement, IWidgetElement, IHierarchyDisposable {
            public VisualElement Element => this;
            public Widget Descriptor { get; set; }
            public WidgetState<T> State { get; set; }
            public bool firstPaint = true;
            public bool isDisposed = false;
            private bool isBuilding = false;

            public bool Reconcile(Widget updated) {
                if (isDisposed) throw new ObjectDisposedException(nameof(StatefulWidget<T>));
                if (updated is not T widget) return false;
                if (parent == null)
                    throw new InvalidOperationException(
                        "StatefulWidget's element must be attached to the hierarchy before it can be reconciled."
                    );

                if (State == null) {
                    State = widget.CreateState();
                    State.widget = widget;
                    State.mount = new BuildContext(this);
                    State.setStateAction = UserScheduleRebuild;
                    try {
                        State.InitState(); //
                    } catch (Exception e) { Debug.LogError($"Error while initializing stateful widget state: {e}"); }
                } else {
                    var oldWidget = State.widget;
                    State.widget = widget;
                    try {
                        var result = State.CanReconcile(oldWidget);
                        if (!result) return false;
                    } catch (Exception e) { Debug.LogError($"Error while checking if state can reconcile: {e}"); }

                    try {
                        State.DidUpdateWidget(oldWidget); //   
                    } catch (Exception e) { Debug.LogError($"Error while updating widget: {e}"); }
                }

                try {
                    Modifier.ApplyDelta(firstPaint ? null : Descriptor, widget, this); //
                } catch (Exception e) { Debug.LogError($"Error applying delta: {e}"); }

                Descriptor = widget;
                firstPaint = false;
                SetState();
                return true;
            }

            public void SetState() {
                if (isDisposed) throw new ObjectDisposedException(nameof(StatefulWidget<T>));
                try {
                    isBuilding = true;
                    ModificationBarrier.RemoveRebuild(this);
                    Hosted = State.Build(State.mount);
                    ReconcileHost();
                } catch (Exception e) { Debug.LogError($"Error building stateful widget: {e}"); } finally {
                    isBuilding = false;
                }
            }

            protected override void OnAttached(AttachToPanelEvent evt) {
                base.OnAttached(evt);
                if (Descriptor != null) Reconcile(Descriptor);
            }

            public void UserScheduleRebuild() {
                if (isBuilding) {
                    Debug.LogWarning("setState was called during build. This can lead to unexpected behavior. Ignoring setState call.");
                } else {
                    ModificationBarrier.RunRebuild(this);
                }
            }

            public void Rebuild() {
                if (isDisposed) return;
                SetState();
            }

            protected override void OnThemeUpdated() {
                if (isDisposed) return;
                if (Descriptor == null) return;
                ModificationBarrier.RunRebuild(this);
            }

            public void Dispose() {
                if (isDisposed) return;
                State?.Dispose();
                isDisposed = true;
            }
        }
    }

    public abstract class WidgetState<T> where T : StatefulWidget<T> {
        public T widget;
        public BuildContext mount;
        internal Action setStateAction;

        public virtual void InitState() { }

        public virtual bool CanReconcile(T oldWidget) => true;
        public virtual void DidUpdateWidget(T oldWidget) { }
        public virtual void Dispose() { }

        public abstract Widget Build(BuildContext context);

        public void SetState() => setStateAction?.Invoke();

        public Action SetState(Action action) =>
            () => {
                action?.Invoke();
                setStateAction?.Invoke();
            };
    }
}
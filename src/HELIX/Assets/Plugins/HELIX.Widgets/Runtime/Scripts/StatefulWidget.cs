using System;
using System.Collections.Generic;
using HELIX.Widgets.Signals;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets {
    public abstract class StatefulWidget<T> : Widget where T : StatefulWidget<T> {
        public abstract WidgetState<T> CreateState();

        public override IWidgetElement CreateElement() {
            var element = new StatefulWidgetElement<T> { Descriptor = this };
            return element;
        }
    }

    public class StatefulWidgetElement<T> : BaseHostWidgetElement, IWidgetElement, IHierarchyDisposable
        where T : StatefulWidget<T> {
        public VisualElement Element => this;
        public Widget Descriptor { get; set; }
        public WidgetState<T> State { get; set; }
        public bool firstPaint = true;
        public bool isDisposed = false;
        private bool _isBuilding = false;

        public bool CanReconcile(Widget updated) {
            try {
                if (isDisposed) return false;
                if (updated is not T widget) return false;
                return State?.CanReconcile(widget) ?? true;
            } catch (Exception e) {
                Debug.LogError($"Error while checking if state can reconcile: {e}");
                return false;
            }
        }

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
                State.dependencyTracker = new SignalDependencyTracker(OnDependencyUpdated);
                State.setStateAction = UserScheduleRebuild;
                try {
                    State.InitState(); //
                } catch (Exception e) { Debug.LogError($"Error while initializing stateful widget state: {e}"); }
            } else {
                var oldWidget = State.widget;
                State.widget = widget;

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
                _isBuilding = true;
                ModificationBarrier.RemoveRebuild(this);
                Widget widget = null;
                State.dependencyTracker.RunBuild(() => { widget = State.Build(State.mount); });
                Hosted = widget;
                ReconcileHost();
            } catch (Exception e) { Debug.LogError($"Error building stateful widget: {e}"); } finally {
                _isBuilding = false;
            }
        }

        protected override void OnAttached(AttachToPanelEvent evt) {
            base.OnAttached(evt);
            if (Descriptor != null) Reconcile(Descriptor);
        }

        public void UserScheduleRebuild() {
            if (_isBuilding) {
                Debug.LogWarning(
                    "setState was called during build. This can lead to unexpected behavior. Ignoring setState call."
                );
            } else { ModificationBarrier.RunRebuild(this); }
        }

        public void Rebuild() {
            if (isDisposed) return;
            SetState();
        }

        protected override void OnThemeUpdated() {
            OnDependencyUpdated();
        }

        private void OnDependencyUpdated() {
            if (isDisposed) return;
            if (Descriptor == null) return;
            ModificationBarrier.RunRebuild(this);
        }

        public void Dispose() {
            if (isDisposed) return;
            State?.managedDisposables?.ForEach(disposable => {
                    try { disposable.Dispose(); } catch (Exception e) {
                        Debug.LogError($"Error while disposing managed disposable: {e}");
                    }
                }
            );
            State?.Dispose();
            State?.dependencyTracker?.Dispose();
            State = null;
            isDisposed = true;
            ModificationBarrier.RemoveRebuild(this);
        }
    }

    public abstract class WidgetState<T> where T : StatefulWidget<T> {
        public T widget;
        public BuildContext mount;
        public SignalDependencyTracker dependencyTracker;
        internal List<IDisposable> managedDisposables;
        internal Action setStateAction;

        protected S AddDisposable<S>(S disposable) where S : IDisposable {
            managedDisposables ??= new List<IDisposable>();
            managedDisposables.Add(disposable);
            return disposable;
        }

        protected void RemoveDisposable<S>(S disposable) where S : IDisposable {
            managedDisposables?.Remove(disposable);
        }

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
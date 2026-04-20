using System;
using System.Collections;
using System.Collections.Generic;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Signals;
using HELIX.Widgets.Theming;
using UnityEngine;

namespace HELIX.Widgets {
    public interface IStatefulWidget { }

    public abstract class StatefulWidget<T> : Widget, IStatefulWidget where T : StatefulWidget<T> {

        protected StatefulWidget() {
            AddModifier(ModifierFallbacks.ImplicitFlexFill);
        }

        protected StatefulWidget(
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(key, constants) {
            DefaultModifiers(ModifierSet.DefaultFlexFill, modifiers);
        }

        public abstract State<T> CreateState();

        public override IWidgetElement CreateElement() {
            return ReconcileInto(new StatefulWidgetElement<T>());
        }

    }

    public abstract class SingleChildStatefulWidget<T> : StatefulWidget<T>, IEnumerable<Widget>
        where T : SingleChildStatefulWidget<T> {

        public Widget child;

        protected SingleChildStatefulWidget(
            Widget child = null,
            Key key = default,
            object[] constants = null,
            IReadOnlyCollection<Modifier> modifiers = null
        ) : base(key, constants, modifiers) {
            this.child = child;
        }

        public IEnumerator<Widget> GetEnumerator() {
            if (child != null) yield return child;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(Widget widget) {
            if (child != null)
                throw new InvalidOperationException("SingleChildStatefulWidget can only have one child.");
            child = widget;
        }

    }

    public class StatefulWidgetElement<T> : BuildingWidgetBaseElement<T>, IHierarchyDisposable, IStatefulWidget
        where T : StatefulWidget<T> {

        public bool isDisposed;
        public State<T> State { get; set; }

        public void Dispose() {
            if (isDisposed) return;
            State?.managedDisposables?.ForEach(disposable => {
                    try {
                        disposable.Dispose();
                    } catch (Exception e) {
                        HelixDiagnostics.Build(
                            "An error occured while disposing a stateful widget state managed disposable.",
                            collector => collector
                                .AddRange(new ErrorSpacer(), new ErrorProperty("The disposable is", disposable))
                                .OwnerChain(this).OffendingElement(this),
                            e
                        ).Report(DiagnosticLevel.Error);
                    }
                }
            );
            try {
                State?.Dispose();
            } catch (Exception e) {
                HelixDiagnostics.Build(
                    "An error occured while disposing a stateful widget state.",
                    collector => collector.OwnerChain(this).OffendingElement(this),
                    e
                ).Report(DiagnosticLevel.Error);
            }

            State?.dependencyTracker?.Dispose();
            State = null;
            isDisposed = true;
            ModificationBarrier.RemoveRebuild(this);
        }

        public override bool CanReconcile(Widget updated) {
            try {
                if (isDisposed) return false;
                if (updated is not T widget) return false;
                return State?.CanReconcile(widget) ?? true;
            } catch (Exception e) {
                Debug.LogError($"Error while checking if state can reconcile: {e}");
                return false;
            }
        }

        public override bool Reconcile(Widget updated) {
            if (isDisposed) throw new ObjectDisposedException(nameof(StatefulWidget<T>));
            return base.Reconcile(updated);
        }

        public void UserScheduleRebuild() {
            if (IsBuilding)
                HelixDiagnostics.Build(
                    "SetState was called during the build phase of a stateful widget.",
                    "Calling SetState during building is not allowed.",
                    hints: new DiagnosticsNode[] {
                        new ErrorHint(
                            "Consider calling SetState in response to user interactions, " +
                            "lifecycle events, or asynchronous operations instead of during the build phase."
                        ),
                        new ErrorHint("Consider using listeners or signals for value driven state management")
                    }
                ).Report(DiagnosticLevel.Warning);
            else ModificationBarrier.Rebuild(this);
        }

        protected override void OnWatchedThemeUpdated(ThemeProperty property, object value) {
            base.OnWatchedThemeUpdated(property, value);
            OnDependencyUpdated();
        }

        public override S GetThemed<S>(BaseThemeProperty<S> property, bool listen = true) {
            return listen ? ThemeValue(property).Value : ThemeProviderElement.Resolve(ThemeProviderElement, property);
        }

        public override bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true) {
            if (listen) ThemeValue(property);
            return ThemeProviderElement.TryResolve(ThemeProviderElement, property, out value);
        }

        private void OnDependencyUpdated() {
            if (isDisposed) return;
            if (Descriptor == null) return;
            ModificationBarrier.Rebuild(this);
        }

        protected override IBuildable GetBuildableForWidget(T previous, T widget) {
            return State;
        }

        public override void BeforeBuild(T previous, T widget) {
            base.BeforeBuild(previous, widget);
            if (State == null) {
                State = widget.CreateState();
                State.widget = widget;
                State.mount = this;
                State.dependencyTracker = new SignalDependencyTracker(OnDependencyUpdated) { owner = this };
                State.setStateAction = UserScheduleRebuild;
                try {
                    State.InitState(); //
                } catch (Exception e) {
                    HelixDiagnostics.Build(
                        "An error occured while initializing a stateful widget state.",
                        collector => collector.OwnerChain(this).OffendingWidget(widget),
                        e
                    ).Report(DiagnosticLevel.Error);
                }
            } else {
                var oldWidget = State.widget;
                State.widget = widget;

                try {
                    State.DidUpdateWidget(oldWidget); //   
                } catch (Exception e) {
                    HelixDiagnostics.Build(
                        "An error occured while updating a stateful widget state.",
                        collector => collector.OwnerChain(this)
                            .OffendingWidget(widget)
                            .AddRange(new ErrorSpacer(), new ErrorProperty("The previous widget was", previous)),
                        e
                    ).Report(DiagnosticLevel.Error);
                }
            }
        }

        public override Widget Build(IBuildable buildable, T previous, T widget) {
            Widget inner = null;
            State.dependencyTracker.RunBuild(() => { inner = base.Build(buildable, previous, widget); });
            return inner;
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<State<T>>("state", State));
        }

        public override string ToStringShort() {
            return $"{typeof(T).Name}:Element#{this.ShortHash()}";
        }

    }

    public abstract class State<T> : DiagnosticableBase, IBuildable where T : StatefulWidget<T> {

        public SignalDependencyTracker dependencyTracker;
        internal List<IDisposable> managedDisposables;
        public BuildContext mount;
        internal Action setStateAction;
        public T widget;

        public abstract Widget Build(BuildContext context);

        protected S AddDisposable<S>(S disposable) where S : IDisposable {
            managedDisposables ??= new List<IDisposable>();
            managedDisposables.Add(disposable);
            return disposable;
        }

        protected void RemoveDisposable<S>(S disposable) where S : IDisposable {
            managedDisposables?.Remove(disposable);
        }

        public virtual void InitState() { }

        public virtual bool CanReconcile(T oldWidget) {
            return true;
        }

        public virtual void DidUpdateWidget(T oldWidget) { }

        public virtual void Dispose() { }

        public void SetState() {
            setStateAction?.Invoke();
        }

        public Action SetState(Action action) {
            return () => {
                action?.Invoke();
                setStateAction?.Invoke();
            };
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(
                new IterableProperty<object>(
                    "dependencies",
                    dependencyTracker.dependencies.Keys,
                    level: DiagnosticLevel.Debug
                )
            );
        }

    }
}
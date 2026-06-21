using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using HELIX.Diagnostics;
using HELIX.Diagnostics.Error;
using HELIX.Diagnostics.Properties;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Elements;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Signals;
using HELIX.Widgets.Theming;
using HELIX.Widgets.Utilities;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace HELIX.Widgets {
  public interface IStatefulWidget { }

  /// <summary>
  /// <para>Base class of widgets that maintain state.</para>
  /// <para>
  /// The behavior of a StatefulWidget is defined by its <see cref="State{T}"/> object.
  /// </para>
  /// </summary>
  public abstract class StatefulWidget<T> : Widget, IStatefulWidget where T : StatefulWidget<T> {
    /// <seealso cref="ModifierFallbacks.ImplicitFlexFill"/>
    protected StatefulWidget(
      Key key = default,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key) {
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


    /// <inheritdoc/>
    protected SingleChildStatefulWidget(
      Widget child = null,
      Key key = default,
      IReadOnlyCollection<Modifier> modifiers = null
    ) : base(key, modifiers) {
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

  // ReSharper disable once InconsistentNaming
  public interface StateBuildContext : BuildContext, IPossiblyDisposed {
    bool IsMounted { get; }
    bool IsBuilding { get; }

    void ScheduleRebuild(bool isImplicit = false);
  }


  // ReSharper disable once InconsistentNaming
  public interface StateBuildContext<T> : StateBuildContext where T : StatefulWidget<T> {
    State<T> State { get; }
  }

  public class StatefulWidgetElement<T>
    : BuildingWidgetBaseElement<T>, IHierarchyDisposable, IStatefulWidget, StateBuildContext<T>
    where T : StatefulWidget<T> {
    private static readonly string _ussName = $"{typeof(T).Name}Element";

    public bool IsDisposed { get; private set; }
    public State<T> State { get; private set; }

    public bool IsMounted => !IsDisposed && State != null && Descriptor != null;

    public void ScheduleRebuild(bool isImplicit = false) {
      if (IsBuilding) {
        if (!isImplicit) {
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
        }
        return;
      }
      ModificationBarrier.Rebuild(this);
    }

    public void Dispose() {
      if (IsDisposed) return;
      var managedDisposables = State?.managedDisposables;
      if (managedDisposables != null) {
        foreach (var disposable in managedDisposables) {
          try { disposable.Dispose(); } catch (Exception e) {
            HelixDiagnostics.Build(
              "An error occured while disposing a stateful widget state managed disposable.",
              collector => collector
                .AddRange(new ErrorSpacer(), new ErrorProperty("The disposable is", disposable))
                .OwnerChain(this).OffendingElement(this),
              e
            ).Report(DiagnosticLevel.Error);
          }
        }
      }

      var observerFunctions = State?.functionObservers;
      if (observerFunctions != null) {
        foreach (var entry in observerFunctions) {
          try { entry.observer.Dispose(); } catch (Exception e) {
            HelixDiagnostics.Build(
              "An error occured while disposing a stateful widget state observer.",
              collector => collector
                .AddRange(new ErrorSpacer(), new ErrorProperty("The observer is", entry))
                .OwnerChain(this).OffendingElement(this),
              e
            ).Report(DiagnosticLevel.Error);
          }
        }
        observerFunctions.Clear();
      }

      try { State?.Dispose(); } catch (Exception e) {
        HelixDiagnostics.Build(
          "An error occured while disposing a stateful widget state.",
          collector => collector.OwnerChain(this).OffendingElement(this),
          e
        ).Report(DiagnosticLevel.Error);
      }

      State?.dependencyTracker?.Dispose();
      State = null;
      IsDisposed = true;
      ModificationBarrier.RemoveRebuild(this);
    }

    public override bool CanReconcile(Widget updated) {
      try {
        if (IsDisposed) return false;
        if (updated is not T widget) return false;
        return State?.CanReconcile(widget) ?? true;
      } catch (Exception e) {
        Debug.LogError($"Error while checking if state can reconcile: {e}");
        return false;
      }
    }

    public override bool Reconcile(Widget updated) {
      if (IsDisposed) throw new ObjectDisposedException(nameof(StatefulWidget<T>));
      name = _ussName;
      return base.Reconcile(updated);
    }

    public void UserScheduleRebuild() {
      if (IsBuilding) {
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
      } else ModificationBarrier.Rebuild(this);
    }

    protected override void OnWatchedThemeUpdated(ThemeProperty property, object value) {
      base.OnWatchedThemeUpdated(property, value);
      OnDependencyUpdated();
    }

    public override S GetThemed<S>(BaseThemeProperty<S> property, bool listen = true) {
      return listen ? ThemeValue(property).Value : ThemeProviderNode.Resolve(ThemeProvider, property);
    }

    public override bool TryGetThemed<S>(BaseThemeProperty<S> property, out S value, bool listen = true) {
      if (listen) ThemeValue(property);
      return ThemeProviderNode.TryResolve(ThemeProvider, property, out value);
    }

    private void OnDependencyUpdated() {
      if (IsDisposed) return;
      if (Descriptor == null) return;
      if (IsBuilding)
        return; // This can technically be a "warning" but mostly it's not a problem since effects are obvious
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
        try {
          State.InitState(); //
        } catch (Exception e) {
          HelixDiagnostics.Build(
            "An error occured while initializing a stateful widget state.",
            collector => collector.OwnerChain(this).OffendingWidget(widget),
            e
          ).Report(DiagnosticLevel.Error);
        }

        try {
          State.Configure(
            new State<T>.ConfigureContext {
              state = State,
              old = widget,
              initial = true
            }
          ); //
        } catch (Exception e) {
          HelixDiagnostics.Build(
            "An error occured while configuring the initial widget state.",
            collector => collector.OwnerChain(this)
              .OffendingWidget(widget)
              .AddRange(new ErrorSpacer()),
            e
          ).Report(DiagnosticLevel.Error);
        }
      } else {
        var oldWidget = State.widget;
        if (ReferenceEquals(oldWidget, widget)) return; // Technically optimizes fully constant rebuild invocations
        State.widget = widget;

        try {
          State.DidUpdateWidget(oldWidget); //
          State.Configure(
            new State<T>.ConfigureContext {
              state = State,
              old = oldWidget,
              initial = false
            }
          ); //
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

  /// <summary>
  /// <para>
  /// The state of a <see cref="StatefulWidget{T}"/>. It holds its widget's internal state and is used to
  /// update the UI when the widget configuration changes.
  /// </para>
  /// <para>
  /// Once the state is created, a call to <see cref="InitState"/> will be made immediately before the first
  /// <see cref="Build"/> call.
  /// </para>
  /// <para>
  /// The state maintains a list of managed disposables that will be disposed when the state is disposed and also
  /// provides a <see cref="Dispose"/> callback method.
  /// </para>
  /// <para>
  /// <see cref="Signal"/> values can be directly accessed from within the state's <see cref="Build"/> methods and
  /// dependencies will be automatically tracked by the <see cref="SignalDependencyTracker"/>.
  /// </para>
  /// <para>
  /// Once any dependency changes or <see cref="SetState()"/> gets called, a <see cref="ModificationBarrier.Rebuild"/>
  /// will be scheduled. Once the <see cref="Reconciler"/> rebuilds the object, <see cref="CanReconcile"/> will be
  /// called to determine if the state can be reused with the new widget configuration. If it returns true,
  /// <see cref="DidUpdateWidget"/> will be called to notify the state of the new widget configuration before the
  /// <see cref="Build"/> method is called again.
  /// </para>
  /// </summary>
  public abstract class State<T> : DiagnosticableBase, IBuildable where T : StatefulWidget<T> {
    internal List<IDisposable> managedDisposables;
    internal List<SateObserverFunctionEntry> functionObservers;

    /// <summary>
    /// The <see cref="SignalDependencyTracker"/> that tracks signal dependencies for this state.
    /// </summary>
    public SignalDependencyTracker dependencyTracker;

    /// <summary>
    /// The <see cref="IWidgetElement"/> that this state is mounted to. This context will never change and can be used
    /// to access the <see cref="VisualElement"/> of the widget. Will also always be equal to the <c>context</c> passed
    /// into the <see cref="Build"/> method.
    /// </summary>
    public StateBuildContext<T> mount;

    /// <summary>
    /// The widget configuration that is currently bound to this state.
    /// </summary>
    public T widget;

    public abstract Widget Build(BuildContext context);

    /// <summary>
    /// Tracks a disposable dependency that will automatically be disposed when the state is disposed.
    /// </summary>
    protected S AddDisposable<S>(S disposable) where S : IDisposable {
      managedDisposables ??= new List<IDisposable>();
      managedDisposables.Add(disposable);
      return disposable;
    }

    /// <summary>
    /// Removes a disposable dependency from the state's managed disposables.
    /// </summary>
    protected void RemoveDisposable<S>(S disposable) where S : IDisposable {
      managedDisposables?.Remove(disposable);
    }

    /// <summary>
    /// <para>Called immediately before the first <see cref="Build"/> call to initialize the state.</para>
    /// </summary>
    /// <remarks>
    /// Subscriptions and disposables should be created here and then properly disposed in <see cref="Dispose"/> if
    /// not automatically disposed using <see cref="AddDisposable"/>.
    /// </remarks>
    public virtual void InitState() { }

    /// <summary>
    /// <para>Called when the state is about to be rebuilt with a new widget configuration.</para>
    /// </summary>
    /// <param name="oldWidget">
    /// The widget configuration before the rebuild. The current widget configuration is available
    /// through the <see cref="widget"/> property.
    /// </param>
    /// <returns>Whether the state can be reused with the new widget configuration.</returns>
    public virtual bool CanReconcile(T oldWidget) {
      return true;
    }

    /// <summary>
    /// <para>Called when the state is rebuilt with a new widget configuration.</para>
    /// </summary>
    /// <param name="oldWidget">
    /// The widget configuration before the rebuild. The current widget configuration is available
    /// through the <see cref="widget"/> property.
    /// </param>
    public virtual void DidUpdateWidget(T oldWidget) { }

    /// <summary>
    /// Called when the state is rebuilt with a new widget configuration.
    /// </summary>
    public virtual void Configure(ConfigureContext context) { }

    /// Called when the state is disposed.
    public virtual void Dispose() { }

    /// <summary>
    /// Schedules a rebuild of the widget.
    /// </summary>
    public void SetState() {
      mount.ScheduleRebuild();
    }

    /// <summary>
    /// Wraps an action in a tailing <see cref="SetState()"/> call.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public Action SetState(Action action) {
      return () => {
        action?.Invoke();
        mount.ScheduleRebuild();
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

    public ref struct ConfigureContext {
      public State<T> state;
      public T old;
      public bool initial;

      public void MethodObserver(Signal signal, Action handle, bool fireImmediately = false) {
        CheckNonAnonymous(handle);
        state.functionObservers ??= new List<SateObserverFunctionEntry>();
        for (var i = 0; i < state.functionObservers.Count; i++) {
          var entry = state.functionObservers[i];
          if (!Equals(entry.handler, handle)) continue;
          entry.Resubscribe(signal);
          return;
        }
        InsertObserver(signal.AddObserver(handle, fireImmediately), handle);
      }

      public void MethodObserver<V>(Signal<V> signal, Action<V> handle, bool fireImmediately = false) {
        CheckNonAnonymous(handle);
        state.functionObservers ??= new List<SateObserverFunctionEntry>();
        for (var i = 0; i < state.functionObservers.Count; i++) {
          var entry = state.functionObservers[i];
          if (!Equals(entry.handler, handle)) continue;
          entry.Resubscribe(signal);
          return;
        }
        InsertObserver(signal.AddObserver(handle, fireImmediately), handle);
      }

      public void MethodObserver(Signal oldSignal, Signal signal, Action handle, bool fireImmediately = false) {
        CheckNonAnonymous(handle);
        state.functionObservers ??= new List<SateObserverFunctionEntry>();
        for (var i = 0; i < state.functionObservers.Count; i++) {
          var entry = state.functionObservers[i];
          if (!Equals(entry.handler, handle) || entry.signal != oldSignal) continue;
          entry.Resubscribe(signal);
          return;
        }
        InsertObserver(signal.AddObserver(handle, fireImmediately), handle);
      }

      public void MethodObserver<V>(Signal<V> oldSignal, Signal<V> signal, Action<V> handle, bool fireImmediately = false) {
        CheckNonAnonymous(handle);
        state.functionObservers ??= new List<SateObserverFunctionEntry>();
        for (var i = 0; i < state.functionObservers.Count; i++) {
          var entry = state.functionObservers[i];
          if (!Equals(entry.handler, handle) || !ReferenceEquals(entry.signal, oldSignal)) continue;
          entry.Resubscribe(signal);
          return;
        }
        InsertObserver(signal.AddObserver(handle, fireImmediately), handle);
      }

      private void InsertObserver(FunctionSignalObserver observer, Delegate handler) {
        state.functionObservers.Add(
          new SateObserverFunctionEntry {
            observer = observer,
            signal = observer.Current,
            handler = handler
          }
        );
      }

      [Conditional("UNITY_EDITOR")]
      private static void CheckNonAnonymous(Delegate handler) {
        if (handler.Method.Name.StartsWith("<") && handler.Method.Name.Contains("__")) {
          throw new InvalidOperationException(
            $"Anonymous delegates are not supported as signal observers because of unstable identity. " +
            $"Please use a named instance or static method instead."
          );
        }
      }
    }
  }

  public class SateObserverFunctionEntry {
    public Signal signal;
    public FunctionSignalObserver observer;
    public Delegate handler;

    public void Resubscribe(Signal target) {
      observer.Observe(target);
      signal = target;
    }
  }
}
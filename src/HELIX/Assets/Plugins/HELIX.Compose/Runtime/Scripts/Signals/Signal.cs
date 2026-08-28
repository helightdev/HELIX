using System;
using System.Collections.Generic;
using HELIX.Diagnostics;
using HELIX.Prose;
using UnityEngine;
using UnityEngine.Pool;

namespace HELIX.Compose {
  /// <summary>
  ///   <para>A signal represents a piece of reactive state that can be observed for changes.</para>
  ///   <para>
  ///     Signals can be observed by implementing the <see cref="ISignalObserver" /> interface and subscribing to the signal
  ///     using <see cref="AddObserver(ISignalObserver)" />. <see cref="StatefulWidget{T}" />s may also access signals inside
  ///     their build methods, which will automatically subscribe to the signal and rebuild the widget when the signal
  ///     changes.
  ///   </para>
  /// </summary>
  public abstract class Signal : ContextData, IDisposable, IPossiblyDisposed {
    private const int _maxNotificationStackDepth = 16;
    private readonly HashSet<ISignalObserver> _observers = new();
    private int _notificationStackDepth;

    public int contextKey;

    protected Signal(string name = "Signal", Type registeredType = null) {
      detached = true;
      contextKey = ContextKeyData.ClaimAnonymous(
        registeredType ?? typeof(Signal),
        name,
        new WeakReference<object>(this)
      );
    }

    public override void Dispose() {
      if (IsDisposed) return;
      ContextKeyData.ReleaseAnonymous(contextKey);
      contextKey = 0;

      var list = ListPool<ISignalObserver>.Get();
      try {
        list.AddRange(_observers);
        foreach (var observer in list) {
          try {
            observer.OnSignalRemoved(this); //
          } catch (Exception e) {
            throw HelixDiagnostics.ProseError(
              "An error occurred while disposing a signal observer.",
              writeDetails: writer => {
                writer.Property("Observer", observer, Datatypes.Object<ISignalObserver>());
                writer.Property("Signal", this, Datatypes.Object<Signal>());
              },
              exception: e
            );
          }
        }
      } finally {
        ListPool<ISignalObserver>.Release(list);
        IsDisposed = true;
        _observers.Clear();
      }
      DisposeContext();
    }

    public bool IsDisposed { get; private set; }

    protected void NotifyDirty() {
      if (_notificationStackDepth >= _maxNotificationStackDepth) {
        Debug.LogWarning(
          HelixDiagnostics.ProseErrorText(
            "Maximum signal notification stack depth exceeded",
            "This warning indicates that the maximum allowed depth for nested signal notifications has been exceeded. " +
            "This can occur when signals have circular dependencies, causing them to notify each other indefinitely. " +
            "To resolve this issue, review your signal dependencies and ensure that there are no circular references.",
            writer => writer.Property("Signal", this, Datatypes.Object<Signal>()),
            "Check the signals that depend on this signal and ensure that they do not create a circular dependency."
          )
        );
        return;
      }
      IncrementContextVersion();

      _notificationStackDepth++;
      var buffer = ListPool<ISignalObserver>.Get();
      try {
        buffer.AddRange(_observers);
        foreach (var observer in buffer) {
          try {
            if (observer is IPossiblyDisposed { IsDisposed: true }) {
              RemoveObserver(observer);
              continue;
            }

            observer.OnSignalDirty(this); //
          } catch (Exception e) {
            throw HelixDiagnostics.ProseError(
              "An error occurred while notifying a signal observer of a dirty signal.",
              writeDetails: writer => {
                writer.Property("Observer", observer, Datatypes.Object<ISignalObserver>());
                writer.Property("Signal", this, Datatypes.Object<Signal>());
              },
              exception: e
            );
          }
        }
      } finally {
        _notificationStackDepth--;
        ListPool<ISignalObserver>.Release(buffer);
      }
    }

    protected void NotifyObservers() {
      if (_notificationStackDepth >= _maxNotificationStackDepth) {
        Debug.LogWarning(
          HelixDiagnostics.ProseErrorText(
            "Maximum signal notification stack depth exceeded",
            "This warning indicates that the maximum allowed depth for nested signal notifications has been exceeded. " +
            "This can occur when signals have circular dependencies, causing them to notify each other indefinitely. " +
            "To resolve this issue, review your signal dependencies and ensure that there are no circular references.",
            writer => writer.Property("Signal", this, Datatypes.Object<Signal>()),
            "Check the signals that depend on this signal and ensure that they do not create a circular dependency."
          )
        );
        return;
      }

      using (HXComposer.BeginBatch()) SendNotifyObservers();
    }

    private void SendNotifyObservers() {
      var buffer = ListPool<ISignalObserver>.Get();
      try {
        _notificationStackDepth++;
        buffer.AddRange(_observers);
        foreach (var observer in buffer) {
          try { observer.OnSignalChanged(this); } catch (Exception e) {
            throw HelixDiagnostics.ProseError(
              "An error occurred while notifying a signal observer of a changed value.",
              writeDetails: writer => {
                writer.Property("Observer", observer, Datatypes.Object<ISignalObserver>());
                writer.Property("Signal", this, Datatypes.Object<Signal>());
              },
              exception: e
            );
          }
        }
      } finally {
        _notificationStackDepth--;
        ListPool<ISignalObserver>.Release(buffer);
      }
    }

    public virtual bool AddObserver(ISignalObserver observer) {
      var result = _observers.Add(observer);
      if (result) observer.OnSignalAdded(this);
      return result;
    }

    public virtual bool RemoveObserver(ISignalObserver observer) {
      var result = _observers.Remove(observer);
      if (result) observer.OnSignalRemoved(this);
      return result;
    }

    public FunctionSignalObserver AddObserver(Action onChanged, bool fireImmediately = false) {
      var observer = new FunctionSignalObserver(_ => onChanged?.Invoke());
      observer.Observe(this);
      if (fireImmediately) onChanged?.Invoke();
      return observer;
    }

    /// <summary>
    ///   Creates a signal that holds a single value.
    /// </summary>
    /// <param name="initialValue">The initial value of the signal. Defaults to the default value of <typeparamref name="T" />.</param>
    /// <param name="equality">
    ///   If true, the signal will only notify observers when the value changes to a different value
    ///   (as determined by <see cref="EqualityComparer{T}.Default" />). If false, the signal will notify observers
    ///   whenever the value is set, even if it's the same as the current value.
    /// </param>
    public static ValueSignal<T> Value<T>(T initialValue = default, bool equality = true) {
      return new ValueSignal<T>(initialValue, equality);
    }

    /// <summary>
    ///   Creates a signal that recomputes its value whenever any of its dependencies change.
    /// </summary>
    /// <param name="computeFunc">
    ///   A function that computes the value of the signal based on its dependencies.
    ///   This function will be called whenever any of the signal's dependencies change.
    /// </param>
    /// <remarks>
    ///   The current value of a signal will be invalidated on <see cref="Signal.NotifyDirty" /> and
    ///   only recomputed when accessed.
    /// </remarks>
    /// <seealso cref="SignalDependencyTracker" />
    public static ComputedSignal<T> Computed<T>(Func<T> computeFunc) {
      return new ComputedSignal<T>(computeFunc);
    }
  }

  public abstract class Signal<T> : Signal {
    protected Signal(string name = "ValueSignal", Type registeredType = null) : base(
      name,
      registeredType ?? typeof(Signal<T>)
    ) { }

    public T Value {
      get {
        if (HXComposer.CurrentBoundary != null) {
          HXComposer.CurrentBoundary.SubscribeToContextData(contextKey, this);
          return PeekValue();
        }

        var tracker = SignalDependencyTracker.Current;
        tracker?.DependOn(this);
        return PeekValue();
      }
      set {
        var tracker = SignalDependencyTracker.Current;
        if (tracker != null && tracker.IsBuilding && !tracker.IsDisposed)
          throw new InvalidOperationException("Cannot write to a signal during build.");
        SetValue(value);
      }
    }

    public abstract T PeekValue();
    public abstract void SetValue(T newValue);
    public abstract void SetWithoutNotify(T newValue);

    public FunctionSignalObserver AddObserver(Action<T> onChanged, bool fireImmediately = false) {
      var observer = FunctionSignalObserver.Typed(onChanged);
      observer.Observe(this);
      if (fireImmediately) onChanged?.Invoke(PeekValue());
      return observer;
    }

    public static implicit operator T(Signal<T> signal) {
      return signal.Value;
    }
  }
}
using System;
using System.Collections.Generic;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Utilities;
using UnityEngine.Pool;

namespace HELIX.Widgets.Signals {
  /// <summary>
  /// <para>A signal represents a piece of reactive state that can be observed for changes.</para>
  /// <para>
  /// Signals can be observed by implementing the <see cref="ISignalObserver"/> interface and subscribing to the signal
  /// using <see cref="AddObserver(ISignalObserver)"/>. <see cref="StatefulWidget{T}"/>s may also access signals inside
  /// their build methods, which will automatically subscribe to the signal and rebuild the widget when the signal changes.
  /// </para>
  /// </summary>
  public abstract class Signal : DiagnosticableBase, IDisposable, IPossiblyDisposed {
    private const int _maxNotificationStackDepth = 16;
    private readonly HashSet<ISignalObserver> _observers = new();
    private int _notificationStackDepth;

    public virtual void Dispose() {
      if (IsDisposed) return;
      var list = ListPool<ISignalObserver>.Get();
      try {
        list.AddRange(_observers);
        foreach (var observer in list) {
          try {
            observer.OnSignalRemoved(this); //
          } catch (HelixDiagnosticException) { throw; } catch (Exception e) {
            throw HelixDiagnostics.Build(
              "An error occurred while disposing a signal observer.",
              details: new DiagnosticsNode[] {
                new ErrorProperty("The observer is", observer), new ErrorSpacer(),
                new ErrorProperty("The observed signal is", this)
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
    }

    public bool IsDisposed { get; private set; }

    protected void NotifyDirty() {
      if (_notificationStackDepth >= _maxNotificationStackDepth) {
        HelixDiagnostics.Build(
          "Maximum signal notification stack depth exceeded",
          "This warning indicates that the maximum allowed depth for nested signal notifications has been exceeded. " +
          "This can occur when signals have circular dependencies, causing them to notify each other indefinitely. " +
          "To resolve this issue, review your signal dependencies and ensure that there are no circular references.",
          new DiagnosticsNode[] { new ErrorProperty("The signal that triggered this warning is", this) },
          hints: new DiagnosticsNode[] {
            new ErrorHint(
              "Check the signals that depend on this signal and ensure that they do not create a circular dependency."
            )
          }
        ).Report(DiagnosticLevel.Warning);
        return;
      }

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
          } catch (HelixDiagnosticException) { throw; } catch (Exception e) {
            throw HelixDiagnostics.Build(
              "An error occurred while notifying a signal observer of a dirty signal.",
              details: new DiagnosticsNode[] {
                new ErrorProperty("The observer is", observer), new ErrorSpacer(),
                new ErrorProperty("The observed signal is", this)
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
        HelixDiagnostics.Build(
          "Maximum signal notification stack depth exceeded",
          "This warning indicates that the maximum allowed depth for nested signal notifications has been exceeded. " +
          "This can occur when signals have circular dependencies, causing them to notify each other indefinitely. " +
          "To resolve this issue, review your signal dependencies and ensure that there are no circular references.",
          new DiagnosticsNode[] { new ErrorProperty("The signal that triggered this warning is", this) },
          hints: new DiagnosticsNode[] {
            new ErrorHint(
              "Check the signals that depend on this signal and ensure that they do not create a circular dependency."
            )
          }
        ).Report(DiagnosticLevel.Warning);
        return;
      }

      ModificationBarrier.Run(() => {
          var buffer = ListPool<ISignalObserver>.Get();
          try {
            _notificationStackDepth++;
            buffer.AddRange(_observers);
            foreach (var observer in buffer) {
              try { observer.OnSignalChanged(this); } catch (HelixDiagnosticException) { throw; } catch (Exception e) {
                throw HelixDiagnostics.Build(
                  "An error occurred while notifying a signal observer of a changed value.",
                  details: new DiagnosticsNode[] {
                    new ErrorProperty("The observer is", observer), new ErrorSpacer(),
                    new ErrorProperty("The observed signal is", this)
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
      );
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

    public IDisposable AddObserver(Action onChanged, bool fireImmediately = false) {
      var observer = new FunctionSignalObserver(onChanged);
      AddObserver(observer);
      if (fireImmediately) onChanged?.Invoke();
      return observer;
    }

    /// <summary>
    /// Creates a signal that holds a single value.
    /// </summary>
    /// <param name="initialValue">The initial value of the signal. Defaults to the default value of <typeparamref name="T"/>.</param>
    /// <param name="equality">
    /// If true, the signal will only notify observers when the value changes to a different value
    /// (as determined by <see cref="EqualityComparer{T}.Default"/>). If false, the signal will notify observers
    /// whenever the value is set, even if it's the same as the current value.</param>
    public static ValueSignal<T> Value<T>(T initialValue = default, bool equality = true) {
      return new ValueSignal<T>(initialValue, equality);
    }

    /// <summary>
    /// Creates a signal that recomputes its value whenever any of its dependencies change.
    /// </summary>
    /// <param name="computeFunc">
    /// A function that computes the value of the signal based on its dependencies.
    /// This function will be called whenever any of the signal's dependencies change.
    /// </param>
    /// <remarks>
    /// The current value of a signal will be invalidated on <see cref="Signal.NotifyDirty"/> and
    /// only recomputed when accessed.
    /// </remarks>
    /// <seealso cref="SignalDependencyTracker"/>
    public static ComputedSignal<T> Computed<T>(Func<T> computeFunc) {
      return new ComputedSignal<T>(computeFunc);
    }
  }

  public abstract class Signal<T> : Signal {
    public T Value {
      get {
        var tracker = SignalDependencyTracker.Current;
        if (tracker == null) return PeekValue();
        tracker.DependOn(this);
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

    public IDisposable AddObserver(Action<T> onChanged, bool fireImmediately = false) {
      var observer = new FunctionSignalObserver(() => onChanged?.Invoke(PeekValue()));
      AddObserver(observer);
      if (fireImmediately) onChanged?.Invoke(PeekValue());
      return observer;
    }

    public static implicit operator T(Signal<T> signal) {
      return signal.Value;
    }
  }
}
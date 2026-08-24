using System;
using HELIX.Diagnostics;
using HELIX.Prose;
using HELIX.Signals;

namespace HELIX.Widgets.Signals {
  /// <summary>
  /// A signal that recomputes its value whenever any of its dependencies change.
  /// </summary>
  /// <seealso cref="Signal.Computed"/>
  public class ComputedSignal<T> : Signal<T>, ISignalObserver {
    private readonly Func<T> _computeFunc;
    private readonly SignalDependencyTracker _tracker;
    private T _cachedValue;
    private bool _isComputing;
    private bool _isDirty = true;

    public ComputedSignal(Func<T> computeFunc) {
      _computeFunc = computeFunc;
      _tracker = new SignalDependencyTracker(this) { owner = this };
    }

    public void OnSignalDirty(Signal signal) {
      if (_isDirty) return;
      _isDirty = true;
      NotifyDirty();
    }

    public void OnSignalChanged(Signal signal) {
      NotifyObservers();
    }

    public override T PeekValue() {
      if (!_isDirty) return _cachedValue;
      if (_isComputing) {
        throw HelixDiagnostics.ProseError(
          "Circular dependency detected while computing signal value.",
          writeDetails: writer => writer.Property("Signal", this, Datatypes.Object<Signal>()),
          stackTrace: Environment.StackTrace
        );
      }

      try {
        _isComputing = true;
        using (_tracker.BuildScope()) {
          _cachedValue = _computeFunc();
        }
      } catch (Exception ex) {
        throw HelixDiagnostics.ProseError(
          "An error occurred while computing a signal value.",
          writeDetails: writer => writer.Property("Signal", this, Datatypes.Object<Signal>()),
          exception: ex
        );
      } finally { _isComputing = false; }

      _isDirty = false;
      return _cachedValue;
    }

    public override void SetValue(T newValue) {
      throw new NotImplementedException();
    }

    public override void SetWithoutNotify(T newValue) {
      throw new NotImplementedException();
    }

    public override void Dispose() {
      _tracker.Dispose();
      base.Dispose();
    }

  }
}

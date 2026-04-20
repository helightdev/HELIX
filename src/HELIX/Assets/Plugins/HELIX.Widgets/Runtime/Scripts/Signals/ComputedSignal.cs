using System;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Error;
using HELIX.Widgets.Diagnostics.Properties;

namespace HELIX.Widgets.Signals {
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
        throw HelixDiagnostics.Build(
          "Circular dependency detected while computing signal value.",
          details: new DiagnosticsNode[] { new ErrorProperty("The computing signal is", this) },
          stackTrace: Environment.StackTrace
        );
      }

      try {
        _isComputing = true;
        _tracker.RunBuild(() => { _cachedValue = _computeFunc(); });
      } catch (HelixDiagnosticException) { throw; } catch (Exception ex) {
        throw HelixDiagnostics.Build(
          "An error occurred while computing a signal value.",
          details: new DiagnosticsNode[] { new ErrorProperty("The computing signal is", this) },
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

    public override string ToStringShort() {
      return $"ComputedSignal<{typeof(T).Name}>#{this.ShortHash()}";
    }

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new FlagProperty("isDirty", _isDirty, "Dirty"));
      properties.Add(new FlagProperty("isComputing", _isComputing, "Computing", level: DiagnosticLevel.Fine));
      properties.Add(new DiagnosticsProperty<T>("cachedValue", _cachedValue, showName: false));
      properties.Add(
        new DiagnosticsProperty<Func<T>>(
          "computeFunc",
          _computeFunc,
          showName: false,
          level: DiagnosticLevel.Debug
        )
      );
    }
  }
}
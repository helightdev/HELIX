using System;

namespace HELIX.Widgets.Signals {
  public class FunctionSignalObserver : ISignalObserver, IDisposable {
    private readonly Action _onChanged;
    private bool _isDisposed;
    private Signal _lastSignal;

    public FunctionSignalObserver(Action onChanged) {
      _onChanged = onChanged;
    }

    public void Dispose() {
      if (_isDisposed) return;
      _lastSignal?.RemoveObserver(this);
      _isDisposed = true;
    }

    public void OnSignalChanged(Signal signal) {
      if (_isDisposed) return;
      _onChanged?.Invoke();
    }

    public void OnSignalRemoved(Signal signal) {
      if (_isDisposed) return;
      if (ReferenceEquals(signal, _lastSignal)) _lastSignal = null;
    }

    public void OnSignalAdded(Signal signal) {
      if (_isDisposed) return;
      if (_lastSignal == null)
        _lastSignal = signal;
      else if (_lastSignal != signal) {
        _lastSignal.RemoveObserver(this);
        _lastSignal = signal;
      }
    }
  }
}
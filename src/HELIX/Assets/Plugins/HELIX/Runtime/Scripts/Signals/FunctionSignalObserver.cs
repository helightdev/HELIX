using System;
using HELIX.Widgets.Utilities;
using UnityEngine;

namespace HELIX.Widgets.Signals {
  public class FunctionSignalObserver : IDisposable, IPossiblyDisposed, ISignalObserver {
    private readonly Action<Signal>  _onSignalChanged;
    private readonly Action<Signal> _onSignalRemoved;
    private readonly Action<Signal> _onSignalAdded;
    private readonly bool _weak;

    public bool IsDisposed { get; private set; }
    public Signal Current { get; private set; }

    public FunctionSignalObserver(
      Action<Signal> onSignalChanged,
      Action<Signal> onSignalRemoved = null,
      Action<Signal> onSignalAdded = null,
      bool weak = true
    ) {
      _onSignalChanged = onSignalChanged;
      _onSignalRemoved = onSignalRemoved;
      _onSignalAdded = onSignalAdded;
      _weak = weak;
    }

    public static FunctionSignalObserver Typed<T>(
      Action<T> onSignalChanged,
      Action<Signal> onSignalRemoved = null,
      Action<Signal> onSignalAdded = null,
      bool weak = true
    ) {
      return new FunctionSignalObserver(
        signal => {
          if (signal is Signal<T> typedSignal) {
            onSignalChanged(typedSignal.PeekValue());
          }
        },
        onSignalRemoved,
        onSignalAdded,
        weak
      );
    }

    public void Observe(Signal signal) {
      if (Current == signal || signal == null) return;
      if (IsDisposed) throw new ObjectDisposedException(nameof(FunctionSignalObserver));
      Current?.RemoveObserver(this);
      Current = signal;
      if (_weak) {
        signal.AddObserver(new WeakSignalObserver(this));
      } else {
        signal.AddObserver(this);
      }
    }

    public void OnSignalChanged(Signal signal) {
      if (IsDisposed) return;
      _onSignalChanged?.Invoke(signal);
    }

    public void OnSignalRemoved(Signal signal) {
      if (IsDisposed) return;
      _onSignalAdded?.Invoke(signal);
    }

    public void OnSignalAdded(Signal signal) {
      if (IsDisposed) return;
      if (Current == null) {
        Current = signal;
        _onSignalAdded?.Invoke(signal);
        return;
      }

      if (Current != signal) {
        Current?.RemoveObserver(this);
        Current = signal;
        Debug.LogWarning("Current signal changed while observing!");
      }

      _onSignalRemoved?.Invoke(signal);
    }

    public void Dispose() {
      if (IsDisposed) return;
      IsDisposed = true;
      Current?.RemoveObserver(this);
    }
  }
}
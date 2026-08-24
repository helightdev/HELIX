using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HELIX.Diagnostics;
using HELIX.Signals;

namespace HELIX.Widgets.Signals {
  /// <summary>
  /// <para>Provides a mechanism for tracking dependencies between signals.</para>
  /// <para>
  /// When inside the scope of this tracker, all direct value accesses
  /// to a signal will track the signal as a dependency.
  /// </para>
  /// </summary>
  /// <remarks>
  /// This is an older implementation and this will most likely be reworked quite a bit to be more memory efficient
  /// </remarks>
  public class SignalDependencyTracker : ISignalObserver, IDisposable, IPossiblyDisposed {
    public static SignalDependencyTracker Current;
    private readonly ISignalObserver _forwarder;
    private readonly HashSet<Signal> _implicitBuffer = new();
    private readonly Queue<Signal> _removalQueue = new();

    public readonly Dictionary<Signal, SignalDependencyType> dependencies = new();
    public object owner;
    public event Action<Signal> OnDependenciesChanged;

    public SignalDependencyTracker(Action onDependenciesChanged) {
      OnDependenciesChanged += signal => onDependenciesChanged?.Invoke();
    }

    public SignalDependencyTracker(ISignalObserver forwarder) {
      _forwarder = forwarder;
    }

    public bool IsBuilding { get; private set; }

    public void Dispose() {
      if (IsDisposed) return;
      foreach (var signal in dependencies.Keys.ToList()) signal.RemoveObserver(this);

      dependencies.Clear();
      IsDisposed = true;
    }

    public void Clear() {
      IsDisposed = false;
      IsBuilding = false;
      _implicitBuffer.Clear();
      _removalQueue.Clear();
    }

    public bool IsDisposed { get; private set; }

    public void OnSignalChanged(Signal signal) {
      if (IsDisposed) return;
      OnDependenciesChanged?.Invoke(signal);
      _forwarder?.OnSignalChanged(signal);
    }

    public void OnSignalRemoved(Signal signal) {
      if (IsDisposed) return;
      dependencies.Remove(signal);
      _forwarder?.OnSignalRemoved(signal);
    }

    public void OnSignalAdded(Signal signal) {
      if (IsDisposed) return;
      _forwarder?.OnSignalAdded(signal);
    }

    public void OnSignalDirty(Signal signal) {
      if (IsDisposed) return;
      _forwarder?.OnSignalDirty(signal);
    }

    public Scope BuildScope() {
      if (IsDisposed) throw new ObjectDisposedException(nameof(SignalDependencyTracker));
      var previous = Current;
      BeginBuild();
      Current = this;
      return new Scope(this, previous);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RunBuild(Action action) {
      if (IsDisposed) throw new ObjectDisposedException(nameof(SignalDependencyTracker));
      var previous = Current;
      try {
        Current = this;
        BeginBuild();
        action();
      } finally {
        EndBuild();
        Current = previous?.IsDisposed ?? true ? null : previous;
      }
    }

    private void BeginBuild() {
      if (IsDisposed) throw new ObjectDisposedException(nameof(SignalDependencyTracker));
      _implicitBuffer.Clear();
      _removalQueue.Clear();
      IsBuilding = true;
    }

    private void EndBuild() {
      try {
        if (IsDisposed) return;
        foreach (var (signal, value) in dependencies) {
          if (value != SignalDependencyType.Implicit) continue;
          if (_implicitBuffer.Contains(signal)) _implicitBuffer.Remove(signal); //
          else _removalQueue.Enqueue(signal);
        }

        foreach (var remaining in _implicitBuffer) {
          remaining.AddObserver(this);
          dependencies[remaining] = SignalDependencyType.Implicit;
        }

        while (_removalQueue.TryDequeue(out var signal)) {
          dependencies.Remove(signal);
          signal.RemoveObserver(this);
        }
      } finally { IsBuilding = false; }
    }

    public void DependOn(Signal signal) {
      if (Equals(signal, _forwarder)) throw new InvalidOperationException("A signal cannot depend on itself.");

      if (IsDisposed) throw new ObjectDisposedException(nameof(Signal));
      if (IsBuilding) _implicitBuffer.Add(signal);
      else DependOnExplicit(signal);
    }

    public void DependOnExplicit(Signal signal, bool weak = false) {
      if (Equals(signal, _forwarder)) throw new InvalidOperationException("A signal cannot depend on itself.");

      if (dependencies.TryGetValue(signal, out var dependencyType)) {
        if (dependencyType == SignalDependencyType.Explicit) return;
        signal.RemoveObserver(this);
      }

      dependencies[signal] = SignalDependencyType.Explicit;
      if (weak) {
        signal.AddObserver(new WeakSignalObserver(this));
      } else {
        signal.AddObserver(this);
      }
    }

    public readonly struct Scope : IDisposable {
      public readonly SignalDependencyTracker tracker;
      public readonly SignalDependencyTracker previous;

      public Scope(SignalDependencyTracker tracker, SignalDependencyTracker previous) {
        this.tracker = tracker;
        this.previous = previous;
      }

      public void Dispose() {
        tracker.EndBuild();
        Current = previous;
      }
    }
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HELIX.Widgets.Diagnostics;

namespace HELIX.Widgets.Signals {
    public class SignalDependencyTracker : DiagnosticableBase, ISignalObserver, IDisposable {
        public static SignalDependencyTracker Current;
        private readonly ISignalObserver _forwarder;
        private readonly HashSet<Signal> _implicitBuffer = new();
        private readonly Action _onDependenciesChanged;
        private readonly Queue<Signal> _removalQueue = new();

        public readonly Dictionary<Signal, SignalDependencyType> dependencies = new();
        public object owner;

        public SignalDependencyTracker(Action onDependenciesChanged) {
            _onDependenciesChanged = onDependenciesChanged;
        }

        public SignalDependencyTracker(ISignalObserver forwarder) {
            _forwarder = forwarder;
        }

        public bool IsDisposed { get; private set; }
        public bool IsBuilding { get; private set; }

        public void Dispose() {
            if (IsDisposed) return;
            foreach (var signal in dependencies.Keys.ToList()) signal.RemoveObserver(this);

            dependencies.Clear();
            IsDisposed = true;
        }

        public void OnSignalChanged(Signal signal) {
            if (IsDisposed) return;
            _onDependenciesChanged?.Invoke();
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

        public void DependOnExplicit(Signal signal) {
            if (Equals(signal, _forwarder)) throw new InvalidOperationException("A signal cannot depend on itself.");

            if (dependencies.TryGetValue(signal, out var dependencyType)) {
                if (dependencyType == SignalDependencyType.Explicit) return;
                dependencies[signal] = SignalDependencyType.Explicit;
            } else {
                dependencies[signal] = SignalDependencyType.Explicit;
                signal.AddObserver(this);
            }
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<object>("owner", owner, showName: false));
        }
    }
}
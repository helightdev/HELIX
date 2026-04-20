using System;
using System.Runtime.CompilerServices;
using HELIX.Widgets.Utilities;

namespace HELIX.Widgets.Signals {
    public interface ISignalObserver {
        void OnSignalChanged(Signal signal) { }
        void OnSignalRemoved(Signal signal) { }
        void OnSignalAdded(Signal signal) { }
        void OnSignalDirty(Signal signal) { }
    }

    public class WeakSignalObserver : ISignalObserver, IPossiblyDisposed {
        private readonly WeakReference<ISignalObserver> _reference;

        public WeakSignalObserver(ISignalObserver observer) {
            _reference = new WeakReference<ISignalObserver>(observer);
        }

        public bool IsDisposed =>
            !_reference.TryGetTarget(out var observer) ||
            observer is IPossiblyDisposed { IsDisposed: true };

        public void OnSignalChanged(Signal signal) {
            if (_reference.TryGetTarget(out var observer)) observer.OnSignalChanged(signal);
        }

        public void OnSignalRemoved(Signal signal) {
            if (_reference.TryGetTarget(out var observer)) observer.OnSignalRemoved(signal);
        }

        public void OnSignalAdded(Signal signal) {
            if (_reference.TryGetTarget(out var observer)) observer.OnSignalAdded(signal);
        }

        public void OnSignalDirty(Signal signal) {
            if (_reference.TryGetTarget(out var observer)) observer.OnSignalDirty(signal);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) return true;
            var target = _reference.TryGetTarget(out var observer) ? observer : null;
            return target == obj;
        }

        public override int GetHashCode() {
            return RuntimeHelpers.GetHashCode(this);
        }
    }
}
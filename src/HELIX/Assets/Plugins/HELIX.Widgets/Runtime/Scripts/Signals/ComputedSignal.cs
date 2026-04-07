using System;

namespace HELIX.Widgets.Signals {
    public class ComputedSignal<T> : Signal<T>, ISignalObserver {
        private readonly Func<T> _computeFunc;
        private readonly SignalDependencyTracker _tracker;
        private T _cachedValue;
        private bool _isComputing;
        private bool _isDirty = true;

        public ComputedSignal(Func<T> computeFunc) {
            _computeFunc = computeFunc;
            _tracker = new SignalDependencyTracker(this);
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
            if (_isComputing)
                throw new InvalidOperationException("Circular dependency detected while computing signal value.");
            try {
                _isComputing = true;
                _tracker.RunBuild(() => { _cachedValue = _computeFunc(); });
            } finally { _isComputing = false; }

            _isDirty = false;
            return _cachedValue;
        }

        public override void SetValue(T value) {
            throw new NotImplementedException();
        }

        public override void SetWithoutNotify(T value) {
            throw new NotImplementedException();
        }

        public override void Dispose() {
            _tracker.Dispose();
            base.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace HELIX.Widgets.Signals {
    public abstract class Signal : IDisposable {
        private const int _maxNotificationStackDepth = 16;
        private readonly HashSet<ISignalObserver> _observers = new();
        private int _notificationStackDepth;

        public virtual void Dispose() {
            var list = ListPool<ISignalObserver>.Get();
            try {
                list.AddRange(_observers);
                foreach (var observer in list) {
                    try { observer.OnSignalRemoved(this); } catch (Exception e) {
                        Debug.LogError($"Error while disposing signal observer: {e}");
                    }
                }
            } finally { ListPool<ISignalObserver>.Release(list); }

            _observers.Clear();
        }

        protected void NotifyDirty() {
            if (_notificationStackDepth >= _maxNotificationStackDepth) {
                Debug.LogWarning(
                    "Maximum signal notification stack depth exceeded. This may indicate a circular dependency in your signals."
                );
                return;
            }

            try {
                _notificationStackDepth++;
                foreach (var observer in _observers) {
                    try { observer.OnSignalDirty(this); } catch (Exception e) {
                        Debug.LogError($"Error notifying signal observer of dirty state: {e}");
                    }
                }
            } finally { _notificationStackDepth--; }
        }

        protected void NotifyObservers() {
            if (_notificationStackDepth >= _maxNotificationStackDepth) {
                Debug.LogWarning(
                    "Maximum signal notification stack depth exceeded. This may indicate a circular dependency in your signals."
                );
                return;
            }

            ModificationBarrier.Run(() => {
                    var buffer = ListPool<ISignalObserver>.Get();
                    try {
                        _notificationStackDepth++;
                        buffer.AddRange(_observers);
                        foreach (var observer in buffer) {
                            try {
                                observer.OnSignalChanged(this); //
                            } catch (Exception e) { Debug.LogError($"Error notifying signal observer: {e}"); }
                        }
                    } finally {
                        _notificationStackDepth--;
                        ListPool<ISignalObserver>.Release(buffer);
                    }
                }
            );
        }

        public bool AddObserver(ISignalObserver observer) {
            var result = _observers.Add(observer);
            if (result) observer.OnSignalAdded(this);
            return result;
        }

        public bool RemoveObserver(ISignalObserver observer) {
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

        public static ValueSignal<T> Value<T>(T initialValue = default, bool equality = true) {
            return new ValueSignal<T>(initialValue, equality);
        }

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
        public abstract void SetValue(T value);
        public abstract void SetWithoutNotify(T value);

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
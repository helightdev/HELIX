using System;
using HELIX.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Animation {
    /// <summary>
    ///     Represents a base class for managing the transition of states using tweening logic.
    ///     Provides static creation methods to construct typed tweening arenas such as for colors or floats.
    /// </summary>
    public abstract class TweenStateArena {
        internal TweenStateArena() { }

        public static TweenStateArena<Color> ColorArena(
            VisualElement scheduler,
            Action<Color> update,
            long durationMs = 100L,
            Color startValue = default,
            EasingMode easingMode = EasingMode.Linear
        ) {
            var arena = new TweenStateArena<Color>(scheduler, durationMs, Color.Lerp, update, easingMode);
            arena.Set(startValue);
            return arena;
        }

        public static TweenStateArena<float> FloatArena(
            VisualElement scheduler,
            Action<float> update,
            long durationMs = 100L,
            float startValue = 0f,
            EasingMode easingMode = EasingMode.Linear
        ) {
            var arena = new TweenStateArena<float>(scheduler, durationMs, Mathf.Lerp, update, easingMode);
            arena.Set(startValue);
            return arena;
        }
    }

    public class TweenStateArena<T> : TweenStateArena {
        public delegate T LerpFunc(T a, T b, float t);

        private long _durationMs;
        private EasingMode _easingMode;

        private readonly LerpFunc _lerpLerp;
        private readonly DebouncedScheduler _scheduler;
        private readonly Action<T> _updateFunc;
        private T _value;

        public long DurationMs {
            get => _durationMs;
            set => _durationMs = value;
        }

        public TimeValue Duration {
            get => TimeValue.Milliseconds(_durationMs);
            set => _durationMs = value.unit == TimeUnit.Second ? (long)(value.value * 1000) : (long)value.value;
        }

        public EasingMode EasingMode {
            get => _easingMode;
            set => _easingMode = value;
        }

        public TweenStateArena(
            IVisualElementScheduler scheduler,
            long durationMs,
            LerpFunc lerp,
            Action<T> update,
            EasingMode easingMode = EasingMode.Linear
        ) {
            _scheduler = new DebouncedScheduler(scheduler);
            _durationMs = durationMs;
            _easingMode = easingMode;
            _lerpLerp = lerp;
            _updateFunc = update;
        }

        public T Value {
            get => _value;
            set => Set(value);
        }

        /// <summary>
        ///     Initiates a tweening operation transitioning the current value to the specified new value
        ///     over the configured duration using the provided interpolation function.
        ///     The transition updates the value at regular intervals and invokes the update action
        ///     accordingly.
        /// </summary>
        /// <param name="newValue">
        ///     The target value to which the tween animation will transition.
        /// </param>
        public void Push(T newValue) {
            if (_durationMs == 0) {
                Set(newValue);
                return;
            }
            
            var currentValue = Value;
            _scheduler.Tween(
                _durationMs,
                v => {
                    var t = _easingMode.Eval(v);
                    _value = _lerpLerp(currentValue, newValue, t);
                    _updateFunc(_value);
                }
            );
        }

        /// <summary>
        ///     Sets the current value of the tween without initiating a transition or animation.
        ///     Immediately applies the specified value and invokes the update action to reflect the change.
        /// </summary>
        /// <param name="newValue">
        ///     The value to be directly assigned as the current state.
        /// </param>
        public void Set(T newValue) {
            _scheduler.Stop();
            _value = newValue;
            _updateFunc(Value);
        }
    }
}
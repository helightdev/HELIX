using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Animation {
    /// <summary>
    /// Manages the animation sequence by controlling its playback direction, duration, and looping behavior.
    /// </summary>
    public class AnimationController {
        private float _value;
        private long _duration = 100L;
        private bool _forward;
        private LoopType _type;

        private readonly IVisualElementScheduledItem _scheduledItem;

        /// <summary>
        /// Event triggered during each update of the animation's progress.
        /// This event allows subscribers to receive the current normalized value of the animation,
        /// ranging between 0.0 and 1.0, as it progresses.
        /// </summary>
        public event Action<float> OnUpdate;

        /// <summary>
        /// Event triggered when the animation sequence completes its progress in the current direction.
        /// This can occur at the end of a single animation, after a loop iteration,
        /// or as part of a ping-pong transition, depending on the set <see cref="LoopType"/>.
        /// </summary>
        public event Action OnComplete;
        
        
        public AnimationController(IVisualElementScheduler scheduler) {
            _scheduledItem = scheduler.Execute(TimerUpdateEvent).Until(() => false);
            _scheduledItem.Pause();
            Value = 0f;
        }

        public AnimationController(IVisualElementScheduler scheduler, Action<float> onUpdate) : this(scheduler) {
            OnUpdate += onUpdate;
        }

        public AnimationController(VisualElement element) : this(element.schedule) { }

        public AnimationController(VisualElement element, Action<float> onUpdate) : this(element.schedule, onUpdate) { }

        public long Duration {
            get => _duration;
            set => _duration = value;
        }

        public float Value {
            get => _value;
            set => _value = value;
        }

        public LoopType LoopType {
            get => _type;
            set => _type = value;
        }

        /// <summary>
        /// Plays the animation in the forward direction, starting from the beginning if specified.
        /// </summary>
        /// <param name="restart">
        /// If true, the animation will reset to the beginning (value 0) before playing.
        /// If false, the animation will continue from its current position.
        /// </param>
        public void Forward(bool restart = false) {
            if (restart) Value = 0f;
            if (Mathf.Approximately(Value, 1f)) return;
            _forward = true;
            _scheduledItem.Resume();
            OnUpdate?.Invoke(Value);
        }

        /// <summary>
        /// Plays the animation in the backward direction, optionally starting from the end.
        /// </summary>
        /// <param name="restart">
        /// If true, the animation will reset to the end (value 1) before playing.
        /// If false, the animation will continue from its current position.
        /// </param>
        public void Backward(bool restart = false) {
            if (restart) Value = 1f;
            if (Mathf.Approximately(Value, 0f)) return;
            _forward = false;
            _scheduledItem.Resume();
            OnUpdate?.Invoke(Value);
        }

        /// <summary>
        /// Stops the animation playback and pauses any scheduled updates.
        /// </summary>
        public void Stop() {
            _scheduledItem.Pause();
        }

        private void TimerUpdateEvent(TimerState obj) {
            var stepValue = (double)obj.deltaTime / Duration;
            var unclampedValue = (float)(Value + (_forward ? stepValue : -stepValue));
            var hasCompleted = _forward && unclampedValue >= 1f || !_forward && unclampedValue <= 0f;
            switch (LoopType) {
                case LoopType.None: {
                    Value = Mathf.Clamp01(unclampedValue);
                    OnUpdate?.Invoke(Value);
                    if (!hasCompleted) return;
                    _scheduledItem.Pause();
                    OnComplete?.Invoke();
                    break;
                }
                case LoopType.PingPong: {
                    _value = unclampedValue;
                    while (_value is < 0f or > 1f) {
                        _value = _forward ? 2f - _value : -_value;
                        OnUpdate?.Invoke(Value);
                        OnComplete?.Invoke();
                        _forward = !_forward;
                    }
                    break;
                }
                case LoopType.Loop when hasCompleted: {
                    var newValue = _forward ? unclampedValue - 1f : unclampedValue + 1f;
                    Value = Mathf.Clamp01(newValue);
                    OnUpdate?.Invoke(Value);
                    OnComplete?.Invoke();
                    break;
                }
                case LoopType.Loop:
                    Value = unclampedValue;
                    OnUpdate?.Invoke(Value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum LoopType {
        None,
        Loop,
        PingPong
    }
}
using System;
using UnityEngine.UIElements;

namespace HELIX.Types {
    public struct Transition : IEquatable<Transition> {
        public readonly StylePropertyName property;
        public EasingFunction easing;
        public TimeValue duration;
        public TimeValue delay;

        public Transition(StylePropertyName property) : this() {
            this.property = property;
            easing = new EasingFunction(EasingMode.Linear);
            duration = new TimeValue(200f, TimeUnit.Millisecond);
            delay = new TimeValue(0f, TimeUnit.Millisecond);
        }

        public static implicit operator Transition(StylePropertyName propertyName) {
            return new Transition(propertyName);
        }

        public bool Equals(Transition other) {
            return property.Equals(other.property) && easing.Equals(other.easing) && duration.Equals(other.duration) &&
                   delay.Equals(other.delay);
        }

        public override bool Equals(object obj) {
            return obj is Transition other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(property, easing, duration, delay);
        }
    }

    public interface IEventHandler {
        void Register(VisualElement element);
        void Unregister(VisualElement element);
    }

    public readonly struct CallbackHandler<T> : IEventHandler where T : EventBase<T>, new() {
        private readonly EventCallback<T> _callback;

        public CallbackHandler(EventCallback<T> callback) {
            _callback = callback;
        }

        public static implicit operator CallbackHandler<T>(EventCallback<T> callback) {
            return new CallbackHandler<T>(callback);
        }

        public void Register(VisualElement element) {
            element.RegisterCallback(_callback);
        }

        public void Unregister(VisualElement element) {
            element.UnregisterCallback(_callback);
        }
    }
}
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Types {
  public struct Transition : IEquatable<Transition> {
    public readonly StylePropertyName property;
    public EasingFunction easing;
    public TimeValue duration;
    public TimeValue delay;

    public const float DefaultDuration = 200f;

    public Transition(StylePropertyName property) : this() {
      this.property = property;
      easing = new EasingFunction(EasingMode.Linear);
      duration = new TimeValue(DefaultDuration, TimeUnit.Millisecond);
      delay = new TimeValue(0f, TimeUnit.Millisecond);
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

    public override string ToString() {
      var builder = new StringBuilder();
      builder.Append("Transition(");
      builder.Append(property);
      if (easing.mode != EasingMode.Linear) {
        builder.Append(", easing: ");
        builder.Append(easing.mode);
      }

      if (!Mathf.Approximately(duration.value, DefaultDuration) || duration.unit != TimeUnit.Millisecond) {
        builder.Append(", duration: ");
        builder.Append(duration);
      }

      if (delay.value > 0) {
        builder.Append(", delay: ");
        builder.Append(delay);
      }

      builder.Append(")");
      return builder.ToString();
    }

    public static implicit operator Transition(StylePropertyName propertyName) {
      return new Transition(propertyName);
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
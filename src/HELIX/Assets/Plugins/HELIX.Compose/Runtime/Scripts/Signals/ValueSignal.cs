using System.Collections.Generic;

namespace HELIX.Compose {
  /// <summary>
  ///   A signal that holds a single value.
  /// </summary>
  /// <seealso cref="Signal.Value" />
  public class ValueSignal<T> : Signal<T> {
    private readonly bool _equality;
    protected IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
    protected T value;

    public ValueSignal(T value = default, bool equality = true) {
      this.value = value;
      _equality = equality;
    }

    public override T PeekValue() {
      return value;
    }

    public override void SetValue(T newValue) {
      if (_equality && comparer.Equals(value, newValue)) return;
      value = newValue;
      NotifyDirty();
      NotifyObservers();
    }

    public override void SetWithoutNotify(T newValue) {
      value = newValue;
      NotifyDirty();
    }

    public void NotifyListeners() {
      NotifyDirty();
      NotifyObservers();
    }
  }
}
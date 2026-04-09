using System.Collections.Generic;
using HELIX.Widgets.Diagnostics;

namespace HELIX.Widgets.Signals {
    public class ValueSignal<T> : Signal<T> {
        private readonly bool _equality;
        private T _value;

        public ValueSignal(T value = default, bool equality = true) {
            _value = value;
            _equality = equality;
        }

        public override T PeekValue() {
            return _value;
        }

        public override void SetValue(T value) {
            if (_equality && EqualityComparer<T>.Default.Equals(_value, value)) return;
            _value = value;
            NotifyDirty();
            NotifyObservers();
        }

        public override void SetWithoutNotify(T value) {
            _value = value;
            NotifyDirty();
        }

        public void NotifyListeners() {
            NotifyDirty();
            NotifyObservers();
        }

        public override string ToStringShort() {
            return $"ValueSignal<{typeof(T).Name}>#{this.ShortHash()}";
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<T>("value", _value, showName: false));
        }
    }
}
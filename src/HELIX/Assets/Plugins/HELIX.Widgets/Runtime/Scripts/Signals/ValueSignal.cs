using System.Collections.Generic;
using HELIX.Widgets.Diagnostics;

namespace HELIX.Widgets.Signals {
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
            if (_equality && EqualityComparer<T>.Default.Equals(value, newValue)) return;
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

        public override string ToStringShort() {
            return $"ValueSignal<{typeof(T).Name}>#{this.ShortHash()}";
        }

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<T>("value", value, showName: false));
        }
    }
}
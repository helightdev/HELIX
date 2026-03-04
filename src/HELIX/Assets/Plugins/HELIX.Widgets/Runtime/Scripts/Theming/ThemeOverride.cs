using System;

namespace HELIX.Widgets.Theming {
    public abstract class ThemeOverride {
        public abstract bool TryGetOverride(out object value);
    }

    [Serializable]
    public class ThemeOverride<T> : ThemeOverride {
        public ThemeOverrideType type = ThemeOverrideType.None;
        public T constantValue;
        public string propertyReference;

        public static implicit operator ThemeOverride<T>(T value) {
            return new ThemeOverride<T> {
                type = ThemeOverrideType.Value,
                constantValue = value
            };
        }

        public override bool TryGetOverride(out object value) {
            if (type == ThemeOverrideType.Value) {
                value = constantValue;
                return true;
            }

            value = default(T);
            return false;
        }
    }

    public enum ThemeOverrideType {
        None = 0,
        Value = 1,
        PropertyReference = 2
    }
}
using System;

namespace HELIX.Widgets.Theming {
    public abstract class ThemeOptional {
        public abstract bool TryGetValue(out object result);
    }

    [Serializable]
    public class ThemeOptional<T> : ThemeOptional {
        public bool hasValue;
        public T value;

        public static ThemeOptional<T> None =>
            new() {
                hasValue = false,
                value = default
            };

        public static implicit operator ThemeOptional<T>(T value) {
            return new ThemeOptional<T> {
                hasValue = true,
                value = value
            };
        }

        public static implicit operator ThemeOptional<T>(ThemeOverride<T> themeOverride) {
            if (themeOverride.TryGetOverride(out var value)) {
                return new ThemeOptional<T> {
                    hasValue = true,
                    value = (T)value
                };
            }

            return new ThemeOptional<T> {
                hasValue = false,
                value = default
            };
        }

        public static implicit operator T(ThemeOptional<T> optional) {
            return optional.hasValue ? optional.value : default;
        }

        public static implicit operator ThemeOverride<T>(ThemeOptional<T> optional) {
            return optional.hasValue
                ? new ThemeOverride<T> {
                    type = ThemeOverrideType.Value,
                    constantValue = optional.value
                }
                : new ThemeOverride<T> { type = ThemeOverrideType.None };
        }

        public override bool TryGetValue(out object result) {
            if (hasValue) {
                result = value;
                return true;
            }

            result = default(T);
            return false;
        }
    }
}
using System;

namespace HELIX.Widgets.Theming {
    public interface IMaybeThemeValue {

        bool TryGetThemeValue(out object result);

    }

    public interface IMaybeThemeValue<T> : IMaybeThemeValue {

        bool TryGetThemeValueTyped(out T result) {
            var success = TryGetThemeValue(out var obj);
            if (success && obj is T typed) {
                result = typed;
                return true;
            }

            result = default;
            return false;
        }

    }

    public abstract class ThemeOptional : IMaybeThemeValue {

        public abstract bool TryGetThemeValue(out object result);

    }

    [Serializable]
    public class ThemeOptional<T> : ThemeOptional, IMaybeThemeValue<T> {

        public bool hasValue;
        public T value;

        public static ThemeOptional<T> None =>
            new() {
                hasValue = false,
                value = default
            };

        public override bool TryGetThemeValue(out object result) {
            if (hasValue) {
                result = value;
                return true;
            }

            result = default(T);
            return false;
        }

        public static implicit operator ThemeOptional<T>(T value) {
            return new ThemeOptional<T> {
                hasValue = true,
                value = value
            };
        }

        public static implicit operator ThemeOptional<T>(ThemeOverride<T> themeOverride) {
            if (themeOverride.TryGetThemeValue(out var value))
                return new ThemeOptional<T> {
                    hasValue = true,
                    value = (T)value
                };

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

    }
}
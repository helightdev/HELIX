using System;

namespace HELIX.Widgets.Theming {
    public abstract class ThemeOverrides { }

    [Serializable]
    public class ThemeOverride<T> : ThemeOverrides {
        public ThemeOverrideType type = ThemeOverrideType.None;
        public T constantValue;
        public string propertyReference;

        public void ApplyTo(ThemeValue<T> value) {
            switch (type) {
                case ThemeOverrideType.None:
                    break;
                case ThemeOverrideType.Value:
                    value.Value = constantValue;
                    break;
                case ThemeOverrideType.PropertyReference:
                    value.SwapPropertyByReference(propertyReference);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum ThemeOverrideType {
        None = 0,
        Value = 1,
        PropertyReference = 2
    }
}
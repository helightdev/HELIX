using System;
using Codice.Client.Common;

namespace HELIX.Widgets.Theming {
    public abstract class ThemeOverrides { }

    [Serializable]
    public class ThemeOverride<T> : ThemeOverrides {
        public ThemeOverrideType type = ThemeOverrideType.None;
        public T constantValue;
        public string propertyReference;
    }

    public enum ThemeOverrideType {
        None = 0,
        Value = 1,
        PropertyReference = 2
    }
}
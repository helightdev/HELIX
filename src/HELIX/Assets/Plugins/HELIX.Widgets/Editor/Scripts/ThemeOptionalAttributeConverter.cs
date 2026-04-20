using HELIX.Widgets.Theming;
using UnityEditor.UIElements;

namespace HELIX.Widgets.Editor {
  public class ThemeOptionalAttributeConverter<T> : UxmlAttributeConverter<ThemeOptional<T>> {
    private readonly ThemeOverrideAttributeConverter<T> _overrideConverter = new();

    public override ThemeOptional<T> FromString(string value) {
      return _overrideConverter.FromString(value);
    }

    public override string ToString(ThemeOptional<T> value) {
      return _overrideConverter.ToString(value);
    }
  }
}
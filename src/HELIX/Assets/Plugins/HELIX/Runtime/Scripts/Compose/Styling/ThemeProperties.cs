using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public static class ThemeProperties {
    public static readonly ThemeProperty<Length> TextGap = new(data => data[SpacingRole.Spacing1]);
    public static readonly ThemeProperty<Length> DecoratorColumnGap = new(data => data[SpacingRole.Spacing1]);

    public static readonly ThemeProperty<TextStyle> LabelStyle = new(data => data[TextRole.LabelLarge]);
    public static readonly ThemeProperty<TextStyle> DescriptionStyle = new(data => data[TextRole.LabelMedium]);
    public static readonly ThemeProperty<TextStyle> PrefixStyle = new(data => data[TextRole.BodyMedium]);
    public static readonly ThemeProperty<TextStyle> SuffixStyle = new(data => data[TextRole.BodyMedium]);
    public static readonly ThemeProperty<TextStyle> DecoratorStyle = new(data => data[TextRole.LabelMedium]);

  }
}
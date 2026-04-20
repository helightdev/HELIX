using HELIX.Types;
using UnityEditor.UIElements;
using UnityEngine;

namespace HELIX.Editor {
  public class AlignmentAttributeConverter : UxmlAttributeConverter<Alignment> {
    public override Alignment FromString(string value) {
      if (HelixConvert.ToVector2(value, out var vec2)) return vec2;
      Debug.LogError(
        $"Cannot convert '{value}' to Alignment. Expected format: 'x,y' where x and y are floats between 0 and 1."
      );
      return default;
    }

    public override string ToString(Alignment value) {
      return HelixConvert.ToUssString(value);
    }
  }
}
using HELIX.Diagnostics;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using HELIX.Widgets.Universal.Styles;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
  public class TextStyleModifier : Modifier {
    public readonly HTextStyle style;

    public TextStyleModifier(HTextStyle style) {
      this.style = style;
    }

    public override void Apply(VisualElement element) {
      (style ?? HTextStyle.Default).Apply(element);
    }

    public override void Reset(VisualElement element) {
      HTextStyle.Default.Apply(element);
    }

    public override bool HasChanged(Modifier previous) {
      if (previous is not TextStyleModifier prev) return true;
      return !Equals(style, prev.style);
    }

    public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
      base.FillModifierProperties(properties);
      properties.Add(new TextStyleProperty("style", style, showName: false));
    }

    public static TextStyleModifier Of(HTextStyle style) {
      return new TextStyleModifier(style);
    }
  }
}
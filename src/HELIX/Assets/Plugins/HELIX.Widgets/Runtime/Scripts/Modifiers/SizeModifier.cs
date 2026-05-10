using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
  public class SizeModifier : Modifier {
    public static readonly SizeModifier None = new(
      StyleLength2.Initial,
      StyleLength2.Initial,
      StyleLength2.Initial
    );

    public readonly BoxConstraints constraints;

    public SizeModifier(StyleLength2 size, StyleLength2 minSize, StyleLength2 maxSize) {
      constraints = new BoxConstraints(size, minSize, maxSize);
    }

    public SizeModifier(BoxConstraints constraints) {
      this.constraints = constraints;
    }

    public override void Apply(VisualElement element) {
      constraints.Apply(element);
    }

    public override void Reset(VisualElement element) {
      BoxConstraints.Initial.Apply(element);
    }

    public override bool HasChanged(Modifier previous) {
      if (previous is not SizeModifier prev) return true;
      return !Equals(constraints, prev.constraints);
    }

    public static SizeModifier Of(StyleLength width, StyleLength height) {
      return new SizeModifier(
        new StyleLength2(width, height),
        StyleLength2.Initial,
        StyleLength2.Initial
      );
    }

    public static SizeModifier Of(BoxConstraints constraints) {
      return new SizeModifier(
        constraints.preferred,
        constraints.min,
        constraints.max
      );
    }

    public static SizeModifier Tight(StyleLength2 size) {
      return new SizeModifier(size, size, size);
    }

    public static SizeModifier Tight(StyleLength width, StyleLength height) {
      return Tight(new StyleLength2(width, height));
    }

    public static SizeModifier Min(StyleLength2 min) {
      return new SizeModifier(min, min, StyleLength2.Initial);
    }

    public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
      base.FillModifierProperties(properties);
      properties.Add(
        new DiagnosticsProperty<BoxConstraints>(
          "constraints",
          constraints,
          defaultValue: BoxConstraints.Initial,
          showName: false
        )
      );
    }
  }
}
using System;
using HELIX.Diagnostics;
using HELIX.Types;
using HELIX.Widgets.Modifiers;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal.Substances {
  /// <summary>
  /// An implementation of <see cref="Substance"/> that builds an <see cref="HIcon"/> widget based on a provided icon name and font.
  /// </summary>
  public class IconSubstance : Substance {
    public WidgetStateProperty<string> icon = WidgetStateProperties.Never<string>();
    public WidgetStateProperty<FontDefinition> font = WidgetStateProperties.Never<FontDefinition>();
    public WidgetStateProperty<StyleColor> color = WidgetStateProperties.Never<StyleColor>();
    public WidgetStateProperty<StyleLength> size = WidgetStateProperties.Never<StyleLength>();
    public WidgetStateProperty<BoxConstraints> constraints = WidgetStateProperties.Never<BoxConstraints>();
    public WidgetStateProperty<ModifierSet> modifiers = WidgetStateProperties.Never<ModifierSet>();
    public WidgetStateProperty<float> opacity = WidgetStateProperties.Never<float>();
    public WidgetStateProperty<StyleLength4> position = WidgetStateProperties.Never<StyleLength4>();
    public WidgetStateProperty<Transition[]> transitions = WidgetStateProperties.Never<Transition[]>();

    public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
      base.DebugFillProperties(properties);
      properties.Add(new DiagnosticsProperty<object>("icon", icon, showName: false));
      properties.Add(new DiagnosticsProperty<object>("font", font, showName: false));
      properties.Add(new DiagnosticsProperty<object>("color", color, showName: false));
      properties.Add(new DiagnosticsProperty<object>("size", size, showName: false));
      properties.Add(new DiagnosticsProperty<object>("constraints", constraints, showName: false));
      properties.Add(new DiagnosticsProperty<object>("position", position, showName: false));
      properties.Add(new DiagnosticsProperty<object>("opacity", opacity, showName: false));
      properties.Add(new DiagnosticsProperty<object>("transitions", transitions, showName: false));
      properties.Add(new DiagnosticsProperty<object>("modifiers", modifiers, showName: false));
    }

    public override IWidgetListCandidate Build(BuildContext context, WidgetState state) {
      var box = new HIcon(
        icon.ResolveOrDefault(state), 
        font.ResolveOrDefault(state, FaSolidIcons.FontDefinition),
        size: size.ResolveOrDefault(state, StyleKeyword.Initial),
        color: color.ResolveOrDefault(state, Color.white),
        modifiers: new Modifier[] {
          new SizeModifier(constraints.ResolveOrDefault(state, BoxConstraints.Initial)).Fallback(),
          new PositionModifier(position.ResolveOrDefault(state, StyleLength4.Zero), Position.Absolute),
          new OpacityModifier(opacity.ResolveOrDefault(state, 1f)).Fallback(),
          new TransitionsModifier(transitions.ResolveOrDefault(state, Array.Empty<Transition>())).Fallback(),
          FlexibleModifier.Fill
        }
      );
      box.AddModifiers(modifiers.ResolveOrDefault(state, ModifierSet.Empty));
      return box;
    }
  }
}
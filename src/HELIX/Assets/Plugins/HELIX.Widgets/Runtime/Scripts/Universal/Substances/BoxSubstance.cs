using System;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Modifiers;
using HELIX.Widgets.Universal.Styles;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal.Substances {
    public class BoxSubstance : Substance {

        public WidgetStateProperty<BackgroundStyle> background = WidgetStateProperties.Never<BackgroundStyle>();
        public WidgetStateProperty<Border> border = WidgetStateProperties.Never<Border>();
        public WidgetStateProperty<BorderRadius> borderRadius = WidgetStateProperties.Never<BorderRadius>();
        public WidgetStateProperty<BoxConstraints> constraints = WidgetStateProperties.Never<BoxConstraints>();
        public WidgetStateProperty<ModifierSet> modifiers = WidgetStateProperties.Never<ModifierSet>();
        public WidgetStateProperty<float> opacity = WidgetStateProperties.Never<float>();
        public WidgetStateProperty<StyleLength4> position = WidgetStateProperties.Never<StyleLength4>();
        public WidgetStateProperty<Transition[]> transitions = WidgetStateProperties.Never<Transition[]>();

        public override void DebugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.DebugFillProperties(properties);
            properties.Add(new DiagnosticsProperty<object>("backgroundStyle", background, showName: false));
            properties.Add(new DiagnosticsProperty<object>("border", border, showName: false));
            properties.Add(new DiagnosticsProperty<object>("borderRadius", borderRadius, showName: false));
            properties.Add(new DiagnosticsProperty<object>("constraints", constraints, showName: false));
            properties.Add(new DiagnosticsProperty<object>("position", position, showName: false));
            properties.Add(new DiagnosticsProperty<object>("opacity", opacity, showName: false));
            properties.Add(new DiagnosticsProperty<object>("transitions", transitions, showName: false));
            properties.Add(new DiagnosticsProperty<object>("modifiers", modifiers, showName: false));
        }

        public override IWidgetListCandidate Build(BuildContext context, WidgetState state) {
            var resolveOrDefault = background.ResolveOrDefault(state);
            var box = new HBox(
                background: resolveOrDefault,
                borderRadius: borderRadius.ResolveOrDefault(state, BorderRadius.None),
                border: border.ResolveOrDefault(state, Border.None),
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
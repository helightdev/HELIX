using System;
using HELIX.Extensions;
using HELIX.Types;
using HELIX.Widgets.Diagnostics;
using HELIX.Widgets.Diagnostics.Properties;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class TransitionsModifier : Modifier {
        public static readonly TransitionsModifier None = new(Array.Empty<Transition>());
        public readonly Transition[] transitions;

        public TransitionsModifier(Transition[] transitions) {
            this.transitions = transitions;
        }

        public TransitionsModifier() {
            transitions = Array.Empty<Transition>();
        }

        public override void Apply(VisualElement element) {
            element.Transitions(transitions);
        }

        public override void Reset(VisualElement element) {
            element.Transitions();
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not TransitionsModifier prev) return true;
            if (transitions.Length != prev.transitions.Length) return true;
            for (var i = 0; i < transitions.Length; i++)
                if (!transitions[i].Equals(prev.transitions[i]))
                    return true;

            return false;
        }

        public override void FillModifierProperties(DiagnosticPropertiesBuilder properties) {
            base.FillModifierProperties(properties);
            properties.Add(new IterableProperty<Transition>("transitions", transitions, showName: false));
        }

        protected override string FindConstantName() {
            if (DeepEquals(None)) return nameof(None);
            return null;
        }

        public static TransitionsModifier Of(params Transition[] transitions) {
            return new TransitionsModifier(transitions);
        }
    }
}
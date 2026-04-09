using System;
using HELIX.Extensions;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class TransitionModifier : Modifier {
        public static readonly TransitionModifier None = new(Array.Empty<Transition>());
        public readonly Transition[] transitions;

        public TransitionModifier(Transition[] transitions) {
            this.transitions = transitions;
        }

        public TransitionModifier() {
            transitions = Array.Empty<Transition>();
        }

        public override void Apply(VisualElement element) {
            element.Transitions(transitions);
        }

        public override void Reset(VisualElement element) {
            element.Transitions();
        }

        public override bool HasChanged(Modifier previous) {
            if (previous is not TransitionModifier prev) return true;
            if (transitions.Length != prev.transitions.Length) return true;
            for (var i = 0; i < transitions.Length; i++) {
                if (!transitions[i].Equals(prev.transitions[i])) return true;
            }

            return false;
        }

        public static TransitionModifier Of(params Transition[] transitions) {
            return new TransitionModifier(transitions);
        }
    }
}
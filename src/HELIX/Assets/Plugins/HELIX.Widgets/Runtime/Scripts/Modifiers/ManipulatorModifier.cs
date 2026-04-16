using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class ManipulatorModifier : SingletonModifier {
        public IManipulator manipulator;

        public ManipulatorModifier(IManipulator manipulator) {
            this.manipulator = manipulator;
        }

        public override void Hook(VisualElement element) {
            element.AddManipulator(manipulator);
        }

        public override void Unhook(VisualElement element) {
            element.RemoveManipulator(manipulator);
        }

        public override bool HasChanged(Modifier previous) {
            return previous is not ManipulatorModifier prev || !ReferenceEquals(manipulator, prev.manipulator);
        }

        public static ManipulatorModifier Of(IManipulator manipulator) {
            return new ManipulatorModifier(manipulator);
        }
    }
}
using HELIX.Widgets.Universal.Controllers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers
{
    public class ButtonControllerModifier : SingletonModifier {
        public readonly ButtonController controller;
        private ButtonController.ButtonManipulator _manipulator;

        public ButtonControllerModifier(ButtonController controller) {
            this.controller = controller;
        }

        public override void Hook(VisualElement element) {
            _manipulator = new ButtonController.ButtonManipulator(controller);
            element.AddManipulator(_manipulator);
        }

        public override void Unhook(VisualElement element) {
            if (_manipulator != null && _manipulator.target == element) element.RemoveManipulator(_manipulator);
            _manipulator = null;
        }

        public override bool HasChanged(Modifier previous) {
            return previous is not ButtonControllerModifier prev || !ReferenceEquals(controller, prev.controller);
        }
    }
}
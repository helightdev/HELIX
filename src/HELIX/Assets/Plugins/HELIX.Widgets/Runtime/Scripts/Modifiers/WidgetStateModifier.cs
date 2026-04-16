using HELIX.Widgets.Universal.Controllers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class WidgetStateModifier : SingletonModifier {
        public readonly WidgetStateController controller;
        private WidgetStateModifierManipulator _manipulator;

        public WidgetStateModifier(WidgetStateController controller) {
            this.controller = controller;
        }

        public override void Hook(VisualElement element) {
            _manipulator = new WidgetStateModifierManipulator(controller);
            element.AddManipulator(_manipulator);
        }

        public override void Unhook(VisualElement element) {
            if (_manipulator != null && _manipulator.target == element) element.RemoveManipulator(_manipulator);
            _manipulator = null;
        }

        public override bool HasChanged(Modifier previous) {
            return previous is not WidgetStateModifier prev || !ReferenceEquals(controller, prev.controller);
        }

        public class WidgetStateModifierManipulator : Manipulator {
            public readonly WidgetStateController controller;

            public WidgetStateModifierManipulator(WidgetStateController controller) {
                this.controller = controller;
            }

            protected override void RegisterCallbacksOnTarget() {
                target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
                target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
                target.RegisterCallback<FocusInEvent>(OnFocusIn);
                target.RegisterCallback<FocusOutEvent>(OnFocusOut);
                target.RegisterCallback<NavigationMoveEvent>(OnNavigationMove);
            }

            protected override void UnregisterCallbacksFromTarget() {
                target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
                target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
                target.UnregisterCallback<FocusInEvent>(OnFocusIn);
                target.UnregisterCallback<FocusOutEvent>(OnFocusOut);
                target.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove);
            }

            private void OnFocusOut(FocusOutEvent evt) {
                controller.Disable(WidgetState.Focused);
            }

            private void OnFocusIn(FocusInEvent evt) {
                controller.Enable(WidgetState.Focused);
                if (WidgetStateController.LastNavigated) {
                    controller.Enable(WidgetState.Navigated);
                }
            }

            private void OnPointerLeave(PointerLeaveEvent evt) {
                controller.Disable(WidgetState.Hovered);
            }

            private void OnPointerEnter(PointerEnterEvent evt) {
                controller.Enable(WidgetState.Hovered);
            }

            private void OnNavigationMove(NavigationMoveEvent evt) {
                controller.Enable(WidgetState.Navigated);
            }
        }
    }
}
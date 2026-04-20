using HELIX.Widgets.Universal.Controllers;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Modifiers {
    public class WidgetStateModifier : SingletonModifier {
        public readonly WidgetStateController controller;
        private WidgetStateModifierManipulator _manipulator;
        public bool handleFocus = true;

        public WidgetStateModifier(WidgetStateController controller, bool handleFocus = true) {
            this.controller = controller;
            this.handleFocus = handleFocus;
        }

        public override void Hook(VisualElement element) {
            _manipulator = new WidgetStateModifierManipulator(controller, handleFocus);
            element.AddManipulator(_manipulator);
        }

        public override void Unhook(VisualElement element) {
            if (_manipulator != null && _manipulator.target == element) element.RemoveManipulator(_manipulator);
            _manipulator = null;
        }

        public override bool HasChanged(Modifier previous) {
            return previous is not WidgetStateModifier prev || !ReferenceEquals(controller, prev.controller) ||
                   handleFocus != prev.handleFocus;
        }

        public class WidgetStateModifierManipulator : Manipulator {
            public readonly WidgetStateController controller;
            public readonly bool handleFocus;

            public WidgetStateModifierManipulator(WidgetStateController controller, bool handleFocus) {
                this.controller = controller;
                this.handleFocus = handleFocus;
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
                if (!handleFocus) return;
                controller.Disable(WidgetState.Focused);
            }

            private void OnFocusIn(FocusInEvent evt) {
                if (!handleFocus) return;
                controller.Enable(WidgetState.Focused);
                if (WidgetStateController.LastNavigated) controller.Enable(WidgetState.Navigated);
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
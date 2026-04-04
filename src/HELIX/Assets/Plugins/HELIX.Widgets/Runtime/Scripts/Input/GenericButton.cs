using System;
using HELIX.Widgets.Descriptors;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Input {
    [UxmlElement]
    public partial class GenericButton : BaseElement, IWidgetElement {
        public WidgetState state = WidgetState.None;

        public bool Enabled {
            get => !state.HasFlag(WidgetState.Disabled);
            set {
                state = value ? state & ~WidgetState.Disabled : state | WidgetState.Disabled;
                UpdateWidgetState(state);
            }
        }

        public bool Selected {
            get => state.HasFlag(WidgetState.Selected);
            set {
                state = value ? state | WidgetState.Selected : state & ~WidgetState.Selected;
                UpdateWidgetState(state);
            }
        }

        public GenericButton() {
            this.AddManipulator(new Manipulator(this));
        }

        public void HandleClick() {
            if (!Enabled) return;
            if (Descriptor is ButtonBuilder bb) bb.onClick?.Invoke();
        }

        public void UpdateWidgetState(WidgetState newState) {
            if (newState == state) return;
            state = newState;
            if (Builder != null && Descriptor != null) {
                BuildChildAndReconcile();
            }
        }

        public class Manipulator : Clickable {
            public GenericButton button;

            public Manipulator(GenericButton button) : base(button.HandleClick) {
                this.button = button;
            }

            protected override void RegisterCallbacksOnTarget() {
                base.RegisterCallbacksOnTarget();
                target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
                target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
                target.RegisterCallback<FocusInEvent>(OnFocusIn);
                target.RegisterCallback<FocusOutEvent>(OnFocusOut);
                target.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
            }

            private void OnNavigationSubmit(NavigationSubmitEvent evt) {
                if (!button.Enabled) return;
                button.HandleClick();
                button.UpdateWidgetState(button.state | WidgetState.Pressed);
                button.schedule
                    .Execute(() => button.UpdateWidgetState(button.state & ~WidgetState.Pressed))
                    .ExecuteLater(100);
            }

            protected override void UnregisterCallbacksFromTarget() {
                base.UnregisterCallbacksFromTarget();
                target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
                target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
                target.UnregisterCallback<FocusInEvent>(OnFocusIn);
                target.UnregisterCallback<FocusOutEvent>(OnFocusOut);
                target.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
            }

            private void OnFocusOut(FocusOutEvent evt) {
                button.UpdateWidgetState(button.state & ~WidgetState.Focused);
            }

            private void OnFocusIn(FocusInEvent evt) {
                button.UpdateWidgetState(button.state | WidgetState.Focused);
            }

            private void OnPointerLeave(PointerLeaveEvent evt) {
                button.UpdateWidgetState(button.state & ~WidgetState.Hovered);
            }

            private void OnPointerEnter(PointerEnterEvent evt) {
                if (button.Enabled) button.UpdateWidgetState(button.state | WidgetState.Hovered);
            }

            protected override void ProcessUpEvent(EventBase evt, Vector2 localPosition, int pointerId) {
                base.ProcessUpEvent(evt, localPosition, pointerId);
                button.UpdateWidgetState(button.state & ~WidgetState.Pressed);
            }

            protected override void ProcessCancelEvent(EventBase evt, int pointerId) {
                base.ProcessCancelEvent(evt, pointerId);
                button.UpdateWidgetState(button.state & ~WidgetState.Pressed);
            }

            protected override void ProcessDownEvent(EventBase evt, Vector2 localPosition, int pointerId) {
                base.ProcessDownEvent(evt, localPosition, pointerId);
                if (button.Enabled) button.UpdateWidgetState(button.state | WidgetState.Pressed);
            }
        }

        public VisualElement Element => this;
        public Widget Descriptor { get; private set; }
        private WidgetStateBuilder Builder { get; set; }

        public bool Reconcile(Widget updated) {
            if (updated is not ButtonBuilder bb) return false;
            bb.alignment.AlignAsColumn(this);
            Builder = bb.builder;
            Enabled = bb.enabled;
            Selected = bb.selected;
            BuildChildAndReconcile();
            Modifier.ApplyDelta(Descriptor, updated, this);
            Descriptor = updated;
            return true;
        }

        public void BuildChildAndReconcile() {
            if (Builder == null) return;
            try {
                var widget = Builder(new BuildContext(this), state);
                DefaultReconciler.ReconcileSingleDirect(this, widget);
            } catch (Exception e) { Debug.LogError($"Error building button child: {e}"); }
        }
    }
}
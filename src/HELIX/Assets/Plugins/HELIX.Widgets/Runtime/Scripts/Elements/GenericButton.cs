using System;
using HELIX.Widgets.Universal;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Elements {
    [UxmlElement]
    public partial class GenericButton : BuildingWidgetBaseElement<ButtonBuilder> {
        public Action onClick;
        public WidgetState state = WidgetState.None;

        public GenericButton() {
            this.AddManipulator(new Manipulator(this));
        }

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

        private BuildFunction<WidgetState> Builder { get; set; }

        public void HandleClick() {
            Debug.Log("Button clicked!");
            if (!Enabled) return;
            if (Descriptor is ButtonBuilder bb && bb.onClick != null) {
                Debug.Log("Invoking button's onClick action...");
                ModificationBarrier.Run(() => {
                    Debug.Log("Inside ModificationBarrier, invoking onClick...");
                    bb.onClick.Invoke();
                });
            }

            onClick?.Invoke();
        }

        public void UpdateWidgetState(WidgetState newState) {
            if (newState == state) return;
            state = newState;
            if (Builder != null && Descriptor != null) ModificationBarrier.Rebuild(this);
        }

        public override void Apply(ButtonBuilder previous, ButtonBuilder widget) {
            base.Apply(previous, widget);
            widget.alignment.AlignAsColumn(this);
            Builder = widget.builder;
            Enabled = widget.enabled;
            Selected = widget.selected;
        }

        protected override IBuildable GetBuildableForWidget(ButtonBuilder previous, ButtonBuilder widget) {
            return new ParameterizedFunctionBuildable<WidgetState>(Builder, state);
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
    }
}
using System;
using HELIX.Widgets.Signals;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Widgets.Universal.Controllers {
  public class ButtonController : Signal {
    public readonly WidgetStateController widgetState;
    public bool enabled = true;
    public Action onClick;

    public ButtonController(WidgetStateController widgetState = null) {
      this.widgetState = widgetState;
    }

    public bool Enabled => enabled && (widgetState?.PeekValue().Enabled() ?? true);

    private void HandleClick() {
      NotifyDirty();
      NotifyObservers();
      onClick?.Invoke();
    }

    public class ButtonManipulator : Clickable {
      public ButtonController controller;

      public ButtonManipulator(ButtonController controller) : base(controller.HandleClick) {
        this.controller = controller;
      }

      protected override void RegisterCallbacksOnTarget() {
        base.RegisterCallbacksOnTarget();
        target.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
      }

      private void OnNavigationSubmit(NavigationSubmitEvent evt) {
        if (!controller.Enabled) return;
        controller.HandleClick();
        controller.widgetState?.Enable(WidgetState.Pressed | WidgetState.Navigated);
        target.schedule
          .Execute(() => controller.widgetState?.Disable(WidgetState.Pressed))
          .ExecuteLater(100);
      }

      protected override void UnregisterCallbacksFromTarget() {
        base.UnregisterCallbacksFromTarget();
        target.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
      }

      protected override void ProcessUpEvent(EventBase evt, Vector2 localPosition, int pointerId) {
        base.ProcessUpEvent(evt, localPosition, pointerId);
        controller?.widgetState.Disable(WidgetState.Pressed);
      }

      protected override void ProcessCancelEvent(EventBase evt, int pointerId) {
        base.ProcessCancelEvent(evt, pointerId);
        controller?.widgetState.Disable(WidgetState.Pressed);
      }

      protected override void ProcessDownEvent(EventBase evt, Vector2 localPosition, int pointerId) {
        if (!controller.Enabled) return;
        base.ProcessDownEvent(evt, localPosition, pointerId);
        controller?.widgetState?.DisableEnable(WidgetState.Navigated, WidgetState.Pressed);
      }
    }
  }

  public class TextEditingController : ValueSignal<string> {
    public readonly WidgetStateController widgetState;
    public bool enabled = true;
    public Action onBeginEditing;
    public Action onCanceled;
    public Action<string> onChanged;
    public Action onEndEditing;
    public Action<string> onSubmitted;

    public TextEditingController(WidgetStateController widgetState = null, string initialValue = "")
      : base(initialValue) {
      this.widgetState = widgetState;
    }

    public bool Enabled => enabled && (widgetState?.PeekValue().Enabled() ?? true);

    public override void SetValue(string newValue) {
      base.SetValue(newValue);
      onChanged?.Invoke(newValue);
    }
  }
}
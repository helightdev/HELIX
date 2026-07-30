using HELIX.Theming;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public abstract class InputClickableComposable<T> : InputBoundaryComposable<T> where T : struct {
    private int _activePointerId = -1;

    protected bool Active { get; private set; }
    protected Vector2 LastMousePosition { get; private set; }

    protected InputClickableComposable() {
      handleFocus = true;
    }

    protected override void OnAttach() {
      base.OnAttach();
      Node.RegisterCallback<PointerDownEvent>(OnPointerDown);
      Node.RegisterCallback<PointerMoveEvent>(OnPointerMove);
      Node.RegisterCallback<PointerUpEvent>(OnPointerUp);
      Node.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
      Node.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
      Node.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
    }

    protected override void OnDetach() {
      base.OnDetach();
      Node.UnregisterCallback<PointerDownEvent>(OnPointerDown);
      Node.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
      Node.UnregisterCallback<PointerUpEvent>(OnPointerUp);
      Node.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
      Node.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
      Node.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
    }

    protected virtual void OnClick(EventBase evt) { }

    protected virtual void OnNavigationSubmit(NavigationSubmitEvent evt) {
      if ((InputState & State.Disabled) != 0) return;
      OnClick(evt);
      evt.StopPropagation();
    }

    protected virtual void OnPointerDown(PointerDownEvent evt) {
      if (Active || evt.button != (int)MouseButton.LeftMouse) return;

      Active = true;
      _activePointerId = evt.pointerId;
      LastMousePosition = evt.localPosition;

      Node.CapturePointer(evt.pointerId);
      this.Enable(State.Active);
      Node.MarkDirty();

      evt.StopImmediatePropagation();
    }

    protected virtual void OnPointerMove(PointerMoveEvent evt) {
      if (!Active) return;
      LastMousePosition = evt.localPosition;
      evt.StopPropagation();
    }

    protected virtual void OnPointerUp(PointerUpEvent evt) {
      if (!Active || evt.pointerId != _activePointerId) return;

      var clicked = Node.worldBound.Contains(evt.position);

      Active = false;
      _activePointerId = -1;
      LastMousePosition = evt.localPosition;

      Node.ReleasePointer(evt.pointerId);
      this.Disable(State.Active);
      Node.MarkDirty();

      if (clicked) {
        OnClick(evt);
      }

      evt.StopPropagation();
    }

    protected virtual void OnPointerCancel(PointerCancelEvent evt) {
      if (!Active || evt.pointerId != _activePointerId) return;

      Cancel(evt, evt.pointerId);
    }

    protected virtual void OnPointerCaptureOut(PointerCaptureOutEvent evt) {
      if (!Active) return;

      Cancel(evt, evt.pointerId);
    }

    protected virtual void Cancel(EventBase evt, int pointerId) {
      Active = false;
      _activePointerId = -1;

      Node.ReleasePointer(pointerId);
      this.Disable(State.Active);
      Node.MarkDirty();

      evt.StopPropagation();
    }
  }
}
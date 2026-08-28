using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  [AttributeUsage(AttributeTargets.Method)]
  [MixinExpression("@CALL<ClickHandlerImpl>")]
  [MixinImport(typeof(ComposeMixinLibrary))]
  public class ClickHandlerAttribute : Attribute { }

  public class InputClickableManipulator : Manipulator {
    public InputClickableManipulator(
      VisualElement target,
      IStateHolder holder,
      Action<EventBase> onClickAction = null
    ) {
      this.target = target;
      Holder = holder;
      OnClickAction = onClickAction;
    }

    protected bool Active { get; private set; }
    protected Vector2 LastMousePosition { get; private set; }
    public IStateHolder Holder { get; }
    public int ActivePointerId { get; private set; } = -1;
    public Action<EventBase> OnClickAction { get; set; }

    protected override void RegisterCallbacksOnTarget() {
      target.RegisterCallback<PointerDownEvent>(OnPointerDown);
      target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
      target.RegisterCallback<PointerUpEvent>(OnPointerUp);
      target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
      target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
      target.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
    }

    protected override void UnregisterCallbacksFromTarget() {
      target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
      target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
      target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
      target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
      target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
      target.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
    }

    public virtual void OnClick(EventBase evt) {
      OnClickAction?.Invoke(evt);
    }

    public virtual void OnNavigationSubmit(NavigationSubmitEvent evt) {
      if ((Holder.InputState & State.Disabled) != 0) return;
      OnClick(evt);
      evt.StopPropagation();
    }

    public virtual void OnPointerDown(PointerDownEvent evt) {
      if (Active || evt.button != (int)MouseButton.LeftMouse) return;

      Active = true;
      ActivePointerId = evt.pointerId;
      LastMousePosition = evt.localPosition;

      target.CapturePointer(evt.pointerId);
      Holder.Enable(State.Active);
      Holder.MarkDirty();

      evt.StopImmediatePropagation();
    }

    public virtual void OnPointerMove(PointerMoveEvent evt) {
      if (!Active) return;
      LastMousePosition = evt.localPosition;
      evt.StopPropagation();
    }

    public virtual void OnPointerUp(PointerUpEvent evt) {
      if (!Active || evt.pointerId != ActivePointerId) return;

      var clicked = target.worldBound.Contains(evt.position);

      Active = false;
      ActivePointerId = -1;
      LastMousePosition = evt.localPosition;

      target.ReleasePointer(evt.pointerId);
      Holder.Disable(State.Active);
      Holder.MarkDirty();

      if (clicked) OnClick(evt);

      evt.StopPropagation();
    }

    public virtual void OnPointerCancel(PointerCancelEvent evt) {
      if (!Active || evt.pointerId != ActivePointerId) return;

      Cancel(evt, evt.pointerId);
    }

    public virtual void OnPointerCaptureOut(PointerCaptureOutEvent evt) {
      if (!Active) return;

      Cancel(evt, evt.pointerId);
    }

    public virtual void Cancel(EventBase evt, int pointerId) {
      Active = false;
      ActivePointerId = -1;

      target.ReleasePointer(pointerId);
      Holder.Disable(State.Active);
      Holder.MarkDirty();

      evt.StopPropagation();
    }
  }
}
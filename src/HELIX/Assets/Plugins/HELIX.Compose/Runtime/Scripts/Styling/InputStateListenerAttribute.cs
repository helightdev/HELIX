using System;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  [AttributeUsage(AttributeTargets.Class)]
  public class InputStateListenerAttribute : Attribute {
    public InputStateListenerAttribute(bool focus = true, bool hover = true, bool dirty = true) { }
  }

  public class InputListenerManipulator : Manipulator {
    public bool focus;
    public IStateHolder holder;
    public bool hover;

    public InputListenerManipulator(
      VisualElement target,
      IStateHolder holder,
      bool focus = true,
      bool hover = true
    ) {
      this.target = target;
      this.holder = holder;
      this.focus = focus;
      this.hover = hover;
    }

    protected override void RegisterCallbacksOnTarget() {
      if (hover) target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
      if (hover) target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
      if (focus) target.RegisterCallback<FocusInEvent>(OnFocusIn);
      if (focus) target.RegisterCallback<FocusOutEvent>(OnFocusOut);
      if (focus) target.RegisterCallback<NavigationMoveEvent>(OnNavigationMove);
    }

    protected override void UnregisterCallbacksFromTarget() {
      if (hover) target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
      if (hover) target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
      if (focus) target.UnregisterCallback<FocusInEvent>(OnFocusIn);
      if (focus) target.UnregisterCallback<FocusOutEvent>(OnFocusOut);
      if (focus) target.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove);
    }

    protected virtual void OnFocusOut(FocusOutEvent evt) {
      if (!focus || (holder.InputState & State.Focused) == 0) return;
      holder.Disable(State.Focused);
    }

    protected virtual void OnFocusIn(FocusInEvent evt) {
      if (!focus || (holder.InputState & State.Focused) != 0) return;
      holder.Enable(State.Focused);
      //if (WidgetStateController.LastNavigated) state.Enable(WidgetState.Navigated);
    }

    protected virtual void OnPointerLeave(PointerLeaveEvent evt) {
      if ((holder.InputState & State.Hovered) == 0) return;
      holder.Disable(State.Hovered);
    }

    protected virtual void OnPointerEnter(PointerEnterEvent evt) {
      if ((holder.InputState & State.Hovered) != 0) return;
      holder.Enable(State.Hovered);
    }

    protected virtual void OnNavigationMove(NavigationMoveEvent evt) {
      if ((holder.InputState & State.Navigated) != 0) return;
      holder.Enable(State.Navigated);
    }
  }
}

using HELIX.Theming;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public class InputBoundaryComposable<T> : PropsBoundaryComposable<T>, IWidgetStateHolder where T : struct {
    public bool handleFocus;
    private State _inputState;

    public ref State InputState => ref _inputState;

    protected override void OnAttach() {
      base.OnAttach();
      Node.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
      Node.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
      Node.RegisterCallback<FocusInEvent>(OnFocusIn);
      Node.RegisterCallback<FocusOutEvent>(OnFocusOut);
      Node.RegisterCallback<NavigationMoveEvent>(OnNavigationMove);
    }

    protected override void OnDetach() {
      Node.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
      Node.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
      Node.UnregisterCallback<FocusInEvent>(OnFocusIn);
      Node.UnregisterCallback<FocusOutEvent>(OnFocusOut);
      Node.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove);
      base.OnDetach();
    }

    protected virtual void OnFocusOut(FocusOutEvent evt) {
      if (!handleFocus || (InputState & State.Focused) == 0) return;
      this.Disable(State.Focused);
      Node.MarkDirty();
    }

    protected virtual void OnFocusIn(FocusInEvent evt) {
      if (!handleFocus || (InputState & State.Focused) != 0) return;
      this.Enable(State.Focused);
      Node.MarkDirty();
      //if (WidgetStateController.LastNavigated) state.Enable(WidgetState.Navigated);
    }

    protected virtual void OnPointerLeave(PointerLeaveEvent evt) {
      if ((InputState & State.Hovered) == 0) return;
      this.Disable(State.Hovered);
      Node.MarkDirty();
    }

    protected virtual void OnPointerEnter(PointerEnterEvent evt) {
      if ((InputState & State.Hovered) != 0) return;
      this.Enable(State.Hovered);
      Node.MarkDirty();
    }

    protected virtual void OnNavigationMove(NavigationMoveEvent evt) {
      if ((InputState & State.Navigated) != 0) return;
      this.Enable(State.Navigated);
      Node.MarkDirty();
    }
  }
}
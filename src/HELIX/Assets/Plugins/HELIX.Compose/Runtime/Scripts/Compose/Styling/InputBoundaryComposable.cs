using System;
using HELIX.Theming;
using UnityEngine.UIElements;

namespace HELIX.Compose {

  [AttributeUsage(AttributeTargets.Class)]
  [MixinExpression(@"
@CALL<RequireInputState>

@MIXIN<$PostConstruct> global::UnityEngine.UIElements.VisualElementExtensions.AddManipulator(this,
  @\  new InputBoundaryManipulator(this, this, @attr#focus)
  @\);

@SCOPE
  @MATCH @attr#dirty:?eq<true>
  @MIXIN<^OnStateChanged:HELIX.Theming.StateChangedHandler> this.MarkDirty();
@END
")]
  [MixinImport(typeof(ComposeMixinLibrary))]
  public class InputStateListener : Attribute {
    public InputStateListener(bool focus = true, bool hover = true, bool dirty = true) { }
  }

  public class InputBoundaryManipulator : Manipulator {
    public IStateHolder holder;
    public bool focus;
    public bool hover;

    public InputBoundaryManipulator(VisualElement target, IStateHolder holder,
      bool focus = true,
      bool hover = true) {
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
      holder.MarkDirty();
    }

    protected virtual void OnFocusIn(FocusInEvent evt) {
      if (!focus || (holder.InputState & State.Focused) != 0) return;
      holder.Enable(State.Focused);
      holder.MarkDirty();
      //if (WidgetStateController.LastNavigated) state.Enable(WidgetState.Navigated);
    }

    protected virtual void OnPointerLeave(PointerLeaveEvent evt) {
      if ((holder.InputState & State.Hovered) == 0) return;
      holder.Disable(State.Hovered);
      holder.MarkDirty();
    }

    protected virtual void OnPointerEnter(PointerEnterEvent evt) {
      if ((holder.InputState & State.Hovered) != 0) return;
      holder.Enable(State.Hovered);
      holder.MarkDirty();
    }

    protected virtual void OnNavigationMove(NavigationMoveEvent evt) {
      if ((holder.InputState & State.Navigated) != 0) return;
      holder.Enable(State.Navigated);
      holder.MarkDirty();
    }
  }

  public class InputBoundaryComposable<T> : PropsBoundaryComposable<T>, IStateHolder where T : struct {
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

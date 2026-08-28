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

}

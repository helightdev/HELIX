using System;
using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.NW {
  public static class BuiltIns {
    public static ref ElementRef Padding(this ref ElementRef scope, StyleLength4 size) {
      scope.composable.Element.Padding(size);
      scope.composable.DirtyFlags |= UssDirtyFlags.Padding;
      return ref scope;
    }

    public static ref ElementRef BackgroundColor(this ref ElementRef scope, Color color) {
      scope.composable.Element.BackgroundColor(color);
      scope.composable.DirtyFlags |= UssDirtyFlags.Background;
      return ref scope;
    }

    public static ref ElementRef TextColor(this ref ElementRef scope, Color color) {
      scope.composable.Element.TextColor(color);
      scope.composable.DirtyFlags |= UssDirtyFlags.Text;
      return ref scope;
    }

    public static ref ElementRef Display(this ref ElementRef scope, bool display) {
      scope.composable.Element.Display(display);
      scope.composable.DirtyFlags |= UssDirtyFlags.Visibility;
      return ref scope;
    }

    // Flex
    private static readonly ushort _flexId = CompositionId.GetTypeId();

    public static ScopeHandle Flex(
      this ref Composition ctx,
      Axis mainAxis,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      float gap = 0f,
      bool reverse = false
    ) {
      if (ctx.AUTHORING.InitializeNode(_flexId, out var node)) {
        // No state initialization
      }

      node.style.flexDirection = mainAxis.ToFlexDirection(reverse);
      node.style.justifyContent = main;
      node.style.alignItems = cross;
      return ctx.AUTHORING.YieldScope(
        ref ctx,
        node,
        static (BoundaryCell cell, in ScopeHandle _) => {
          cell.TrimChildren();
        }
      );
    }

    // Text
    private static readonly ushort _textId = CompositionId.GetTypeId();

    public static ref ElementRef Text(this ref Composition ctx, string text) {
      if (!ctx.AUTHORING.RequireTracked<Label>(_textId, out var label, out var retained)) {
        label = new Label();
      }

      label.text = text;
      return ref ctx.AUTHORING.YieldElement(ref ctx, label);
    }


    public static ref ElementRef TextField(this ref Composition ctx) {
      if (!ctx.AUTHORING.RequireTracked<TextField>(_textId, out var label, out var retained)) {
        label = new TextField();
      }

      return ref ctx.AUTHORING.YieldElement(ref ctx, label);
    }

    // Boundary
    private static readonly ushort _boundaryId = CompositionId.GetTypeId();

    public static ref ElementRef Boundary(this ref Composition ctx, Composable composable) {
      if (ctx.AUTHORING.InitializeAnonymouseBoundaryNode(_boundaryId, out var node, out var state)) {
        // No state initialization
      }
      node.composable = composable;
      return ref ctx.AUTHORING.YieldBoundary(ref ctx, node);
    }

    public static ref ElementRef Boundary<T>(this ref Composition ctx, T props, Composable composable)
      where T : struct {
      if (ctx.AUTHORING.InitializePropsBoundaryNode<T>(_boundaryId, out var node, out var state)) {
        // No state initialization
      }
      state.props = props;
      node.composable = composable;
      return ref ctx.AUTHORING.YieldBoundary(ref ctx, node);
    }

    public abstract class InputClickableBase<T> : InputStateBase<T> where T : struct {
      private int _activePointerId = -1;

      protected bool Active { get; private set; }
      protected Vector2 LastMousePosition { get; private set; }

      protected override void OnAttach() {
        base.OnAttach();
        Node.RegisterCallback<PointerDownEvent>(OnPointerDown);
        Node.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        Node.RegisterCallback<PointerUpEvent>(OnPointerUp);
        Node.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        Node.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
      }

      protected override void OnDetach() {
        base.OnDetach();
        Node.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        Node.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        Node.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        Node.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
        Node.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
      }

      protected virtual void OnClick(EventBase evt) { }

      protected virtual void OnPointerDown(PointerDownEvent evt) {
        if (Active || evt.button != (int)MouseButton.LeftMouse) return;

        Active = true;
        _activePointerId = evt.pointerId;
        LastMousePosition = evt.localPosition;

        Node.CapturePointer(evt.pointerId);
        this.Enable(StateFlag.Pressed);
        Node.MarkDirty();

        evt.StopImmediatePropagation();
      }

      protected virtual void OnPointerMove(PointerMoveEvent evt) {
        if (!Active) return;

        LastMousePosition = evt.localPosition;
        this.Toggle(StateFlag.Pressed, Node.worldBound.Contains(evt.position));
        Node.MarkDirty();

        evt.StopPropagation();
      }

      protected virtual void OnPointerUp(PointerUpEvent evt) {
        if (!Active || evt.pointerId != _activePointerId) return;

        var clicked = Node.worldBound.Contains(evt.position);

        Active = false;
        _activePointerId = -1;
        LastMousePosition = evt.localPosition;

        Node.ReleasePointer(evt.pointerId);
        this.Disable(StateFlag.Pressed);
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
        this.Disable(StateFlag.Pressed);
        Node.MarkDirty();

        evt.StopPropagation();
      }
    }
  }

  public static partial class ButtonDefinition {

    [CompositionBoundary(Base = typeof(BuiltIns.InputClickableBase<>))]
    public static partial ref ElementRef Button(
      ref this Composition cx,
      [Prop] string label = null,
      [Prop] Action<IBoundary> action = null
    );

    public partial class ButtonState {
      [Context] ContextReference<int> Inline = TestClass.MyKey;

      protected override void OnRecompose(ref Composition cx) {
        var color = Colors.Black;
        if (InputState.Pressed()) color = Colors.Red;
        else if (InputState.Hovered()) color = Colors.Blue;

        cx.Text(Props.Label).TextColor(color);
      }

      protected override void OnClick(EventBase evt) {
        Props.Action?.Invoke(Node);
      }
    }
  }
}
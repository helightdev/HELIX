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
      scope.composable.Flag |= UssFlag.Padding;
      return ref scope;
    }

    public static ref ElementRef Focusable(
      this ref ElementRef scope,
      bool focusable = true,
      int tabIndex = 0,
      bool delegatesFocus = false,
      PickingMode pickingMode = PickingMode.Position
    ) {
      scope.composable.Element.focusable = focusable;
      scope.composable.Element.tabIndex = tabIndex;
      scope.composable.Element.delegatesFocus = delegatesFocus;
      scope.composable.Element.pickingMode = pickingMode;
      scope.composable.Flag |= UssFlag.Focus;
      return ref scope;
    }

    public static ref ElementRef Margin(this ref ElementRef scope, StyleLength4 size) {
      scope.composable.Element.Margin(size);
      scope.composable.Flag |= UssFlag.Margin;
      return ref scope;
    }

    public static ref ElementRef Size(this ref ElementRef scope, BoxConstraints constraints) {
      constraints.Apply(scope.element);
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef Position(this ref ElementRef scope, StyleLength4 position) {
      scope.composable.Element.Position(position);
      scope.composable.Flag |= UssFlag.Position;
      return ref scope;
    }

    public static ref ElementRef Absolute(this ref ElementRef scope, bool absolute = true) {
      scope.composable.Element.style.position =
        absolute ? UnityEngine.UIElements.Position.Absolute : UnityEngine.UIElements.Position.Relative;
      scope.composable.Flag |= UssFlag.Position;
      return ref scope;
    }

    public static ref ElementRef Border(this ref ElementRef scope, Border border) {
      border.Apply(scope.element);
      scope.composable.Flag |= UssFlag.BorderWidth | UssFlag.BorderColor;
      return ref scope;
    }

    public static ref ElementRef BorderRadius(this ref ElementRef scope, BorderRadius radius) {
      radius.Apply(scope.element);
      scope.composable.Flag |= UssFlag.Radius;
      return ref scope;
    }


    public static ref ElementRef BackgroundColor(this ref ElementRef scope, Color color) {
      scope.composable.Element.BackgroundColor(color);
      scope.composable.Flag |= UssFlag.Background;
      return ref scope;
    }

    public static ref ElementRef TextColor(this ref ElementRef scope, Color color) {
      scope.composable.Element.TextColor(color);
      scope.composable.Flag |= UssFlag.Text;
      return ref scope;
    }

    public static ref ElementRef Display(this ref ElementRef scope, bool display) {
      scope.composable.Element.Display(display);
      scope.composable.Flag |= UssFlag.Visibility;
      return ref scope;
    }

    public static ref ElementRef Opacity(this ref ElementRef scope, float opacity) {
      scope.composable.Element.Opacity(opacity);
      scope.composable.Flag |= UssFlag.Visibility;
      return ref scope;
    }

    public static ref ElementRef TextRole(this ref ElementRef scope, TextRole role) {
      if (ThemeData.Context.TryReadScope(out var theme)) {
        theme.GetTextStyleRef(role).Apply(scope.composable);
      }
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
      bool reverse = false,
      bool clear = false
    ) {
      if (ctx.AUTHORING.InitializeNode(_flexId, out var node) || clear) {
        node.hierarchy.Clear();
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


    private static readonly ushort _boxId = CompositionId.GetTypeId();

    public static ref ElementRef DrawSolidBox(
      this ref Composition cx,
      Border? border = null,
      BorderRadius? radius = null,
      Color? color = null,
      float opacity = 1f,
      BoxConstraints constraints = default,
      StyleLength4 position = default,
      bool absolute = false,
      TransitionOptions? transition = null
    ) {
      if (cx.AUTHORING.InitializeNode(_boxId, out var node)) { }
      ref var reference = ref cx.AUTHORING.YieldElement(ref cx, node);

      reference.Border(border ?? Types.Border.None)
        .BorderRadius(radius ?? Types.BorderRadius.None)
        .BackgroundColor(color ?? Colors.Transparent)
        .Opacity(opacity)
        .Absolute(absolute)
        .Size(constraints)
        .Position(position);

      return ref reference;
    }

    // Text
    private static readonly ushort _textId = CompositionId.GetTypeId();

    public static ref ElementRef Text(this ref Composition ctx, string text) {
      if (!ctx.AUTHORING.RequireTracked<Label>(_textId, out var label, out var retained)) {
        label = new Label();
        label.NoPaddingAndMargin();
      }

      label.text = text;

      return ref ctx.AUTHORING.YieldElement(ref ctx, label);
    }


    public static ref ElementRef TextField(this ref Composition ctx) {
      if (!ctx.AUTHORING.RequireTracked<TextField>(_textId, out var label, out var retained)) {
        label = new TextField();
        label.NoPaddingAndMargin();
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

      protected InputClickableBase() {
        handleFocus = true;
      }

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

  public struct DrawSolidBoxStyle {
    public StateProperty<Border> border;
    public StateProperty<BorderRadius> radius;
    public StateProperty<Color> color;
    public StateProperty<float> opacity;
    public StateProperty<BoxConstraints> constraints;
    public StateProperty<StyleLength4> position;
    public StateProperty<bool> absolute;
    public StateProperty<TransitionOptions> transition;

    public DrawSolidBoxStyle(
      StateProperty<Border> border = null,
      StateProperty<BorderRadius> radius = null,
      StateProperty<Color> color = null,
      StateProperty<float> opacity = null,
      StateProperty<BoxConstraints> constraints = null,
      StateProperty<StyleLength4> position = null,
      StateProperty<bool> absolute = null,
      StateProperty<TransitionOptions> transition = null
    ) {
      this.border = border ?? StateProperties.Never<Border>();
      this.radius = radius ?? StateProperties.Never<BorderRadius>();
      this.color = color ?? StateProperties.Never<Color>();
      this.opacity = opacity ?? StateProperties.Never<float>();
      this.constraints = constraints ?? StateProperties.Never<BoxConstraints>();
      this.position = position ?? StateProperties.Never<StyleLength4>();
      this.absolute = absolute ?? StateProperties.Never<bool>();
      this.transition = transition ?? StateProperties.Never<TransitionOptions>();
    }

    public readonly StateComposable Bake() {
      var style = this;
      return (ref Composition cx, StateFlag state) => cx.DrawSolidBox(
        border: style.border.ResolveOrDefault(state, Types.Border.None),
        radius: style.radius.ResolveOrDefault(state, Types.BorderRadius.None),
        color: style.color.ResolveOrDefault(state, Colors.Transparent),
        opacity: style.opacity.ResolveOrDefault(state, 1f),
        constraints: style.constraints.ResolveOrDefault(state, BoxConstraints.Initial),
        position: style.position.ResolveOrDefault(state, StyleLength4.Zero),
        absolute: style.absolute.ResolveOrDefault(state, true),
        transition: style.transition.ResolveOrDefault(state, TransitionOptions.Default)
      );
    }
  }

  public struct ControlBoxStyle {
    public static readonly ControlBoxStyle Default = CommonShapes.ToggleControlBox(BuiltinThemes.DefaultDark);

    public StateProperty<StyleLength4> padding;
    public StateProperty<StyleLength4> margin;
    public StateProperty<Alignment> alignment;
    public StateProperty<BoxConstraints> constraints;
    public StateProperty<TextStyle> textStyle;
    public StateComposable background;

    public ControlBoxStyle(
      StateProperty<StyleLength4> padding = null,
      StateProperty<StyleLength4> margin = null,
      StateProperty<Alignment> alignment = null,
      StateProperty<BoxConstraints> constraints = null,
      StateProperty<TextStyle> textStyle = null,
      StateComposable background = null
    ) {
      this.padding = padding ?? StateProperties.Never<StyleLength4>();
      this.margin = margin ?? StateProperties.Never<StyleLength4>();
      this.alignment = alignment ?? StateProperties.Never<Alignment>();
      this.constraints = constraints ?? StateProperties.Never<BoxConstraints>();
      this.textStyle = textStyle ?? StateProperties.Never<TextStyle>();
      this.background = background;
    }

    public void ApplyColumn(StateFlag flag, IComposable composable) {
      var element = composable.Element;
      constraints.ResolveOrDefault(flag, BoxConstraints.Initial).Apply(element);
      alignment.ResolveOrDefault(flag, Alignment.Center).AlignAsColumn(element);
      element.Padding(padding.ResolveOrDefault(flag, StyleLength4.Zero));
      element.Margin(margin.ResolveOrDefault(flag, StyleLength4.Zero));
      composable.Flag |= UssFlag.GroupAlign | UssFlag.Size | UssFlag.Padding;
    }

    public void RenderBoundary(ref Composition cx, StateFlag state) {
      ApplyColumn(state, cx.boundary);

      TextStyle.WriteMerged(ref cx, textStyle, state).Apply(cx.boundary);

      // var text = InheritableTextStyle.Context.ReadScopeOrDefault();
      // text.Merge(textStyle.ResolveOrDefault(state, InheritableTextStyle.Null));
      // cx.WriteContext(InheritableTextStyle.Context, text);
      // text.Apply(cx.boundary);

      background?.Invoke(ref cx, state);
    }
  }

  public static partial class ButtonDefinition {
    [CompositionBoundary(Base = typeof(BuiltIns.InputClickableBase<>))]
    public static partial ref ElementRef Button(
      ref this Composition cx,
      [Prop] InlineComposable content,
      [Prop] Action<IBoundary> action = null,
      [Prop] bool enabled = true,
      [Prop] bool selected = false,
      [Prop] ControlBoxStyle? style = null
    );

    public static readonly ContextKey<ControlBoxStyle> Style = new("ButtonStyle", ControlBoxStyle.Default);

    public partial class ButtonState {
      protected override void OnRecompose(ref Composition cx) {
        this.Toggle(StateFlag.Selected, Props.Selected);
        this.Toggle(StateFlag.Disabled, !Props.Enabled);
        cx.APPLY.Focusable();

        var boxStyle = Props.Style ?? Style.ReadScopeOrDefault();
        boxStyle.RenderBoundary(ref cx, InputState);
        Props.Content?.Invoke(ref cx);
      }

      protected override void OnClick(EventBase evt) {
        using (HX.BatchScope()) {
          Props.Action?.Invoke(Node);
        }
      }
    }
  }
}
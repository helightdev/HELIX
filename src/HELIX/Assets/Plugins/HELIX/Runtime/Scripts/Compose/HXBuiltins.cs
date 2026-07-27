using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public static partial class HXBuiltins {
    private sealed class TransitionStyleLists {
      public readonly List<TimeValue> durations;
      public readonly List<EasingFunction> easings;
      public readonly List<TimeValue> delays;

      public TransitionStyleLists(in TransitionOptions options) {
        durations = new List<TimeValue>(1) { options.duration };
        easings = new List<EasingFunction>(1) { options.easing };
        delays = new List<TimeValue>(1) { options.delay };
      }
    }

    private static readonly List<StylePropertyName> _allTransitionProperties =
      new(1) { new StylePropertyName("all") };
    private static readonly Dictionary<TransitionOptions, TransitionStyleLists> _transitionStyles = new();

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

    public static ref ElementRef Flexible(this ref ElementRef scope, float grow = 1f, float shrink = 1f) {
      scope.composable.Element.style.flexGrow = grow;
      scope.composable.Element.style.flexShrink = shrink;
      scope.composable.Flag |= UssFlag.Flex;
      return ref scope;
    }

    public static ref ElementRef FlexGrow(this ref ElementRef scope, float grow) {
      scope.composable.Element.style.flexGrow = grow;
      scope.composable.Flag |= UssFlag.Flex;
      return ref scope;
    }

    public static ref ElementRef FlexShrink(this ref ElementRef scope, float shrink) {
      scope.composable.Element.style.flexShrink = shrink;
      scope.composable.Flag |= UssFlag.Flex;
      return ref scope;
    }

    public static ref ElementRef FlexBasis(this ref ElementRef scope, StyleLength basis) {
      scope.composable.Element.style.flexBasis = basis;
      scope.composable.Flag |= UssFlag.Flex;
      return ref scope;
    }

    public static ref ElementRef AlignSelf(this ref ElementRef scope, Align alignment) {
      scope.composable.Element.style.alignSelf = alignment;
      scope.composable.Flag |= UssFlag.GroupAlign;
      return ref scope;
    }

    public static ref ElementRef Width(this ref ElementRef scope, StyleLength width) {
      scope.composable.Element.style.width = width;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef Height(this ref ElementRef scope, StyleLength height) {
      scope.composable.Element.style.height = height;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef MinWidth(this ref ElementRef scope, StyleLength width) {
      scope.composable.Element.style.minWidth = width;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef MinHeight(this ref ElementRef scope, StyleLength height) {
      scope.composable.Element.style.minHeight = height;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef MaxWidth(this ref ElementRef scope, StyleLength width) {
      scope.composable.Element.style.maxWidth = width;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef MaxHeight(this ref ElementRef scope, StyleLength height) {
      scope.composable.Element.style.maxHeight = height;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef AspectRatio(this ref ElementRef scope, float ratio) {
      scope.composable.Element.style.aspectRatio = ratio;
      scope.composable.Flag |= UssFlag.Size;
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

    public static ref ElementRef BackgroundImage(this ref ElementRef scope, Background image) {
      scope.composable.Element.style.backgroundImage = image;
      scope.composable.Flag |= UssFlag.Background;
      return ref scope;
    }

    public static ref ElementRef BackgroundSize(this ref ElementRef scope, BackgroundSize size) {
      scope.composable.Element.style.backgroundSize = size;
      scope.composable.Flag |= UssFlag.Background;
      return ref scope;
    }

    public static ref ElementRef BackgroundTint(this ref ElementRef scope, Color color) {
      scope.composable.Element.style.unityBackgroundImageTintColor = color;
      scope.composable.Flag |= UssFlag.BackgroundAdvanced;
      return ref scope;
    }

    public static ref ElementRef TextColor(this ref ElementRef scope, Color color) {
      scope.composable.Element.TextColor(color);
      scope.composable.Flag |= UssFlag.Text;
      return ref scope;
    }

    public static ref ElementRef TextSize(this ref ElementRef scope, StyleLength size) {
      scope.composable.Element.style.fontSize = size;
      scope.composable.Flag |= UssFlag.Text;
      return ref scope;
    }

    public static ref ElementRef TextFont(this ref ElementRef scope, StyleFont font) {
      scope.composable.Element.style.unityFont = font;
      scope.composable.Flag |= UssFlag.Text;
      return ref scope;
    }

    public static ref ElementRef TextFont(this ref ElementRef scope, StyleFontDefinition font) {
      scope.composable.Element.style.unityFontDefinition = font;
      scope.composable.Flag |= UssFlag.Text;
      return ref scope;
    }

    public static ref ElementRef TextAlign(this ref ElementRef scope, TextAnchor alignment) {
      scope.composable.Element.style.unityTextAlign = alignment;
      scope.composable.Flag |= UssFlag.Text;
      return ref scope;
    }

    public static ref ElementRef WhiteSpace(this ref ElementRef scope, WhiteSpace whiteSpace) {
      scope.composable.Element.style.whiteSpace = whiteSpace;
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

    public static ref ElementRef Visible(this ref ElementRef scope, bool visible) {
      scope.composable.Element.style.visibility =
        visible ? Visibility.Visible : Visibility.Hidden;
      scope.composable.Flag |= UssFlag.Visibility;
      return ref scope;
    }

    public static ref ElementRef Overflow(this ref ElementRef scope, Overflow overflow) {
      scope.composable.Element.style.overflow = overflow;
      scope.composable.Flag |= UssFlag.Clipping;
      return ref scope;
    }

    public static ref ElementRef Translate(this ref ElementRef scope, Translate translate) {
      scope.composable.Element.style.translate = translate;
      scope.composable.Flag |= UssFlag.Transform;
      return ref scope;
    }

    public static ref ElementRef Rotate(this ref ElementRef scope, Rotate rotate) {
      scope.composable.Element.style.rotate = rotate;
      scope.composable.Flag |= UssFlag.Transform;
      return ref scope;
    }

    public static ref ElementRef Scale(this ref ElementRef scope, Scale scale) {
      scope.composable.Element.style.scale = scale;
      scope.composable.Flag |= UssFlag.Transform;
      return ref scope;
    }

    public static ref ElementRef TransformOrigin(
      this ref ElementRef scope,
      TransformOrigin origin
    ) {
      scope.composable.Element.style.transformOrigin = origin;
      scope.composable.Flag |= UssFlag.Transform;
      return ref scope;
    }

    public static ref ElementRef Cursor(
      this ref ElementRef scope,
      UnityEngine.UIElements.Cursor cursor
    ) {
      scope.composable.Element.style.cursor = cursor;
      scope.composable.Flag |= UssFlag.Special;
      return ref scope;
    }

    public static ref ElementRef Transition(
      this ref ElementRef scope,
      TransitionOptions options
    ) {
      if (!_transitionStyles.TryGetValue(options, out var lists)) {
        lists = new TransitionStyleLists(in options);
        _transitionStyles.Add(options, lists);
      }
      var style = scope.composable.Element.style;
      style.transitionProperty = _allTransitionProperties;
      style.transitionDuration = lists.durations;
      style.transitionTimingFunction = lists.easings;
      style.transitionDelay = lists.delays;
      scope.composable.Flag |= UssFlag.Transition;
      return ref scope;
    }

    public static ref ElementRef TextRole(this ref ElementRef scope, TextRole role) {
      ThemeData.Context.ReadScope().GetTextStyleRef(role).Apply(scope.composable);
      // if (ThemeData.Context.TryReadScope(out var theme)) {
      //   theme.GetTextStyleRef(role).Apply(scope.composable);
      // } else {
      //   Debug.LogError($"Theme not found for TextRole: {role}");
      // }
      return ref scope;
    }

    public static ref ElementRef Class(this ref ElementRef scope, string className, bool enabled = true) {
      scope.composable.Element.EnableInClassList(className, enabled);
      scope.composable.Flag |= UssFlag.Classes;
      return ref scope;
    }

    public static ref ElementRef Fill(this ref ElementRef scope) {
      return ref scope.Flexible().AlignSelf(Align.Stretch);
    }


    // Space
    private static readonly ushort _spaceId = CompositionId.GetTypeId();

    public static ref ElementRef Spacing(this ref Composition cx, SpacingRole role) {
      var theme = ThemeData.Context.ReadScope();
      var gap = theme.GetSpacing(role);
      return ref cx.Gap(gap);
    }

    public static ref ElementRef Spacing(this ref Composition cx, int level) {
      var role = (SpacingRole)level;
      return ref cx.Spacing(role);
    }

    public static ref ElementRef Gap(this ref Composition cx, Length? gap = null) {
      if (cx.AUTHORING.InitializeNode(_spaceId, out var node)) { }
      ref var reference = ref cx.AUTHORING.YieldElement(ref cx, node);

      if (gap == null) {
        reference.Size(BoxConstraints.Initial);
        reference.FlexGrow(1);
      } else {
        var isParentHorizontal = cx.AUTHORING.cell.current.Element.style.flexDirection == FlexDirection.Row;
        if (isParentHorizontal) {
          reference.Width((StyleLength)gap);
          reference.FlexGrow(0);
        } else {
          reference.Height((StyleLength)gap);
          reference.FlexGrow(0);
        }
      }

      reference.element.name = "Gap";
      reference.composable.Flag |= UssFlag.Name;

      return ref reference;
    }


    // Flex
    private static readonly ushort _flexId = CompositionId.GetTypeId();

    public static ScopeHandle Flex(
      this ref Composition ctx,
      Axis mainAxis,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      bool reverse = false,
      bool clear = false
    ) {
      if (ctx.AUTHORING.InitializeNode(_flexId, out var node) || clear) {
        node.hierarchy.Clear();
      }

      node.style.flexDirection = mainAxis.ToFlexDirection(reverse);
      node.style.justifyContent = main;
      node.style.alignItems = cross;
      node.Flag |= UssFlag.GroupAlign;

      return ctx.AUTHORING.YieldScope(
        ref ctx,
        node,
        static (BoundaryCell cell, in ScopeHandle _) => {
          cell.TrimChildren();
        }
      );
    }

    // Container
    private static readonly ushort _containerId = CompositionId.GetTypeId();

    public static ScopeHandle Container(this ref Composition cx) {
      if (cx.AUTHORING.InitializeNode(_flexId, out var node)) {
        node.hierarchy.Clear();
      }

      return cx.AUTHORING.YieldScope(
        ref cx,
        node,
        static (BoundaryCell cell, in ScopeHandle _) => {
          cell.TrimChildren();
        }
      );
    }


    // Solid box
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

      if (transition.HasValue) {
        reference.Transition(transition.Value);
      } else if ((reference.composable.Flag & UssFlag.Transition) != 0) {
        UssFlag.Transition.ClearFlags(reference.element);
        reference.composable.Flag &= ~UssFlag.Transition;
      }

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
  }

  public static partial class ButtonDefinition {
    [CompositionBoundary(Base = typeof(InputClickableComposable<>))]
    public static partial ref ElementRef Button(
      ref this Composition cx,
      [Prop] Composable content,
      [Prop] CompositionAction action = null,
      [Prop] bool enabled = true,
      [Prop] bool selected = false,
      [Prop] ControlBoxStyle? style = null,
      [Prop] PrefixLabelSuffixSpec? presentation = null
    );

    public static ref ElementRef Button(
      ref this Composition cx,
      in PrefixLabelSuffixSpec presentation,
      CompositionAction action = null,
      bool enabled = true,
      bool selected = false,
      ControlBoxStyle? style = null
    ) {
      return ref Button(ref cx, null, action, enabled, selected, style, presentation);
    }

    public static readonly ContextKey<ControlBoxStyle> Style = new("ButtonStyle", ControlBoxStyle.Default);

    public partial class ButtonComposable {
      protected override void OnRecompose(ref Composition cx) {
        this.Toggle(StateFlag.Selected, Props.Selected);
        this.Toggle(StateFlag.Disabled, !Props.Enabled);
        Node.SetEnabled(Props.Enabled);
        cx.APPLY.Focusable(Props.Enabled);

        var boxStyle = Props.Style ?? Style.ReadScope();
        boxStyle.RenderBoundary(ref cx, InputState);
        if (Props.Content != null) Props.Content.Invoke(ref cx);
        else if (Props.Presentation.HasValue) {
          var presentation = Props.Presentation.Value;
          cx.PrefixLabelSuffix(in presentation);
        }
      }

      protected override void OnClick(EventBase evt) {
        if (!Props.Enabled) return;
        Props.Action?.Call(Node);
      }
    }
  }
}
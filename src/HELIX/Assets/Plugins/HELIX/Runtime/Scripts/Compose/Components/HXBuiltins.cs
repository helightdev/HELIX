using System.Collections.Generic;
using HELIX.Coloring;
using HELIX.Extensions;
using HELIX.Theming;
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
      scope.element.Padding(size);
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
      scope.element.focusable = focusable;
      scope.element.tabIndex = tabIndex;
      scope.element.delegatesFocus = delegatesFocus;
      scope.element.pickingMode = pickingMode;
      scope.composable.Flag |= UssFlag.Focus;
      return ref scope;
    }

    public static ref ElementRef Margin(this ref ElementRef scope, StyleLength4 size) {
      scope.element.Margin(size);
      scope.composable.Flag |= UssFlag.Margin;
      return ref scope;
    }

    public static ref ElementRef Size(this ref ElementRef scope, BoxConstraints constraints) {
      constraints.Apply(scope.element);
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef Position(this ref ElementRef scope, StyleLength4 position) {
      scope.element.Position(position);
      scope.composable.Flag |= UssFlag.Position;
      return ref scope;
    }

    public static ref ElementRef Flexible(this ref ElementRef scope, float grow = 1f, float shrink = 1f) {
      scope.element.style.flexGrow = grow;
      scope.element.style.flexShrink = shrink;
      scope.composable.Flag |= UssFlag.Flex;
      return ref scope;
    }

    public static ref ElementRef FlexGrow(this ref ElementRef scope, float grow) {
      scope.element.style.flexGrow = grow;
      scope.composable.Flag |= UssFlag.Flex;
      return ref scope;
    }

    public static ref ElementRef FlexShrink(this ref ElementRef scope, float shrink) {
      scope.element.style.flexShrink = shrink;
      scope.composable.Flag |= UssFlag.Flex;
      return ref scope;
    }

    public static ref ElementRef FlexBasis(this ref ElementRef scope, StyleLength basis) {
      scope.element.style.flexBasis = basis;
      scope.composable.Flag |= UssFlag.Flex;
      return ref scope;
    }

    public static ref ElementRef AlignSelf(this ref ElementRef scope, Align alignment) {
      scope.element.style.alignSelf = alignment;
      scope.composable.Flag |= UssFlag.GroupAlign;
      return ref scope;
    }

    public static ref ElementRef Width(this ref ElementRef scope, StyleLength width) {
      scope.element.style.width = width;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef Height(this ref ElementRef scope, StyleLength height) {
      scope.element.style.height = height;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef MinWidth(this ref ElementRef scope, StyleLength width) {
      scope.element.style.minWidth = width;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef MinHeight(this ref ElementRef scope, StyleLength height) {
      scope.element.style.minHeight = height;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef MaxWidth(this ref ElementRef scope, StyleLength width) {
      scope.element.style.maxWidth = width;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef MaxHeight(this ref ElementRef scope, StyleLength height) {
      scope.element.style.maxHeight = height;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef AspectRatio(this ref ElementRef scope, float ratio) {
      scope.element.style.aspectRatio = ratio;
      scope.composable.Flag |= UssFlag.Size;
      return ref scope;
    }

    public static ref ElementRef Absolute(this ref ElementRef scope, bool absolute = true) {
      scope.element.style.position =
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
      scope.element.BackgroundColor(color);
      scope.composable.Flag |= UssFlag.Background;
      return ref scope;
    }

    public static ref ElementRef BackgroundImage(this ref ElementRef scope, Background image) {
      scope.element.style.backgroundImage = image;
      scope.composable.Flag |= UssFlag.Background;
      return ref scope;
    }

    public static ref ElementRef BackgroundSize(this ref ElementRef scope, BackgroundSize size) {
      scope.element.style.backgroundSize = size;
      scope.composable.Flag |= UssFlag.Background;
      return ref scope;
    }

    public static ref ElementRef BackgroundTint(this ref ElementRef scope, Color color) {
      scope.element.style.unityBackgroundImageTintColor = color;
      scope.composable.Flag |= UssFlag.BackgroundAdvanced;
      return ref scope;
    }

    public static ref ElementRef TextColor(this ref ElementRef scope, Color color) {
      scope.element.TextColor(color);
      scope.composable.Flag |= UssFlag.Text;
      return ref scope;
    }

    public static ref ElementRef TextSize(this ref ElementRef scope, StyleLength size) {
      scope.element.style.fontSize = size;
      scope.composable.Flag |= UssFlag.Text;
      return ref scope;
    }

    public static ref ElementRef TextFont(this ref ElementRef scope, StyleFont font) {
      scope.element.style.unityFont = font;
      scope.composable.Flag |= UssFlag.Text;
      return ref scope;
    }

    public static ref ElementRef TextFont(this ref ElementRef scope, StyleFontDefinition font) {
      scope.element.style.unityFontDefinition = font;
      scope.composable.Flag |= UssFlag.Text;
      return ref scope;
    }

    public static ref ElementRef TextAlign(this ref ElementRef scope, TextAnchor alignment) {
      scope.element.style.unityTextAlign = alignment;
      scope.composable.Flag |= UssFlag.Text;
      return ref scope;
    }

    public static ref ElementRef WhiteSpace(this ref ElementRef scope, WhiteSpace whiteSpace) {
      scope.element.style.whiteSpace = whiteSpace;
      scope.composable.Flag |= UssFlag.Text;
      return ref scope;
    }

    public static ref ElementRef Display(this ref ElementRef scope, bool display) {
      scope.element.Display(display);
      scope.composable.Flag |= UssFlag.Visibility;
      return ref scope;
    }

    public static ref ElementRef Opacity(this ref ElementRef scope, float opacity) {
      scope.element.Opacity(opacity);
      scope.composable.Flag |= UssFlag.Visibility;
      return ref scope;
    }

    public static ref ElementRef Visible(this ref ElementRef scope, bool visible) {
      scope.element.style.visibility =
        visible ? Visibility.Visible : Visibility.Hidden;
      scope.composable.Flag |= UssFlag.Visibility;
      return ref scope;
    }

    public static ref ElementRef Overflow(this ref ElementRef scope, Overflow overflow) {
      scope.element.style.overflow = overflow;
      scope.composable.Flag |= UssFlag.Clipping;
      return ref scope;
    }

    public static ref ElementRef Translate(this ref ElementRef scope, Translate translate) {
      scope.element.style.translate = translate;
      scope.composable.Flag |= UssFlag.Transform;
      return ref scope;
    }

    public static ref ElementRef Rotate(this ref ElementRef scope, Rotate rotate) {
      scope.element.style.rotate = rotate;
      scope.composable.Flag |= UssFlag.Transform;
      return ref scope;
    }

    public static ref ElementRef Scale(this ref ElementRef scope, Scale scale) {
      scope.element.style.scale = scale;
      scope.composable.Flag |= UssFlag.Transform;
      return ref scope;
    }

    public static ref ElementRef TransformOrigin(
      this ref ElementRef scope,
      TransformOrigin origin
    ) {
      scope.element.style.transformOrigin = origin;
      scope.composable.Flag |= UssFlag.Transform;
      return ref scope;
    }

    public static ref ElementRef Cursor(
      this ref ElementRef scope,
      UnityEngine.UIElements.Cursor cursor
    ) {
      scope.element.style.cursor = cursor;
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
      var style = scope.element.style;
      style.transitionProperty = _allTransitionProperties;
      style.transitionDuration = lists.durations;
      style.transitionTimingFunction = lists.easings;
      style.transitionDelay = lists.delays;
      scope.composable.Flag |= UssFlag.Transition;
      return ref scope;
    }

    public static ref ElementRef TextRole(this ref ElementRef scope, TextRole role) {
      var data = ThemeData.Key[scope];
      ref var style = ref data[role].style;
      style.Apply(scope.composable);
      return ref scope;
    }

    public static ref ElementRef Class(this ref ElementRef scope, string className, bool enabled = true) {
      scope.element.EnableInClassList(className, enabled);
      scope.composable.Flag |= UssFlag.Classes;
      return ref scope;
    }

    public static ref ElementRef Name(this ref ElementRef scope, string name) {
      scope.element.name = name;
      scope.composable.Flag |= UssFlag.Name;
      return ref scope;
    }

    public static ref ElementRef Fill(this ref ElementRef scope) {
      return ref scope.Flexible().AlignSelf(Align.Stretch);
    }


    // Space
    private static readonly ushort _spaceId = CompositionId.GetTypeId();

    public static ref ElementRef Spacing(this ref Composition cx, SpacingRole role) {
      var theme = cx.ReadContext(ThemeData.Key);
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
        var isParentHorizontal = cx.AUTHORING.cell.scope.Element.style.flexDirection == FlexDirection.Row;
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

    // Context Scope
    private static readonly ushort _contextScopeId = CompositionId.GetTypeId();

    public static ScopeHandle ContextContributor(this ref Composition cx) {
      cx.AUTHORING.RequireComposable<ContextComposableElement>(
        _contextScopeId, out var contributor, out var retained
      );

      return cx.AUTHORING.YieldScope(
        ref cx,
        contributor,
        static (BoundaryCell cell, in ScopeHandle _) => {
          cell.TrimChildren();
        }
      );
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
      BoxConstraints? constraints = null,
      StyleLength4? position = null,
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
        .Size(constraints ?? BoxConstraints.Tight(StyleKeyword.Null, StyleKeyword.Null))
        .Position(position ?? new StyleLength4(StyleKeyword.Null));

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
      [Prop] HXControlBoxStyle? style = null
    );

    public static ref ElementRef Button(
      ref this Composition cx,
      CompositionAction action = null,
      bool enabled = true,
      bool selected = false,
      HXControlBoxStyle? style = null
    ) {
      return ref Button(ref cx, null, action, enabled, selected, style);
    }

    public static readonly ContextKey<HXControlBoxStyle> Style = new("ButtonStyle", HXControlBoxStyle.Default);

    public partial class ButtonComposable {
      protected override void OnRecompose(ref Composition cx) {
        this.Toggle(State.Selected, props.Selected);
        this.Toggle(State.Disabled, !props.Enabled);
        Node.SetEnabled(props.Enabled);
        cx.CURSOR.Focusable(props.Enabled);

        var boxStyle = props.Style ?? Style.ReadOrThemeProperty(in cx, ThemeProperties.ButtonFilled);
        boxStyle.RenderBoundary(ref cx, InputState);
        if (props.Content != null) props.Content.Invoke(ref cx);
      }

      protected override void OnClick(EventBase evt) {
        if (!props.Enabled) return;
        props.Action?.Call(Node);
      }
    }
  }
}
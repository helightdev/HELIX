using HELIX.Coloring;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.Compose {
  public static class CommonElements {
    // Space
    private static readonly ushort _spaceId = CompositionId.GetTypeId("Spacing");

    // Context Scope
    private static readonly ushort _contextScopeId = CompositionId.GetTypeId("Context Contributor");


    // Flex
    private static readonly ushort _groupId = CompositionId.GetTypeId("Group");

    // Container
    private static readonly ushort _containerId = CompositionId.GetTypeId("Container");


    // Solid box
    private static readonly ushort _boxId = CompositionId.GetTypeId("DrawSolidBox");


    // Solid box
    private static readonly ushort _image = CompositionId.GetTypeId("Image");

    // Text
    private static readonly ushort _textId = CompositionId.GetTypeId("Text");


    private static readonly ushort _textFieldId = CompositionId.GetTypeId("TextField");

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

    public static ScopeHandle ContextContributor(this ref Composition cx) {
      if (!cx.AUTHORING.RequireComposable<ContextComposableElement>(_contextScopeId, out var contributor, out _))
        contributor = new ContextComposableElement { PackedId = cx.AUTHORING.id.packed };

      return cx.AUTHORING.YieldScope(
        ref cx,
        contributor,
        static (BoundaryCell cell, in ScopeHandle _) => {
          cell.TrimChildren();
        }
      );
    }

    public static ScopeHandle Group(
      this ref Composition ctx,
      Axis mainAxis,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      bool reverse = false,
      bool clear = false
    ) {
      if (ctx.AUTHORING.InitializeNode(_groupId, out var node) || clear) node.hierarchy.Clear();

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

    public static ScopeHandle Group(
      this ref Composition ctx,
      FlexGroup group,
      Flex? flex = null,
      bool clear = false
    ) {
      if (ctx.AUTHORING.InitializeNode(_groupId, out var node) || clear) node.hierarchy.Clear();

      group.Apply(node);
      (flex ?? Flex.Null).Apply(node);
      node.Flag |= UssFlag.GroupAlign | UssFlag.Flex;

      return ctx.AUTHORING.YieldScope(
        ref ctx,
        node,
        static (BoundaryCell cell, in ScopeHandle _) => {
          cell.TrimChildren();
        }
      );
    }

    public static ScopeHandle Row(
      this ref Composition cx,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      bool reverse = false,
      Flex? flex = null,
      bool clear = false
    ) {
      return cx.Group(FlexGroup.Row(main, cross, reverse), flex, clear);
    }

    public static ScopeHandle Column(
      this ref Composition cx,
      Justify main = Justify.FlexStart,
      Align cross = Align.Center,
      bool reverse = false,
      Flex? flex = null,
      bool clear = false
    ) {
      return cx.Group(FlexGroup.Column(main, cross, reverse), flex, clear);
    }

    public static ScopeHandle Container(this ref Composition cx) {
      if (cx.AUTHORING.InitializeNode(_groupId, out var node)) node.hierarchy.Clear();

      return cx.AUTHORING.YieldScope(
        ref cx,
        node,
        static (BoundaryCell cell, in ScopeHandle _) => {
          cell.TrimChildren();
        }
      );
    }

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

      reference.Border(border ?? Border.None)
        .BorderRadius(radius ?? BorderRadius.None)
        .BackgroundColor(color ?? Colors.Transparent)
        .Opacity(opacity)
        .Absolute(absolute)
        .Size(constraints ?? BoxConstraints.Tight(StyleKeyword.Null, StyleKeyword.Null))
        .Position(position ?? new StyleLength4(StyleKeyword.Null));

      if (transition.HasValue) reference.Transition(TransitionPreset.Colors, transition.Value);
      else if ((reference.composable.Flag & UssFlag.Transition) != 0) reference.Transition(null, default);

      return ref reference;
    }

    public static ref ElementRef DrawImage(
      this ref Composition cx,
      in BackgroundImage? image,
      Color? tint = null,
      Color? background = null,
      Border? border = null,
      BorderRadius? radius = null,
      float opacity = 1f,
      BoxConstraints? constraints = null,
      StyleLength4? position = null,
      bool absolute = false,
      TransitionOptions? transition = null
    ) {
      if (cx.AUTHORING.InitializeNode(_image, out var node)) { }
      ref var reference = ref cx.AUTHORING.YieldElement(ref cx, node);

      reference.Border(border ?? Border.None)
        .BorderRadius(radius ?? BorderRadius.None)
        .BackgroundColor(background ?? Colors.Transparent)
        .BackgroundImage(image)
        .BackgroundTint(tint ?? Colors.White)
        .Opacity(opacity)
        .Absolute(absolute)
        .Name("Image")
        .Size(constraints ?? BoxConstraints.Tight(StyleKeyword.Null, StyleKeyword.Null))
        .Position(position ?? new StyleLength4(StyleKeyword.Null));


      if (transition.HasValue) reference.Transition(TransitionPreset.Colors, transition.Value);
      else if ((reference.composable.Flag & UssFlag.Transition) != 0) reference.Transition(null, default);

      return ref reference;
    }

    public static ref ElementRef DrawImage(
      this ref Composition cx,
      BackgroundImage image,
      Color? tint = null,
      Color? background = null,
      Border? border = null,
      BorderRadius? radius = null,
      float opacity = 1f,
      BoxConstraints? constraints = null,
      StyleLength4? position = null,
      bool absolute = false,
      TransitionOptions? transition = null
    ) {
      if (cx.AUTHORING.InitializeNode(_image, out var node)) { }
      ref var reference = ref cx.AUTHORING.YieldElement(ref cx, node);

      reference.Border(border ?? Border.None)
        .BorderRadius(radius ?? BorderRadius.None)
        .BackgroundColor(background ?? Colors.Transparent)
        .BackgroundImage(image)
        .BackgroundTint(tint ?? Colors.White)
        .Opacity(opacity)
        .Absolute(absolute)
        .Size(constraints ?? BoxConstraints.Tight(StyleKeyword.Null, StyleKeyword.Null))
        .Position(position ?? new StyleLength4(StyleKeyword.Null));


      if (transition.HasValue) reference.Transition(TransitionPreset.Colors, transition.Value);
      else if ((reference.composable.Flag & UssFlag.Transition) != 0) reference.Transition(null, default);

      return ref reference;
    }

    public static ref ElementRef Text(this ref Composition ctx, string text) {
      if (!ctx.AUTHORING.RequireTracked<Label>(_textId, out var label, out var retained)) {
        label = new Label();
        label.NoPaddingAndMargin();
      }

      label.text = text;
      return ref ctx.AUTHORING.YieldElement(ref ctx, label);
    }
    /*
     *      if (ctx.TryReadContextData(TextStyle.Key, out var data)) {
         data.value.Apply(element.composable);
       }
     *
     */
    /*
     *      if (ctx.TryReadContextData(TextStyle.Key, out var data)) {
         data.value.Apply(element.composable);
       }
     *
     */

    public static ref ElementRef Text(this ref Composition cx, string text, TextRole role) {
      ref var elementRef = ref cx.Text(text);
      var data = ThemeData.Key[in cx];
      ref var style = ref data[role].style;
      style.Apply(elementRef.composable);
      return ref elementRef;
    }

    public static ref ElementRef TextField(this ref Composition ctx) {
      if (!ctx.AUTHORING.RequireTracked<TextField>(_textFieldId, out var label, out var retained)) {
        label = new TextField();
        label.NoPaddingAndMargin();
      }

      return ref ctx.AUTHORING.YieldElement(ref ctx, label);
    }

    public static ref ElementRef Boundary(this ref Composition ctx, Composable composable) {
      if (!ctx.AUTHORING.RequireComposable<AnonymousBoundaryElement>(
        AnonymousBoundaryElement.typeId,
        out var boundary,
        out _
      )) boundary = new AnonymousBoundaryElement();
      boundary.content = composable;
      return ref ctx.AUTHORING.YieldBoundary(ref ctx, boundary);
    }

    public static ref ElementRef Boundary<T>(this ref Composition ctx, T props, Composable composable)
    where T : struct {
      if (!ctx.AUTHORING.RequireComposable<PropsBoundaryElement<T>>(
        PropsBoundaryElement<T>.typeId,
        out var boundary,
        out _
      )) boundary = new PropsBoundaryElement<T>();
      boundary.props = props;
      boundary.content = composable;
      return ref ctx.AUTHORING.YieldBoundary(ref ctx, boundary);
    }
  }

  [Mixable]
  [BoundaryElementMixin(false)]
  internal partial class AnonymousBoundaryElement {
    internal static readonly ushort typeId = CompositionId.GetTypeId(nameof(AnonymousBoundaryElement));
    internal Composable content;

    [Hook]
    private void OnCompose(ref Composition cx) {
      content?.Invoke(ref cx);
    }
  }

  [Mixable]
  [BoundaryElementMixin(false)]
  internal partial class PropsBoundaryElement<T> where T : struct {
    internal static readonly ushort typeId = CompositionId.GetTypeId(nameof(PropsBoundaryElement<T>));
    internal Composable content;
    internal T props;

    [Hook]
    private void OnCompose(ref Composition cx) {
      content?.Invoke(ref cx);
    }
  }
}
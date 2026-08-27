using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Compose.Forms;
using HELIX.Prose;
using HELIX.Signals;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine;
using UnityEngine.UIElements;

namespace HELIX.UI.Options {
  public sealed class OptionPageDefaultResetModifier : IProseModifier {
    public OptionPageDefaultResetModifier(bool enabled) => Enabled = enabled;
    public bool Enabled { get; }
  }

  public static class OptionPageFieldModifiers {
    public static OptionPageDefaultResetModifier DefaultReset(bool enabled = true) => new(enabled);
    public static OptionPageDefaultResetModifier DisableDefaultReset() => new(false);
  }

  public sealed class OptionPageFieldPresentation {
    public OptionPageFieldPresentation(Composable label, Composable description, Composable tooltip) {
      Label = label;
      Description = description;
      Tooltip = tooltip;
    }
    public Composable Label { get; }
    public Composable Description { get; }
    public Composable Tooltip { get; }
  }

  public sealed class OptionPageFieldController : Signal {
    public OptionPageFieldPresentation Current { get; private set; }

    public void Show(OptionPageFieldPresentation presentation) {
      if (ReferenceEquals(Current, presentation)) return;
      Current = presentation;
      NotifyDirty();
      NotifyObservers();
    }
  }

  public readonly struct OptionPageFieldContext {
    public static readonly ContextKey<OptionPageFieldContext> Key = new("OptionPageFieldContext");
    public readonly OptionPageFieldController controller;
    public readonly FormController form;
    public readonly OptionPagesOptions options;
    public readonly IOptionPagesState state;
    public OptionPageFieldContext(
      OptionPageFieldController controller, FormController form, OptionPagesOptions options,
      IOptionPagesState state
    ) {
      this.controller = controller;
      this.form = form;
      this.options = options;
      this.state = state;
    }
  }

  [EnableMixins]
  [BoundaryComposableMixin]
  public partial class OptionPageFieldElement {
    public partial struct Props {
      public FormController form;
      public OptionPageFieldController controller;
      public OptionPagesOptions options;
      [Prop(null)] public IOptionPagesState state;
      [Prop(null)] public string path;
      [Prop(null, Equatable = false)] public OptionPageFieldPresentation presentation;
      [Prop(null)] public Composable content;
    }

    private OptionPageFieldController _controller;

    protected override void OnAttach() {
      base.OnAttach();
      Node.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
      Node.RegisterCallback<FocusInEvent>(OnFocusIn);
    }

    protected override void OnDetach() {
      Node.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
      Node.UnregisterCallback<FocusInEvent>(OnFocusIn);
      _controller = null;
      base.OnDetach();
    }

    protected override void OnRecompose(ref Composition cx) {
      _controller = props.controller;
      using (cx.WriteContext(out var context)) FormContext.Key[context] = new FormContext(props.form);
      Node.style.alignSelf = Align.Stretch;
      props.content?.Invoke(ref cx);
    }

    internal static void ComposeChangedResetIcon(ref Composition cx) {
      var field = cx.Lookup<OptionPageFieldElement>();
      (field?.props.options.changedOptionResetIcon ?? FaSolidIcons.Ref(FaSolidIcons.ArrowRotateLeft)).Compose(ref cx);
    }

    internal static void ComposeDefaultResetIcon(ref Composition cx) {
      var field = cx.Lookup<OptionPageFieldElement>();
      (field?.props.options.defaultOptionResetIcon ?? FaSolidIcons.Ref(FaSolidIcons.ClockRotateLeft)).Compose(ref cx);
    }

    internal static void ResetChange(CompositionContext context) {
      var field = context.Lookup<OptionPageFieldElement>();
      field?.props.state?.ResetChange(field.props.path);
    }

    internal static void ResetToDefault(CompositionContext context) {
      var field = context.Lookup<OptionPageFieldElement>();
      field?.props.state?.ResetToDefault(field.props.path);
    }

    private void OnPointerEnter(PointerEnterEvent evt) => _controller?.Show(props.presentation);
    private void OnFocusIn(FocusInEvent evt) => _controller?.Show(props.presentation);
  }

  [EnableMixins]
  [BoundaryComposableMixin]
  public partial class OptionPageTooltipElement {
    public partial struct Props {
      [Prop(null)] public Composable trigger;
      [Prop(null)] public Composable content;
    }

    private OverlayController _overlays;
    private OverlayHandle _overlay;

    protected override void OnAttach() {
      base.OnAttach();
      Node.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
      Node.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
    }

    protected override void OnDetach() {
      Node.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
      Node.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
      _overlay?.Dismiss(OverlayDismissReason.AnchorDetached);
      _overlay = null;
      _overlays = null;
      base.OnDetach();
    }

    protected override void OnRecompose(ref Composition cx) {
      _overlays = cx.Overlays(false);
      Node.style.flexGrow = 0f;
      Node.style.flexShrink = 0f;
      Node.style.alignSelf = Align.Center;
      Node.pickingMode = PickingMode.Position;
      if (props.trigger != null) props.trigger(ref cx);
      else cx.Text("\u24D8", TextRole.BodySmall).Opacity(0.7f);
    }

    private void OnPointerEnter(PointerEnterEvent evt) {
      if (props.content == null || _overlays == null || _overlay?.IsOpen == true) return;
      _overlay = Overlay.Build(ComposeTooltip)
        .AnchorTo(Node, OverlayPlacement.Above, new Vector2(0f, -4f))
        .Show(_overlays, Node);
    }

    private void OnPointerLeave(PointerLeaveEvent evt) {
      _overlay?.Dismiss();
      _overlay = null;
    }

    private void ComposeTooltip(ref Composition cx, OverlayContextData overlay) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainerHighest))
        .TextColor(theme.GetColor(ColorRoles.OnSurface))
        .BorderRadius(8f)
        .MaxWidth(320f);
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(10f);
        props.content?.Invoke(ref cx);
      }
    }
  }

  /// <summary>Consumes option-page help prose, delegates control creation, then wraps the completed field.</summary>
  public static class OptionPageFieldFactory {
    public static bool Create(
      IProseField field, object formatter, IReadOnlyList<ComposeProseFieldPart> parts,
      IReadOnlyList<IProseModifier> modifiers, out Composable result
    ) {
      var delegatedParts = new List<ComposeProseFieldPart>(parts.Count);
      Composable label = null;
      Composable description = null;
      Composable tooltip = null;
      for (var i = 0; i < parts.Count; i++) {
        var content = parts[i].Content;
        if (parts[i].Scope is ProseFieldPart { Kind: ProseFieldPartKind.Label }) label = content;
        if (parts[i].Scope is ProseFieldPart { Kind: ProseFieldPartKind.Description }) {
          description = content;
          continue;
        }
        if (parts[i].Scope is ProseFieldPart { Kind: ProseFieldPartKind.Tooltip }) {
          tooltip = content;
          continue;
        }
        delegatedParts.Add(parts[i]);
      }
      label ??= (ref Composition cx) => cx.Text(field.Name);
      if (tooltip != null || description != null) {
        var baseLabel = label;
        var hoverContent = Combine(description, tooltip);
        Composable decoratedLabel = (ref Composition cx) => {
          var context = cx.ReadContext(OptionPageFieldContext.Key, false);
          var hasSidePanel = context.options.hasSidePanel;
          var collapse = context.options.collapseTooltipIntoDescription;
          var content = hasSidePanel ? collapse ? null : tooltip : hoverContent;
          if (content != null && context.options.showTooltipOnLabelHover) {
            OptionPageTooltipElement.ComposeBoundary(ref cx, baseLabel, content);
            return;
          }
          using (cx.Group(Axis.Horizontal, cross: Align.Center)) {
            baseLabel(ref cx);
            if (content != null) {
              cx.Spacing(1);
              OptionPageTooltipElement.ComposeBoundary(ref cx, null, content);
            }
          }
        };
        for (var i = delegatedParts.Count - 1; i >= 0; i--)
          if (delegatedParts[i].Scope is ProseFieldPart { Kind: ProseFieldPartKind.Label })
            delegatedParts.RemoveAt(i);
        delegatedParts.Add(new ComposeProseFieldPart(ProseFields.Label, decoratedLabel));
        label = decoratedLabel;
      }
      var allowDefaultReset = true;
      for (var i = 0; i < modifiers.Count; i++)
        if (modifiers[i] is OptionPageDefaultResetModifier reset)
          allowDefaultReset = reset.Enabled;
      Composable nameActions = (ref Composition cx) => {
        var context = cx.ReadContext(OptionPageFieldContext.Key, false);
        var showChanged = context.options.showChangedOptionReset && context.state?.IsChanged(field.Path) == true;
        var showDefault = allowDefaultReset && context.options.showDefaultOptionReset &&
                          context.state?.IsNonDefault(field.Path) == true;
        if (showChanged) {
          ref var resetChange = ref cx.Button(
            OptionPageFieldElement.ComposeChangedResetIcon,
            action: OptionPageFieldElement.ResetChange,
            style: ThemeProperties.ButtonGhost[in cx]
          );
          resetChange.element.tooltip = "Discard the pending change";
        }
        if (showChanged && showDefault) cx.Spacing(1);
        if (showDefault) {
          ref var resetDefault = ref cx.Button(
            OptionPageFieldElement.ComposeDefaultResetIcon,
            action: OptionPageFieldElement.ResetToDefault,
            style: ThemeProperties.ButtonGhost[in cx]
          );
          resetDefault.element.tooltip = "Reset to the default value";
        }
        if (showChanged || showDefault) cx.Spacing(1);
      };
      if (!ComposeProseFieldFactories.Standard(
            field, formatter, delegatedParts, modifiers, out var fieldContent,
            new InspectorLayoutSlots(nameEnd: nameActions)
          )) {
        result = null;
        return false;
      }
      var presentation = new OptionPageFieldPresentation(label, description, tooltip);
      result = (ref Composition cx) => {
        var context = cx.ReadContext(OptionPageFieldContext.Key, false);
        OptionPageFieldElement.ComposeBoundary(
          ref cx, context.form, context.controller, context.options, context.state, field.Path,
          presentation, fieldContent
        );
      };
      return true;
    }

    private static Composable Combine(Composable first, Composable second) {
      if (first == null) return second;
      if (second == null) return first;
      return (ref Composition cx) => {
        first(ref cx);
        cx.Spacing(1);
        second(ref cx);
      };
    }

  }
}

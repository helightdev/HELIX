using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Prose;
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

    /// <summary>Default-value reset is offered unless the field asked for it to be suppressed.</summary>
    public static bool AllowDefaultReset(this in ProseFieldDecoration decoration) =>
      decoration.Modifier<OptionPageDefaultResetModifier>()?.Enabled ?? true;
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

  [Mixable]
  [BoundaryElementMixin]
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

    [Hook]
    private void OnInit() {
      RegisterCallback<PointerEnterEvent>(OnPointerEnter);
      RegisterCallback<FocusInEvent>(OnFocusIn);
    }

    [Hook]
    private void OnDispose() {
      UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
      UnregisterCallback<FocusInEvent>(OnFocusIn);
      _controller = null;
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      _controller = props.controller;
      using (cx.WriteContext(out var context)) FormContext.Key[context] = new FormContext(props.form);
      style.alignSelf = Align.Stretch;
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

  [Mixable]
  [BoundaryElementMixin]
  public partial class OptionPageTooltipElement {
    public partial struct Props {
      [Prop(null)] public Composable trigger;
      [Prop(null)] public Composable content;
      /// <summary>Shown in place of a trigger. The circled-information sign by default.</summary>
      [Prop("\u24D8")] public string glyph;
      [Prop(0.7f)] public float glyphOpacity;
      [Prop(320f)] public float maxWidth;
    }

    private const float TooltipAnchorGap = 4f;
    private OverlayController _overlays;
    private OverlayHandle _overlay;

    [Hook]
    private void OnInit() {
      RegisterCallback<PointerEnterEvent>(OnPointerEnter);
      RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
    }

    [Hook]
    private void OnDispose() {
      UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
      UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
      _overlay?.Dismiss(OverlayDismissReason.AnchorDetached);
      _overlay = null;
      _overlays = null;
    }

    [Hook]
    private void OnCompose(ref Composition cx) {
      _overlays = cx.Overlays(false);
      style.flexGrow = 0f;
      style.flexShrink = 0f;
      style.alignSelf = Align.Center;
      pickingMode = PickingMode.Position;
      if (props.trigger != null) props.trigger(ref cx);
      else cx.Text(props.glyph, TextRole.BodySmall).Opacity(props.glyphOpacity);
    }

    private void OnPointerEnter(PointerEnterEvent evt) {
      if (props.content == null || _overlays == null || _overlay?.IsOpen == true) return;
      _overlay = Overlay.Build(ComposeTooltip)
        .AnchorTo(this, OverlayPlacement.Above, new Vector2(0f, -TooltipAnchorGap))
        .Show(_overlays, this);
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
        .BorderRadius(theme[RadiusRole.Radius2])
        .MaxWidth(props.maxWidth);
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(theme[SpacingRole.Spacing2]);
        props.content?.Invoke(ref cx);
      }
    }
  }

  /// <summary>Consumes option-page help prose, delegates control creation, then wraps the completed field.</summary>
  public static class OptionPageFieldFactory {
    public static bool Create(
      IProseField field, ref ProseFieldDecoration decoration, out Composable result
    ) {
      var label = decoration.Label ?? ComposeProseFieldFactories.Text(field.Name);
      var description = decoration.Description;
      var tooltip = decoration.Tooltip;

      if (tooltip != null || description != null) {
        var baseLabel = label;
        var hoverContent = Combine(description, tooltip);
        label = (ref Composition cx) => {
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
      }

      // The side panel renders the description itself, so the field row must not repeat it.
      decoration.Label = label;
      decoration.Description = null;
      decoration.Tooltip = null;
      decoration.NameEnd = ComposeResetActions(field.Path, OptionPageFieldModifiers.AllowDefaultReset(in decoration));

      if (!ComposeProseFieldFactories.Default(field, ref decoration, out var fieldContent)) {
        result = null;
        return false;
      }

      var presentation = new OptionPageFieldPresentation(label, description, tooltip);
      var path = field.Path;
      result = (ref Composition cx) => {
        var context = cx.ReadContext(OptionPageFieldContext.Key, false);
        OptionPageFieldElement.ComposeBoundary(
          ref cx, context.form, context.controller, context.options, context.state, path,
          presentation, fieldContent
        );
      };
      return true;
    }

    private static Composable ComposeResetActions(string path, bool allowDefaultReset) =>
      (ref Composition cx) => {
        var context = cx.ReadContext(OptionPageFieldContext.Key, false);
        var showChanged = context.options.showChangedOptionReset && context.state?.IsChanged(path) == true;
        var showDefault = allowDefaultReset && context.options.showDefaultOptionReset &&
                          context.state?.IsNonDefault(path) == true;
        if (showChanged) {
          ref var resetChange = ref cx.Button(
            OptionPageFieldElement.ComposeChangedResetIcon,
            action: OptionPageFieldElement.ResetChange,
            style: ThemeProperties.ButtonGhost[in cx]
          );
          resetChange.element.tooltip = context.options.changedOptionResetTooltip;
        }
        if (showChanged && showDefault) cx.Spacing(1);
        if (showDefault) {
          ref var resetDefault = ref cx.Button(
            OptionPageFieldElement.ComposeDefaultResetIcon,
            action: OptionPageFieldElement.ResetToDefault,
            style: ThemeProperties.ButtonGhost[in cx]
          );
          resetDefault.element.tooltip = context.options.defaultOptionResetTooltip;
        }
        if (showChanged || showDefault) cx.Spacing(1);
      };

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

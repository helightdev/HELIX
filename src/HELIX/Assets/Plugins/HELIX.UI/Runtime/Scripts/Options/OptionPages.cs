using System;
using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Compose.Forms;
using HELIX.Prose;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.UI.Options {
  public enum OptionPagesApplyResult : byte { NothingToApply, Invalid, Applied, ConfirmationRequired }

  public interface IOptionPagesState : IDisposable {
    bool IsDirty { get; }
    bool IsChanged(string path);
    bool IsNonDefault(string path);
    void ResetChange(string path);
    void ResetToDefault(string path);
    OptionPagesApplyResult Apply();
    void Revert();
    void Confirm();
    void Reject();
  }

  [PropStruct] public readonly partial struct OptionPagesOptions {
    public static readonly OptionPagesOptions Default = new(hasSidePanel: true);

    [Prop(true)] public readonly bool hasSidePanel;
    [Prop(false)] public readonly bool collapseTooltipIntoDescription;
    [Prop(false)] public readonly bool showTooltipOnLabelHover;
    [Prop(true)] public readonly bool showChangedOptionReset;
    [Prop(true)] public readonly bool showDefaultOptionReset;
    [Prop(null)] public readonly IconRef? changedOptionResetIcon;
    [Prop(null)] public readonly IconRef? defaultOptionResetIcon;
  }

  public readonly struct OptionPagesNavigationHeaderSpec : ISpec<OptionPagesNavigationHeaderSpec> {
    public readonly NavigationGraph graph;
    public readonly NavigationController controller;
    public readonly Composable<NavigationRoute> label;

    public OptionPagesNavigationHeaderSpec(
      NavigationGraph graph,
      NavigationController controller,
      Composable<NavigationRoute> label
    ) {
      this.graph = graph;
      this.controller = controller;
      this.label = label;
    }

    public ReadComposable<OptionPagesNavigationHeaderSpec> GetDefault(
      in OptionPagesNavigationHeaderSpec spec
    ) => Default;

    public static void Default(ref Composition cx, in OptionPagesNavigationHeaderSpec spec) {
      var label = spec.label;
      using (cx.Row(cross: Align.Stretch)) {
        for (var i = 0; i < spec.graph.Routes.Count; i++) {
          var route = spec.graph.Routes[i];
          cx.NavigationLink(
            route,
            controller: spec.controller,
            content: (ref Composition child) => label?.Invoke(ref child, route),
            style: ThemeProperties.ButtonToggle[in cx]
          );
          if (i + 1 < spec.graph.Routes.Count) cx.Spacing(1);
        }
      }
    }
  }

  public readonly struct OptionPagesHelpPanelSpec : ISpec<OptionPagesHelpPanelSpec> {
    public readonly OptionPageFieldPresentation current;
    public readonly bool collapseTooltipIntoDescription;

    public OptionPagesHelpPanelSpec(
      OptionPageFieldPresentation current,
      bool collapseTooltipIntoDescription
    ) {
      this.current = current;
      this.collapseTooltipIntoDescription = collapseTooltipIntoDescription;
    }

    public ReadComposable<OptionPagesHelpPanelSpec> GetDefault(in OptionPagesHelpPanelSpec spec) => Default;

    public static void Default(ref Composition cx, in OptionPagesHelpPanelSpec spec) {
      using (var help = cx.Column(cross: Align.Stretch, flex: Flex.Shrink(0))) {
        help.With(BoxConstraints.Preferred(new Length(30f, LengthUnit.Percent), StyleKeyword.Auto));
        if (spec.current == null) {
          cx.Text("Field details", TextRole.TitleSmall);
          cx.Spacing(1);
          cx.Text("Focus or point at an option to see more information.", TextRole.BodySmall);
          return;
        }
        spec.current.Label?.Invoke(ref cx);
        var description = spec.current.Description;
        var tooltip = spec.current.Tooltip;
        if (description == null && (!spec.collapseTooltipIntoDescription || tooltip == null)) return;
        cx.Spacing(1);
        description?.Invoke(ref cx);
        if (!spec.collapseTooltipIntoDescription || tooltip == null) return;
        if (description != null) cx.Spacing(1);
        tooltip(ref cx);
      }
    }
  }

  public readonly struct OptionPagesActionBarSpec : ISpec<OptionPagesActionBarSpec> {
    public readonly CompositionAction revert, apply;
    public readonly bool canApply;

    public OptionPagesActionBarSpec(CompositionAction revert, CompositionAction apply, bool canApply) {
      this.revert = revert;
      this.apply = apply;
      this.canApply = canApply;
    }

    public ReadComposable<OptionPagesActionBarSpec> GetDefault(in OptionPagesActionBarSpec spec) => Default;

    public static void Default(ref Composition cx, in OptionPagesActionBarSpec spec) {
      using (cx.Row(main: Justify.FlexEnd, cross: Align.Center)) {
        cx.Button(
          static (ref Composition child) => child.Text("Revert"),
          action: spec.revert,
          style: ThemeProperties.ButtonGhost[in cx]
        );
        cx.Spacing(1);
        cx.Button(static (ref Composition child) => child.Text("Apply"), spec.apply, enabled: spec.canApply);
      }
    }
  }

  public readonly struct OptionPagesConfirmationSpec : ISpec<OptionPagesConfirmationSpec> {
    public readonly CompositionAction reject, confirm;

    public OptionPagesConfirmationSpec(CompositionAction reject, CompositionAction confirm) {
      this.reject = reject;
      this.confirm = confirm;
    }

    public ReadComposable<OptionPagesConfirmationSpec> GetDefault(in OptionPagesConfirmationSpec spec) => Default;

    public static void Default(ref Composition cx, in OptionPagesConfirmationSpec spec) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR.BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainer))
        .TextColor(theme.GetColor(ColorRoles.OnSurfaceContainer)).BorderRadius(12f);
      using (cx.Column(cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(20f);
        cx.Text("Keep these settings?", TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text(
          "The settings have been applied. Confirm to save them, or revert to restore the previous values.",
          TextRole.BodySmall
        );
        cx.Spacing(2);
        using (cx.Row(main: Justify.FlexEnd, cross: Align.Center)) {
          cx.Button(
            static (ref Composition child) => child.Text("Revert"),
            action: spec.reject,
            style: ThemeProperties.ButtonGhost[in cx]
          );
          cx.Spacing(1);
          cx.Button(static (ref Composition child) => child.Text("Keep"), action: spec.confirm);
        }
      }
    }
  }

  public readonly struct OptionPageSectionHeaderSpec : ISpec<OptionPageSectionHeaderSpec> {
    public readonly string title, description;
    public readonly TextRole role;
    public readonly IconRef? icon;

    public OptionPageSectionHeaderSpec(string title, string description, TextRole role, IconRef? icon) {
      this.title = title;
      this.description = description;
      this.role = role;
      this.icon = icon;
    }

    public ReadComposable<OptionPageSectionHeaderSpec> GetDefault(in OptionPageSectionHeaderSpec spec) => Default;

    public static void Default(ref Composition cx, in OptionPageSectionHeaderSpec spec) {
      using (cx.Row(cross: Align.Center)) {
        if (spec.icon.HasValue) {
          spec.icon.Value.Compose(ref cx);
          cx.Spacing(1);
        }
        cx.Text(spec.title, spec.role);
      }
      if (string.IsNullOrEmpty(spec.description)) return;
      cx.Spacing(1);
      cx.Text(spec.description, TextRole.BodySmall);
    }
  }

  [EnableMixins]
  [BoundaryComposableMixin(cacheLookups: true)]
  public partial class OptionPagesElement {
    public partial struct Props {
      [Prop(null, Equatable = false)] public OptionPages pages;
    }

    protected override void OnRecompose(ref Composition cx) {
      Node.style.flexGrow = 1f;
      Node.style.flexShrink = 1f;
      Node.style.alignSelf = Align.Stretch;
      props.pages?.ComposeBoundaryContent(ref cx);
    }
  }

  /// <summary>Projects path-sectioned option content into category navigation and structured option pages.</summary>
  public sealed class OptionPages : IDisposable {
    private readonly NavTreeProse _model;
    private readonly OptionPagesOptions _options;
    private readonly Dictionary<NavTreeProse.Node, Composable> _content = new();
    private readonly FormController _form = new();
    private readonly OptionPageFieldController _fieldHelp = new();
    private readonly OverlayController _overlays = new();
    private readonly NavigationGraph _graph;
    private readonly NavigationController _controller;
    private readonly IOptionPagesState _state;
    private readonly CompositionAction _applyAction;
    private readonly CompositionAction _revertAction;
    private readonly CompositionAction _confirmAction;
    private readonly CompositionAction _rejectAction;
    private readonly Composable<OverlayContextData> _confirmation;
    private readonly Composable<NavigationRoute> _navigationLabel;

    public FormController Form => _form;

    public OptionPages(
      NavTreeProse model,
      OptionPagesOptions? options = null,
      IOptionPagesState state = null
    ) {
      _model = model ?? throw new ArgumentNullException(nameof(model));
      _options = options ?? OptionPagesOptions.Default;
      _state = state;
      _applyAction = context => {
        if (_state.Apply() != OptionPagesApplyResult.ConfirmationRequired) return;
        Overlay.Build(_confirmation)
          .Modal()
          .DismissOnCancel(false)
          .Constraints(BoxConstraints.Only(min: new StyleLength2(360f, 0f)))
          .Show(context);
      };
      _revertAction = _ => _state.Revert();
      _confirmAction = context => {
        _state.Confirm();
        context.OverlayEntry()?.Dismiss();
      };
      _rejectAction = context => {
        _state.Reject();
        context.OverlayEntry()?.Dismiss();
      };
      _confirmation = (ref Composition cx, OverlayContextData _) =>
        cx.Spec(new OptionPagesConfirmationSpec(_rejectAction, _confirmAction));
      _navigationLabel = (ref Composition cx, NavigationRoute route) => {
        var presentation = Presentation(_model.Root.Children[route.Index]);
        cx.Spec(
          new OptionPageSectionHeaderSpec(
            presentation.title,
            null,
            TextRole.LabelLarge,
            presentation.icon
          )
        );
      };
      if (model.Root.Children.Count == 0)
        throw new ArgumentException("Option pages require at least one root category.", nameof(model));

      BakeContent(model.Root);
      var builder = NavigationGraph.Builder(_model.Paths.Format(model.Root.Children[0].Path));
      for (var i = 0; i < model.Root.Children.Count; i++) {
        var section = model.Root.Children[i];
        builder.Route(
          _model.Paths.Format(section.Path),
          NavigationPage.Build((ref Composition cx, NavigationContextData _) => ComposePage(ref cx, section))
            .Name(Presentation(section).title)
            .Transition(NavigationTransitions.Instant)
        );
      }
      _graph = builder.Build();
      _controller = new NavigationController(_graph);
    }

    public void Compose(ref Composition cx) => OptionPagesElement.ComposeBoundary(ref cx, this);

    internal void ComposeBoundaryContent(ref Composition cx) {
      using (cx.WriteContext(out var context)) {
        FormContext.Key[context] = new FormContext(_form);
        OptionPageFieldContext.Key[context] = new OptionPageFieldContext(_fieldHelp, _form, _options, _state);
      }
      cx.SubscribeTo(_form);
      cx.SubscribeTo(_controller);
      cx.SubscribeTo(_fieldHelp);

      using (cx.OverlayHost(_overlays))
      using (cx.Group(FlexGroup.Column(cross: Align.Stretch), Flex.FillFlexible())) {
        if (cx.CursorDirty) cx.CURSOR.Fill();
        cx.Spec(new OptionPagesNavigationHeaderSpec(_graph, _controller, _navigationLabel));
        cx.Spacing(2);
        using (cx.Group(FlexGroup.Row(cross: Align.Stretch), Flex.FillFlexible())) {
          cx.NavigationHost(_graph, _controller, NavigationTransitions.Instant, NavigationHostBehavior.None)
            .Flexible();
          if (_options.hasSidePanel) {
            // TODO: Proper conditionals
            cx.Spacing(3);
            cx.Spec(
              new OptionPagesHelpPanelSpec(
                _fieldHelp.Current,
                _options.collapseTooltipIntoDescription
              )
            );
          }
        }
        if (_state?.IsDirty == true) {
          cx.Spacing(2);
          cx.Spec(new OptionPagesActionBarSpec(_revertAction, _applyAction, !_form.HasErrors));
        }
      }
    }

    public void Dispose() {
      _state?.Dispose();
      _controller.Dispose();
      _form.Dispose();
      _fieldHelp.Dispose();
      _overlays.Dispose();
    }

    private void ComposePage(ref Composition cx, NavTreeProse.Node node) {
      using (cx.ScrollView())
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.AlignSelf(Align.Stretch);
        var presentation = Presentation(node);
        cx.Spec(
          new OptionPageSectionHeaderSpec(
            presentation.title,
            presentation.description,
            presentation.role ?? TextRole.TitleMedium,
            presentation.icon
          )
        );
        cx.Spacing(2);
        _content[node](ref cx);
        for (var i = 0; i < node.Children.Count; i++) {
          cx.Spacing(3);
          ComposeSubcategory(ref cx, node.Children[i]);
        }
      }
    }

    private void ComposeSubcategory(ref Composition cx, NavTreeProse.Node node) {
      var presentation = Presentation(node);
      cx.Spec(
        new OptionPageSectionHeaderSpec(
          presentation.title,
          presentation.description,
          presentation.role ?? TextRole.TitleSmall,
          presentation.icon
        )
      );
      cx.Spacing(1);
      _content[node](ref cx);
      for (var i = 0; i < node.Children.Count; i++) {
        cx.Spacing(2);
        ComposeSubcategory(ref cx, node.Children[i]);
      }
    }

    private readonly struct SectionPresentation {
      public readonly string title, description;
      public readonly TextRole? role;
      public readonly IconRef? icon;

      public SectionPresentation(string title, string description, TextRole? role, IconRef? icon) {
        this.title = title;
        this.description = description;
        this.role = role;
        this.icon = icon;
      }
    }

    private static SectionPresentation Presentation(NavTreeProse.Node node) {
      var title = node.Name;
      string description = null;
      TextRole? role = null;
      IconRef? icon = null;
      for (var i = 0; i < node.Modifiers.Count; i++) {
        if (node.Modifiers[i] is not PathSectionPresentationModifier modifier) continue;
        if (modifier.Title != null) title = modifier.Title;
        if (modifier.Description != null) description = modifier.Description;
        if (modifier.TitleRole.HasValue) role = modifier.TitleRole;
        if (modifier.Icon.HasValue) icon = modifier.Icon;
      }
      return new SectionPresentation(title, description, role, icon);
    }

    private void BakeContent(NavTreeProse.Node node) {
      var axis = Axis.Vertical;
      var main = Justify.FlexStart;
      var cross = Align.Stretch;
      var gap = 8f;
      var reverse = false;
      var clear = false;
      for (var i = 0; i < node.Modifiers.Count; i++) {
        if (node.Modifiers[i] is not ComposeFlexModifier modifier) continue;
        if (modifier.Axis.HasValue) axis = modifier.Axis.Value;
        if (modifier.Main.HasValue) main = modifier.Main.Value;
        if (modifier.Cross.HasValue) cross = modifier.Cross.Value;
        if (modifier.Gap.HasValue) gap = modifier.Gap.Value;
        if (modifier.Reverse.HasValue) reverse = modifier.Reverse.Value;
        if (modifier.Clear.HasValue) clear = modifier.Clear.Value;
      }
      _content.Add(node, HXBaker.Flex(node.Entries, axis, main, cross, gap, reverse, clear));
      for (var i = 0; i < node.Children.Count; i++) BakeContent(node.Children[i]);
    }
  }
}
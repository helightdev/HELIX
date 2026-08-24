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

  [BoundaryComposable(Extension = false, UseLookupCache = true)]
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

    public FormController Form => _form;

    public OptionPages(
      NavTreeProse model, OptionPagesOptions? options = null, IOptionPagesState state = null
    ) {
      _model = model ?? throw new ArgumentNullException(nameof(model));
      _options = options ?? OptionPagesOptions.Default;
      _state = state;
      _applyAction = Apply;
      _revertAction = Revert;
      _confirmAction = Confirm;
      _rejectAction = Reject;
      _confirmation = ComposeConfirmation;
      if (model.Root.Children.Count == 0)
        throw new ArgumentException("Option pages require at least one root category.", nameof(model));

      BakeContent(model.Root);
      var builder = NavigationGraph.Builder(Route(model.Root.Children[0]));
      for (var i = 0; i < model.Root.Children.Count; i++) {
        var section = model.Root.Children[i];
        builder.Route(
          Route(section),
          NavigationPage.Build((ref Composition cx, NavigationContextData _) => ComposePage(ref cx, section))
            .Name(Title(section))
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
        using (cx.Group(Axis.Horizontal, cross: Align.Stretch)) {
          for (var i = 0; i < _model.Root.Children.Count; i++) {
            var section = _model.Root.Children[i];
            cx.NavigationLink(
              _graph.GetRoute(Route(section)),
              controller: _controller,
              content: (ref Composition child) => ComposeTitle(
                ref child, section, TextRole.LabelLarge, useConfiguredRole: false
              ),
              style: ThemeProperties.ButtonToggle[in cx]
            );
            if (i + 1 < _model.Root.Children.Count) cx.Spacing(1);
          }
        }
        cx.Spacing(2);
        using (cx.Group(FlexGroup.Row(cross: Align.Stretch), Flex.FillFlexible())) {
          cx.NavigationHost(_graph, _controller, NavigationTransitions.Instant, NavigationHostBehavior.None)
            .Flexible();
          if (_options.hasSidePanel) { // TODO: Proper conditionals
            cx.Spacing(3);
            using (var help = cx.Group(Axis.Vertical, cross: Align.Stretch)) {
              help.With(Flex.Shrink(0));
              help.With(BoxConstraints.Preferred(new Length(30f, LengthUnit.Percent), StyleKeyword.Auto));
              var current = _fieldHelp.Current;
              if (current == null) { // TODO: Proper conditionals
                cx.Text("Field details", TextRole.TitleSmall);
                cx.Spacing(1);
                cx.Text("Focus or point at an option to see more information.", TextRole.BodySmall);
              } else {
                current.Label?.Invoke(ref cx);
                var details = _options.collapseTooltipIntoDescription
                  ? Combine(current.Description, current.Tooltip)
                  : current.Description;
                if (details != null) { // TODO: Proper conditionals
                  cx.Spacing(1);
                  details(ref cx);
                }
              }
            }
          }
        }
        if (_state?.IsDirty == true) {
          cx.Spacing(2);
          using (cx.Group(Axis.Horizontal, main: Justify.FlexEnd, cross: Align.Center)) {
            cx.Button(
              static (ref Composition child) => child.Text("Revert"),
              action: _revertAction,
              style: ThemeProperties.ButtonGhost[in cx]
            );
            cx.Spacing(1);
            cx.Button(
              static (ref Composition child) => child.Text("Apply"),
              action: _applyAction,
              enabled: !_form.HasErrors
            );
          }
        }
      }
    }

    private void Apply(CompositionContext context) {
      if (_state.Apply() != OptionPagesApplyResult.ConfirmationRequired) return;
      Overlay.Build(_confirmation)
        .Modal()
        .DismissOnCancel(false)
        .Constraints(BoxConstraints.Only(min: new StyleLength2(360f, 0f)))
        .Show(context);
    }

    private void Revert(CompositionContext _) => _state.Revert();

    private void Confirm(CompositionContext context) {
      _state.Confirm();
      context.OverlayEntry()?.Dismiss();
    }

    private void Reject(CompositionContext context) {
      _state.Reject();
      context.OverlayEntry()?.Dismiss();
    }

    private void ComposeConfirmation(ref Composition cx, OverlayContextData _) {
      var theme = cx.ReadContextOrDefault(ThemeData.Key, HXThemes.DefaultDark);
      cx.CURSOR
        .BackgroundColor(theme.GetColor(ColorRoles.SurfaceContainer))
        .TextColor(theme.GetColor(ColorRoles.OnSurfaceContainer))
        .BorderRadius(12f);
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.Padding(20f);
        cx.Text("Keep these settings?", TextRole.TitleMedium);
        cx.Spacing(1);
        cx.Text("The settings have been applied. Confirm to save them, or revert to restore the previous values.",
          TextRole.BodySmall);
        cx.Spacing(2);
        using (cx.Group(Axis.Horizontal, main: Justify.FlexEnd, cross: Align.Center)) {
          cx.Button(
            static (ref Composition child) => child.Text("Revert"),
            action: _rejectAction,
            style: ThemeProperties.ButtonGhost[in cx]
          );
          cx.Spacing(1);
          cx.Button(static (ref Composition child) => child.Text("Keep"), action: _confirmAction);
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

    private string Route(NavTreeProse.Node node) => _model.Paths.Format(node.Path);

    private void ComposePage(ref Composition cx, NavTreeProse.Node node) {
      using (cx.ScrollView())
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.AlignSelf(Align.Stretch);
        ComposeTitle(ref cx, node, TextRole.TitleMedium);
        var presentation = Presentation(node);
        if (!string.IsNullOrEmpty(presentation.description)) {
          cx.Spacing(1);
          cx.Text(presentation.description, TextRole.BodySmall);
        }
        cx.Spacing(2);
        ComposeEntries(ref cx, node);
        for (var i = 0; i < node.Children.Count; i++) {
          cx.Spacing(3);
          ComposeSubcategory(ref cx, node.Children[i]);
        }
      }
    }

    private void ComposeSubcategory(ref Composition cx, NavTreeProse.Node node) {
      ComposeTitle(ref cx, node, TextRole.TitleSmall);
      var presentation = Presentation(node);
      if (!string.IsNullOrEmpty(presentation.description)) {
        cx.Spacing(1);
        cx.Text(presentation.description, TextRole.BodySmall);
      }
      cx.Spacing(1);
      ComposeEntries(ref cx, node);
      for (var i = 0; i < node.Children.Count; i++) {
        cx.Spacing(2);
        ComposeSubcategory(ref cx, node.Children[i]);
      }
    }

    private void ComposeEntries(ref Composition cx, NavTreeProse.Node node) => _content[node](ref cx);

    private readonly struct SectionPresentation {
      public readonly string title, description;
      public readonly TextRole? role;
      public readonly IconRef? icon;
      public SectionPresentation(string title, string description, TextRole? role, IconRef? icon) {
        this.title = title; this.description = description; this.role = role; this.icon = icon;
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

    private static string Title(NavTreeProse.Node node) => Presentation(node).title;

    private static Composable Combine(Composable first, Composable second) {
      if (first == null) return second;
      if (second == null) return first;
      return (ref Composition cx) => {
        first(ref cx);
        cx.Spacing(1);
        second(ref cx);
      };
    }

    private static void ComposeTitle(
      ref Composition cx, NavTreeProse.Node node, TextRole fallback,
      bool useConfiguredRole = true
    ) {
      var presentation = Presentation(node);
      using (cx.Group(Axis.Horizontal, cross: Align.Center)) {
        if (presentation.icon.HasValue) {
          presentation.icon.Value.Compose(ref cx);
          cx.Spacing(1);
        }
        cx.Text(presentation.title, useConfiguredRole ? presentation.role ?? fallback : fallback);
      }
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

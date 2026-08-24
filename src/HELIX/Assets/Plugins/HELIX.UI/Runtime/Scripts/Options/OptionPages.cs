using System;
using System.Collections.Generic;
using HELIX.Compose;
using HELIX.Compose.Forms;
using HELIX.Prose;
using HELIX.Theming;
using HELIX.Types;
using UnityEngine.UIElements;

namespace HELIX.UI.Options {
  [PropStruct] public readonly partial struct OptionPagesOptions {
    public static readonly OptionPagesOptions Default = new(hasSidePanel: true);

    [Prop(true)] public readonly bool hasSidePanel;
    [Prop(false)] public readonly bool collapseTooltipIntoDescription;
    [Prop(false)] public readonly bool showTooltipOnLabelHover;
  }

  [BoundaryComposable(Extension = false)]
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
    private readonly PathSectionedProse _model;
    private readonly OptionPagesOptions _options;
    private readonly Dictionary<PathSectionedProse.Section, Composable> _content = new();
    private readonly FormController _form = new();
    private readonly OptionPageFieldController _fieldHelp = new();
    private readonly OverlayController _overlays = new();
    private readonly NavigationGraph _graph;
    private readonly NavigationController _controller;

    public FormController Form => _form;

    public OptionPages(PathSectionedProse model, OptionPagesOptions? options = null) {
      _model = model ?? throw new ArgumentNullException(nameof(model));
      _options = options ?? OptionPagesOptions.Default;
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
        OptionPageFieldContext.Key[context] = new OptionPageFieldContext(_fieldHelp, _form, _options);
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
      }
    }

    public void Dispose() {
      _controller.Dispose();
      _form.Dispose();
      _fieldHelp.Dispose();
      _overlays.Dispose();
    }

    private string Route(PathSectionedProse.Section section) => _model.Paths.Format(section.Path);

    private void ComposePage(ref Composition cx, PathSectionedProse.Section section) {
      using (cx.ScrollView())
      using (cx.Group(Axis.Vertical, cross: Align.Stretch)) {
        if (cx.CursorDirty) cx.CURSOR.AlignSelf(Align.Stretch);
        ComposeTitle(ref cx, section, TextRole.TitleMedium);
        var presentation = Presentation(section);
        if (!string.IsNullOrEmpty(presentation.description)) {
          cx.Spacing(1);
          cx.Text(presentation.description, TextRole.BodySmall);
        }
        cx.Spacing(2);
        ComposeEntries(ref cx, section);
        for (var i = 0; i < section.Children.Count; i++) {
          cx.Spacing(3);
          ComposeSubcategory(ref cx, section.Children[i]);
        }
      }
    }

    private void ComposeSubcategory(ref Composition cx, PathSectionedProse.Section section) {
      ComposeTitle(ref cx, section, TextRole.TitleSmall);
      var presentation = Presentation(section);
      if (!string.IsNullOrEmpty(presentation.description)) {
        cx.Spacing(1);
        cx.Text(presentation.description, TextRole.BodySmall);
      }
      cx.Spacing(1);
      ComposeEntries(ref cx, section);
      for (var i = 0; i < section.Children.Count; i++) {
        cx.Spacing(2);
        ComposeSubcategory(ref cx, section.Children[i]);
      }
    }

    private void ComposeEntries(ref Composition cx, PathSectionedProse.Section section) => _content[section](ref cx);

    private readonly struct SectionPresentation {
      public readonly string title, description;
      public readonly TextRole? role;
      public readonly IconRef? icon;
      public SectionPresentation(string title, string description, TextRole? role, IconRef? icon) {
        this.title = title; this.description = description; this.role = role; this.icon = icon;
      }
    }

    private static SectionPresentation Presentation(PathSectionedProse.Section section) {
      var title = section.Name;
      string description = null;
      TextRole? role = null;
      IconRef? icon = null;
      for (var i = 0; i < section.Modifiers.Count; i++) {
        if (section.Modifiers[i] is not PathSectionPresentationModifier modifier) continue;
        if (modifier.Title != null) title = modifier.Title;
        if (modifier.Description != null) description = modifier.Description;
        if (modifier.TitleRole.HasValue) role = modifier.TitleRole;
        if (modifier.Icon.HasValue) icon = modifier.Icon;
      }
      return new SectionPresentation(title, description, role, icon);
    }

    private static string Title(PathSectionedProse.Section section) => Presentation(section).title;

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
      ref Composition cx, PathSectionedProse.Section section, TextRole fallback,
      bool useConfiguredRole = true
    ) {
      var presentation = Presentation(section);
      using (cx.Group(Axis.Horizontal, cross: Align.Center)) {
        if (presentation.icon.HasValue) {
          presentation.icon.Value.Compose(ref cx);
          cx.Spacing(1);
        }
        cx.Text(presentation.title, useConfiguredRole ? presentation.role ?? fallback : fallback);
      }
    }

    private void BakeContent(PathSectionedProse.Section section) {
      var axis = Axis.Vertical;
      var main = Justify.FlexStart;
      var cross = Align.Stretch;
      var gap = 8f;
      var reverse = false;
      var clear = false;
      for (var i = 0; i < section.Modifiers.Count; i++) {
        if (section.Modifiers[i] is not ComposeFlexModifier modifier) continue;
        if (modifier.Axis.HasValue) axis = modifier.Axis.Value;
        if (modifier.Main.HasValue) main = modifier.Main.Value;
        if (modifier.Cross.HasValue) cross = modifier.Cross.Value;
        if (modifier.Gap.HasValue) gap = modifier.Gap.Value;
        if (modifier.Reverse.HasValue) reverse = modifier.Reverse.Value;
        if (modifier.Clear.HasValue) clear = modifier.Clear.Value;
      }
      _content.Add(section, HXBaker.Flex(section.Entries, axis, main, cross, gap, reverse, clear));
      for (var i = 0; i < section.Children.Count; i++) BakeContent(section.Children[i]);
    }
  }
}
